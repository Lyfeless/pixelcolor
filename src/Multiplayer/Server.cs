using Network;
using Network.Enums;
using Network.Packets;

namespace PixelColor;

class NetServer {
    ServerConnectionContainer container;

    List<ServerConnection> playerConnections = [];

    Random rand = new();

    bool acceptingConnections;

    public NetServer(int port) {
        Console.WriteLine($"[ SERVER ] Start netserver on port {port}");
        acceptingConnections = true;

        container = ConnectionFactory.CreateServerConnectionContainer(port, false);

        container.ConnectionEstablished += connectionEstablished;
        container.ConnectionLost += connectionLost;

        // container.AllowUDPConnections = false;

        container.Start();
    }

    public void shutdown() {
        container.CloseConnections(CloseReason.ServerClosed);
        container.Stop();

        Game.server = null;
    }

    void connectionEstablished(Connection connection, ConnectionType type) {
        if (!acceptingConnections) {
            // There doesn't seem to be a proper close reason for this one sadly
            connection.Close(CloseReason.NetworkError);
        }

        Console.WriteLine($"[ SERVER ] {type} Connection established");

        connection.TIMEOUT = int.MaxValue;

        connection.RegisterPacketHandler<UsernameRequestServerPacket>(usernameRequest, this);
        connection.RegisterPacketHandler<GameStartPacket>(gameStart, this);
        connection.RegisterPacketHandler<GameEndPacket>(gameEnd, this);
        connection.RegisterPacketHandler<PlayerMoveServerPacket>(playerMove, this);
        connection.RegisterPacketHandler<PlayerSetTilePacket>(playerSetTile, this);
    }

    void connectionLost(Connection connection, ConnectionType type, CloseReason reason) {
        ServerConnection? leavingPlayer = findByConnection(connection);
        if (leavingPlayer == null) { return; }

        sendAll(new PlayerLeaveClientPacket(leavingPlayer.username));
        playerConnections.Remove(leavingPlayer);
    }

    void usernameRequest(UsernameRequestServerPacket packet, Connection connection) {
        if (usernameExists(packet.username)) {
            connection.Send(new UsernameDenyClientPacket());
        }
        else {
            // Give player a UUID to send requests through along with a confirmation that they're allowed into the server
            int UUID;
            do {
                UUID = rand.Next();
            } while (UUIDExists(UUID));

            connection.Send(new UsernameConfirmClientPacket(UUID));

            // Send all currently existing players to the new user
            foreach (ServerConnection player in playerConnections) {
                connection.Send(new PlayerJoinClientPacket(player.username));
            }

            // Add new entry and inform all other players of new connection
            playerConnections.Add(new ServerConnection(connection, UUID, packet.username));

            sendAll(new PlayerJoinClientPacket(packet.username), connection);
        }
    }

    void gameStart(GameStartPacket packet, Connection connection) {
        acceptingConnections = false;

        sendAll(packet, connection);
    }

    void gameEnd(GameEndPacket packet, Connection connection) {
        acceptingConnections = true;

        sendAll(packet, connection);
    }

    void playerMove(PlayerMoveServerPacket packet, Connection connection) {
        ServerConnection? player = findByUUID(packet.UUID);
        if (player == null) { return; }

        string username = player.username;

        sendAll(new PlayerMoveClientPacket(username, packet.x, packet.y), connection);
    }

    void playerSetTile(PlayerSetTilePacket packet, Connection connection) {
        sendAll(packet);
    }

    ServerConnection? findByConnection(Connection connection) {
        return playerConnections.FirstOrDefault(e => e.connection == connection);
    }

    ServerConnection? findByUUID(int UUID) {
        return playerConnections.FirstOrDefault(e => e.UUID == UUID);
    }

    ServerConnection? findByUsername(string username) {
        return playerConnections.FirstOrDefault(e => e.username == username);
    }

    bool UUIDExists(int UUID) {
        return playerConnections.Any(e => e.UUID == UUID);
    }

    bool usernameExists(string username) {
        return playerConnections.Any(e => e.username == username);
    }

    void sendAll(Packet packet, Connection? ignore = null) {
        for (int i = 0; i < playerConnections.Count; ++i) {
            if (playerConnections[i].connection != ignore) {
                playerConnections[i].connection.Send(packet);
            }
        }
    }
}