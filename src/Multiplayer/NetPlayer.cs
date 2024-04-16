using System.Numerics;

namespace PixelColor;

class NetPlayer(string username) {
    public string username { get; private set; } = username;
    public Vector2 position { get; private set; } = Vector2.Zero;

    public void setPosition(Vector2 position) {
        this.position = position;
    }
}