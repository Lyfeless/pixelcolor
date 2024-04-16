using System.Numerics;

namespace PixelColor;

struct Vector2i(int x, int y) {
    public int X = x;
    public int Y = y;

    public static Vector2i Zero { get; } = new(0, 0);
    public static Vector2i Negative { get; } = new(-1, -1);

    public static Vector2i operator +(Vector2i left, Vector2i right) {
        return new(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2i operator -(Vector2i left, Vector2i right) {
        return new(left.X - right.X, left.Y - right.Y);
    }

    public static bool operator ==(Vector2i left, Vector2i right) {
        return left.X == right.X && left.Y == right.Y;
    }

    public static bool operator !=(Vector2i left, Vector2i right) {
        return left.X != right.X && left.Y != right.Y;
    }

    public override string ToString() {
        return X + ", " + Y;
    }
}