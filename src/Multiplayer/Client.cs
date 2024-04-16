using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection.Metadata;
using Network;
using Network.Enums;
using Raylib_cs;

namespace PixelColor;

class NetClient {
    enum ClientState {
        WAITING_FOR_CONNECTION,
        WAITING_FOR_USERNAME,
        ASKING_FOR_USERNAME,
        ACTIVE
    }

    ClientConnectionContainer container;

    ClientState state = ClientState.WAITING_FOR_CONNECTION;

    int UUID = -1;
    string username;

    Dictionary<string, NetPlayer> players = [];
    ConcurrentQueue<PlayerSetTilePacket> tileChangeQueue = [];

    public NetClient(string address, int port, string username) {
        Console.WriteLine($"[ Client ] Start netclient with address {address} on port {port}");

        container = ConnectionFactory.CreateClientConnectionContainer(address, port);
        container.ConnectionEstablished += connectionEstablished;
        container.ConnectionLost += connectionLost;
        container.AutoReconnect = false;

        this.username = username;
    }

    public void disconnect() {
        container.Shutdown(CloseReason.ClientClosed);
        container.Dispose();

        Game.client = null;
    }

    public void update(Level level) {
        while (tileChangeQueue.TryDequeue(out PlayerSetTilePacket packet)) {
            level.trySetTile(packet.x, packet.y, packet.color);
        }
    }

    public void draw(Level level) {
        foreach ((string username, NetPlayer player) in players) {
            Vector2 screenPos = level.tileSpaceToScreenSpace(player.position);

            Raylib.DrawTexture(
                Game.mouseCursor, (int)screenPos.X, (int)screenPos.Y, Color.White);

            Raylib.DrawTextEx(Game.font, username, screenPos - new Vector2(0, 20) + new Vector2(2, 0), 20, 1, Color.White);
            Raylib.DrawTextEx(Game.font, username, screenPos - new Vector2(0, 20) + new Vector2(-2, 0), 20, 1, Color.White);
            Raylib.DrawTextEx(Game.font, username, screenPos - new Vector2(0, 20) + new Vector2(0, 2), 20, 1, Color.White);
            Raylib.DrawTextEx(Game.font, username, screenPos - new Vector2(0, 20) + new Vector2(0, -2), 20, 1, Color.White);
            Raylib.DrawTextEx(Game.font, username, screenPos - new Vector2(0, 20), 20, 1, Color.Black);
        }
    }

    void connectionEstablished(Connection connection, ConnectionType type) {
        // if (!container.IsAlive) { return; }

        Console.WriteLine($"[ Client ] {type} Connection established");

        // Ignore non-tcp connections
        // if (type != ConnectionType.TCP) { return; }
        if (type != ConnectionType.UDP) { return; }

        connection.TIMEOUT = int.MaxValue;

        container.RegisterPacketHandler<UsernameConfirmClientPacket>(usernameConfirm, this);
        container.RegisterPacketHandler<UsernameDenyClientPacket>(usernameDeny, this);
        container.RegisterPacketHandler<GameStartPacket>(gameStart, this);
        container.RegisterPacketHandler<GameEndPacket>(gameEnd, this);
        container.RegisterPacketHandler<PlayerJoinClientPacket>(playerJoin, this);
        container.RegisterPacketHandler<PlayerLeaveClientPacket>(playerLeave, this);
        container.RegisterPacketHandler<PlayerMoveClientPacket>(playerMove, this);
        container.RegisterPacketHandler<PlayerSetTilePacket>(playerSetTile, this);

        sendUsername(username);
    }

    void connectionLost(Connection connection, ConnectionType type, CloseReason reason) {
        Console.WriteLine($"[ Client ] Connection lost, reason {reason}");

        MainMenu? menu = Game.getMainMenu();
        if (menu == null) {
            Game.switchSceneMainMenu("joinDisconnect");
        }
        else {
            menu.clientDisconnect();
        }
    }

    public void sendUsername(string username) {
        this.username = username;

        Console.WriteLine($"[ Client ] Sending temp username {username}");
        container.Send(new UsernameRequestServerPacket(username));
    }

    public void sendLevelData(string data) {
        container.Send(new GameStartPacket(data));
    }

    public void sendLevelEnd() {
        container.Send(new GameEndPacket());
    }

    public void sendPlayerMove(Vector2 position) {
        container.Send(new PlayerMoveServerPacket(UUID, position.X, position.Y));
    }

    public void sendSetTile(int x, int y, int color, long timestamp) {
        container.Send(new PlayerSetTilePacket(x, y, color, timestamp));
    }

    void usernameConfirm(UsernameConfirmClientPacket packet, Connection connection) {
        Console.WriteLine("[ Client ] Username accepted");

        UUID = packet.UUID;

        Game.getMainMenu()?.clientJoinSucceed();
    }

    void usernameDeny(UsernameDenyClientPacket packet, Connection connection) {
        Console.WriteLine("[ Client ] Username denied");

        Game.getMainMenu()?.clientUsernameFail();
    }

    void gameStart(GameStartPacket packet, Connection connection) {
        Console.WriteLine("[ Client ] Game Start Recieved");

        Game.switchSceneLevelRemote(packet.data);
    }

    void gameEnd(GameEndPacket packet, Connection connection) {
        Console.WriteLine("[ Client ] Game End Recieved");

        Game.switchSceneMainMenu("joinSucceed");
    }


    void playerJoin(PlayerJoinClientPacket packet, Connection connection) {
        Console.WriteLine($"[ Client ] New user {packet.username} joined");

        players.Add(packet.username, new NetPlayer(packet.username));
    }

    void playerLeave(PlayerLeaveClientPacket packet, Connection connection) {
        Console.WriteLine($"[ Client ] User {packet.username} left");

        players.Remove(packet.username);
    }

    void playerMove(PlayerMoveClientPacket packet, Connection connection) {
        players[packet.username]?.setPosition(new Vector2(packet.x, packet.y));
    }

    void playerSetTile(PlayerSetTilePacket packet, Connection connection) {
        tileChangeQueue.Enqueue(packet);
    }
}