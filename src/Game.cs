using Raylib_cs;

namespace PixelColor;

static class Game {
    public static string contentPath = "content/images/";
    public static string savePath = "save/";
    public static string saveExtension = ".psave";

    public static NetClient client;
    public static NetServer server;

    public static bool isClientActive => client != null;
    public static bool isServerActive => server != null;

    public static int targetFPS = 60;

    public static Texture2D mouseCursor;

    static readonly int SCR_WIDTH = 800;
    static readonly int SCR_HEIGHT = 800;

    public static Font font;

    static Scene activeScene;

    static bool stopSceneSwitch = false;
    static bool shouldClose = false;

    static string storedLevelData = string.Empty;

    public static void init() {
        Raylib.InitWindow(SCR_WIDTH, SCR_HEIGHT, "Hello World");
        Raylib.SetTargetFPS(targetFPS);
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.SetExitKey(KeyboardKey.Null);

        font = Raylib.LoadFontEx("content/fonts/monofonto rg.otf", 128, [], 0);

        mouseCursor = Raylib.LoadTexture("content/textures/cursor.png");

        activeScene = new MainMenu();
    }

    public static void run() {
        while (!Raylib.WindowShouldClose() && !shouldClose) {
            stopSceneSwitch = false;
            if (storedLevelData != string.Empty) {
                switchSceneFromStoredData();
            }

            TimerController.update();
            UIScaler.update();
            activeScene.update();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DarkGray);

            activeScene.draw();

            Raylib.EndDrawing();
        }
    }

    public static void exit() {
        shouldClose = true;
    }

    public static void cleanup() {
        Raylib.CloseWindow();
        activeScene.exit();
    }

    public static MainMenu? getMainMenu() {
        if (activeScene is MainMenu menu) {
            return menu;
        }
        else {
            return null;
        }
    }

    public static void switchSceneMainMenu(string menu = "main") {
        switchScene(new MainMenu(menu));
    }

    public static void switchSceneLevel(string name, int colorTolerance) {
        switchScene(new Level(name, colorTolerance));
    }

    // This functions is called from a callback thread and makes raylib fail to load data if accessed,
    //      so the change has to be stalled until the next main thread update
    public static void switchSceneLevelRemote(string data) {
        storedLevelData = data;
    }

    static void switchSceneFromStoredData() {
        switchScene(new Level(storedLevelData));
        storedLevelData = string.Empty;
    }

    public static void switchSceneError(string message) {
        switchScene(new ErrorScreen(message));
    }

    static void switchScene(Scene scene) {
        if (stopSceneSwitch) { return; }

        activeScene.exit();
        activeScene = scene;

        stopSceneSwitch = true;
    }
}