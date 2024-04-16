using System.Numerics;
using Raylib_cs;

namespace PixelColor;

class ErrorScreen : Scene {
    string message;

    public ErrorScreen(string message) {
        this.message = message;

        if (Game.isServerActive) {
            Game.client?.sendLevelEnd();
        }
    }

    public void update() {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter)) {
            Game.switchSceneMainMenu(Game.isServerActive ? "levels" : "main");
        }
    }

    public void draw() {
        float scale = Util.getScaleFromTextLength(message, 30, UIScaler.bounds.X - (UIScaler.bounds.X / 20));
        Vector2 drawPosition = new(UIScaler.bounds.X / 40, UIScaler.bounds.Y / 2);

        Raylib.DrawTextEx(Game.font, message, drawPosition, scale, 1, Color.Maroon);
        Raylib.DrawTextEx(Game.font, "An Error Occured:", drawPosition - new Vector2(0, scale), scale, 1, Color.Maroon);
        Raylib.DrawTextEx(Game.font, "Press enter to continue.", drawPosition + new Vector2(0, scale), scale, 1, Color.Maroon);
    }

    public void exit() { }
}