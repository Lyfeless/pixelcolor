using Raylib_cs;

namespace PixelColor;

class PauseMenu {
    static readonly Color backgroundColor = new(150, 150, 150, 200);

    MenuList pauseList;

    MenuList activeMenu;

    public PauseMenu() {
        pauseList = new(
            0.5f,
            [
                new TextListEntry("Quit to Menu", actionQuitToMenu)
            ],
            true
        ) {
            title = "Game Paused"
        };

        activeMenu = pauseList;
    }

    public void update() {
        activeMenu.update();
    }

    public void draw() {
        float backgroundWidth = Math.Min(UIScaler.bounds.X * 0.8f * (UIScaler.bounds.Y / (float)UIScaler.bounds.X), UIScaler.bounds.X);
        Raylib.DrawRectangle((int)(UIScaler.bounds.X - backgroundWidth) / 2, 0, (int)backgroundWidth, UIScaler.bounds.Y, backgroundColor);

        activeMenu.draw();
    }

    void actionQuitToMenu() {
        if (Game.isClientActive) {
            if (Game.isServerActive) {
                Game.switchSceneMainMenu("levels");
                Game.client?.sendLevelEnd();
            }
            else {
                Game.client?.disconnect();
                Game.switchSceneMainMenu("joinDisconnect");
            }
        }
        else {
            Game.switchSceneMainMenu();
        }
    }
}