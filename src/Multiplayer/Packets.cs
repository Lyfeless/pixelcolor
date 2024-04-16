using System.Numerics;
using Network.Packets;

namespace PixelColor;

class UsernameRequestServerPacket(string username) : Packet {
    public string username { get; set; } = username;
}

class UsernameConfirmClientPacket(int UUID) : Packet {
    public int UUID { get; set; } = UUID;
}

class UsernameDenyClientPacket() : Packet { }

class GameStartPacket(string data) : Packet {
    public string data { get; set; } = data;
}

class GameEndPacket() : Packet { }

class PlayerJoinClientPacket(string username) : Packet {
    public string username { get; set; } = username;
}

class PlayerLeaveClientPacket(string username) : Packet {
    public string username { get; set; } = username;
}

class PlayerMoveServerPacket(int UUID, float x, float y) : Packet {
    public int UUID { get; set; } = UUID;
    public float x { get; set; } = x;
    public float y { get; set; } = y;
}

class PlayerMoveClientPacket(string username, float x, float y) : Packet {
    public string username { get; set; } = username;
    public float x { get; set; } = x;
    public float y { get; set; } = y;
}

class PlayerSetTilePacket(int x, int y, int color, long timestamp) : Packet {
    public int x { get; set; } = x;
    public int y { get; set; } = y;
    public int color { get; set; } = color;
    public long timestamp { get; set; } = timestamp;
}