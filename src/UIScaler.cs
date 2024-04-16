using System.Numerics;
using Raylib_cs;

namespace PixelColor;

static class UIScaler {
    static readonly float tileDivision = 10;

    public static Vector2i bounds { get; private set; } = new(0, 0);
    public static Vector2 center { get; private set; }
    public static int tileSize { get; private set; }
    public static Vector2 tileCount { get; private set; }

    public static void update() {
        int newScreenWidth = Raylib.GetScreenWidth();
        int newScreenHeight = Raylib.GetScreenHeight();

        if (newScreenWidth != bounds.X || newScreenHeight != bounds.Y) {
            resize(newScreenWidth, newScreenHeight);
        }
    }

    static void resize(int width, int height) {
        bounds = new(width, height);
        center = new(bounds.X / 2, bounds.Y / 2);

        tileSize = (int)(bounds.Y / tileDivision);
        tileCount = new(bounds.X / (float)tileSize, bounds.Y / (float)tileSize);
    }
}