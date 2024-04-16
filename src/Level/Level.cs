using System.Numerics;
using System.Text;
using Raylib_cs;

namespace PixelColor;

class Level : Scene {
    #region Member Variables
    enum State {
        PLAYING,
        DONE,
        MENU,
        MENU_DONE
    }

    static readonly int autoSaveInterval = 30000;
    static readonly int completeReplayDelay = 3000;
    static readonly int completeReplayLength = 10000;

    public readonly string name;
    public Vector2i size { get; private set; }

    Camera camera;

    public Tile[,] tiles;

    public ColorPicker colorPicker { get; private set; }
    NumberEntry numberEntry;
    PauseMenu pauseMenu;

    public int colorTolerance { get; private set; }

    RenderTexture2D renderTexture;
    public int tileSize = 64;

    Vector2i colorStart = Vector2i.Negative;
    Vector2i colorCurrent = Vector2i.Negative;

    bool shouldAutosave;
    Timer timerHandle;

    bool mouseDownAtInit = false;

    State gameState = State.PLAYING;

    long currentTime;

    bool isRemote = false;

    #endregion

    #region Level Setup

    // Constructor used for singleplayer and host instances
    public Level(string imageName, int colorTolerance = 10) {
        name = imageName;

        // Initialize loader object
        Loader loader;

        if (!File.Exists(Game.savePath + name + Game.saveExtension)) {
            loader = new LoaderNew(name, colorTolerance);
        }
        else {
            FileStream fileStream = File.OpenRead(Game.savePath + name + Game.saveExtension);
            BinaryReader reader = new(fileStream);

            int version = reader.ReadInt32();
            switch (version) {
                case 000000:
                    loader = new LoaderV000000(imageName, reader);
                    break;
                case 000100:
                    loader = new LoaderV000100(imageName, reader);
                    break;
                case 000200:
                    loader = new LoaderV000200(reader);
                    break;
                default:
                    Game.switchSceneError($"Save version {version} is not valid, potentially corrupted.");
                    return;
            }
        }

        if (loader.isFileChanged(Game.contentPath + name + ".png")) {
            Game.switchSceneError($"The selected image has been changed since it was last saved, cannot load old save.");
            loader.close();
            return;
        }

        bool success = setup(loader);

        if (success && Game.isServerActive) {
            save();
            string data = Convert.ToBase64String(File.ReadAllBytes(Game.savePath + name + Game.saveExtension));
            Game.client?.sendLevelData(data);
        }
    }

    // Constructor is used when creating game instance from multiplayer instance
    public Level(string data) {
        string name = string.Empty;
        isRemote = true;

        // Initialize loader object
        Loader loader;

        MemoryStream dataStream = new MemoryStream(Convert.FromBase64String(data));
        BinaryReader reader = new BinaryReader(dataStream);

        int version = reader.ReadInt32();
        switch (version) {
            case 000000:
                Game.switchSceneError($"Save version {version} is not compatible with multiplayer.");
                // loader = new LoaderV000000(imageName, reader);
                return;
            case 000100:
                Game.switchSceneError($"Save version {version} is not compatible with multiplayer.");
                // loader = new LoaderV000100(imageName, reader);
                return;
            case 000200:
                loader = new LoaderV000200(reader);
                break;
            default:
                Game.switchSceneError($"Save version {version} is not valid, potentially corrupted.");
                return;
        }

        setup(loader);
    }

    bool setup(Loader loader) {
        shouldAutosave = false;

        pauseMenu = new();

        size = loader.getSize();
        camera = new(size);

        if (size.X > 500 || size.Y > 500) { tileSize /= 2; }
        if (size.X > 1000 || size.Y > 1000) {
            Game.switchSceneError($"Image size of {size.X}, {size.Y} is too large, cannot be loaded.");
            loader.close();
            return false;
        }
        renderTexture = Raylib.LoadRenderTexture(size.X * tileSize, size.Y * tileSize);

        tiles = new Tile[size.X, size.Y];

        colorPicker = loader.getColorPicker();
        numberEntry = new(colorPicker);

        for (int x = 0; x < size.X; ++x) {
            for (int y = 0; y < size.Y; ++y) {
                int currentColor = loader.getNextCurrentColor();

                // empty value for tiles with transparency
                if (currentColor == -2) {
                    continue;
                }

                Tile tile = loader.getNextTile();
                tiles[x, y] = tile;

                if (tile.previousTile == Vector2i.Negative && tile.nextTile != Vector2i.Negative) {
                    colorStart = new(x, y);
                }
                if (tile.nextTile == Vector2i.Negative && tile.previousTile != Vector2i.Negative) {
                    colorCurrent = new(x, y);
                }

                setTile(x, y, currentColor, true);
            }
        }

        scheduleAutosave();

        colorPicker.clearAllEmpties();
        if (colorPicker.isDone()) {
            complete();
        }

        mouseDownAtInit = Raylib.IsMouseButtonDown(MouseButton.Left);

        loader.close();

        return true;
    }

    #endregion

    public void exit() {
        if (shouldAutosave) {
            save();
        }
        TimerController.clearTimer(timerHandle);
        Raylib.UnloadRenderTexture(renderTexture);
    }

    #region Updating
    public void update() {
        switch (gameState) {
            case State.DONE:
                updateCompletedScreen();
                break;
            case State.MENU:
                updateMenu(State.PLAYING);
                break;
            case State.MENU_DONE:
                updateMenu(State.DONE);
                break;
            case State.PLAYING:
                updateMain();
                break;
        }
    }

    void updateCompletedScreen() {
        camera.moveTowardsFinishView(renderTexture.Texture);

        int pixelsToDraw = (int)Math.Max(1, size.X * size.Y * (1000f / Game.targetFPS) / completeReplayLength);

        while (pixelsToDraw > 0 && colorCurrent != Vector2i.Negative) {
            Raylib.BeginTextureMode(renderTexture);
            Raylib.DrawRectangle(colorCurrent.X * tileSize, colorCurrent.Y * tileSize, tileSize, tileSize, colorPicker.colors[tiles[colorCurrent.X, colorCurrent.Y].targetColor].color);
            Raylib.EndTextureMode();

            colorCurrent = tiles[colorCurrent.X, colorCurrent.Y].nextTile;

            pixelsToDraw--;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) {
            gameState = State.MENU_DONE;
        }
    }

    void updateMenu(State returnState) {
        Game.client?.update(this);

        pauseMenu.update();

        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) {
            gameState = returnState;
        }
    }

    void updateMain() {
        Game.client?.update(this);

        currentTime = Util.getCurrentTimeMillis();

        Vector2 mouseDelta = Raylib.GetMouseDelta();
        if (!mouseDelta.Equals(Vector2.Zero)) {
            Game.client?.sendPlayerMove(mouseToTileSpace());
        }

        if (!camera.isMoving && colorPicker.isMouseInPicker()) {
            colorPicker.update();
        }
        else {
            camera.update();

            if (Raylib.IsMouseButtonDown(MouseButton.Left)) {
                if (!mouseDownAtInit) {
                    Vector2 mousePos = Raylib.GetMousePosition();

                    Vector2 mouseEnd = screenSpaceToTileSpace(mousePos);
                    Vector2 mouseStart = screenSpaceToTileSpace(mousePos - mouseDelta);

                    if (camera.moveDelta != Vector2.Zero) {
                        // Cast move check line before appyling mouse move, so use pre-moved mouse position for the check
                        setTilesAlongLine(mouseEnd - camera.moveDelta, mouseEnd);
                    }


                    setTilesAlongLine(mouseStart, mouseEnd);
                }
            }
            else {
                mouseDownAtInit = false;
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Middle)) {
                Vector2 tilePos = mouseToTileSpace();
                Tile tile = getTileFromVec2(tilePos);

                if (tile != null && tile.currentColor == tile.targetColor) {
                    colorPicker.setColor(tile.currentColor);
                }

            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.E)) {
            colorPicker.changeUsableColor(1);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Q)) {
            colorPicker.changeUsableColor(-1);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace)) {
            colorPicker.setEraser();
        }

        numberEntry.update();

        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) {
            gameState = State.MENU;
        }
    }
    #endregion

    #region Drawing
    public void draw() {
        Raylib.DrawTexturePro(
            renderTexture.Texture,
            new Rectangle(
                0,
                0,
                renderTexture.Texture.Width,
                -renderTexture.Texture.Height
            ),
            new Rectangle(
                (UIScaler.bounds.X / 2) - (camera.x * tileSize / camera.zoom),
                (UIScaler.bounds.Y / 2) - (camera.y * tileSize / camera.zoom),
                renderTexture.Texture.Width / camera.zoom,
                renderTexture.Texture.Height / camera.zoom
            ),
            Vector2.Zero,
            0.0f,
            Color.White
        );

        Game.client?.draw(this);

        if (gameState == State.MENU || gameState == State.MENU_DONE) {
            pauseMenu.draw();
        }

        if (gameState == State.DONE || gameState == State.MENU_DONE) { return; }

        colorPicker.draw();
        numberEntry.draw();
    }
    #endregion

    #region Tile Management
    void setTilesAlongLine(Vector2 startPos, Vector2 endPos) {
        Vector2i tile = new((int)startPos.X, (int)startPos.Y);

        if (tile.X == (int)endPos.X && tile.Y == (int)endPos.Y) {
            if (isPositionValid(tile.X, tile.Y)) {
                trySetTile(tile.X, tile.Y, colorPicker.activeColor);
            }
        }
        else {
            Vector2 posDelta = endPos - startPos;

            Vector2 deltaDistance = new() {
                X = Math.Abs(1.0f / posDelta.X),
                Y = Math.Abs(1.0f / posDelta.Y),
            };

            Vector2i step = new(
                Math.Sign(posDelta.X),
                Math.Sign(posDelta.Y)
            );

            Vector2 sideDistance = new() {
                X = (step.X < 0 ? startPos.X - tile.X : tile.X + 1 - startPos.X) * deltaDistance.X,
                Y = (step.Y < 0 ? startPos.Y - tile.Y : tile.Y + 1 - startPos.Y) * deltaDistance.Y,
            };

            for (int i = 0; i < size.X + size.Y; ++i) {
                if (isPositionValid(tile.X, tile.Y)) {
                    trySetTile(tile.X, tile.Y, colorPicker.activeColor);
                }

                if ((tile.X == (int)endPos.X && tile.Y == (int)endPos.Y) || step == Vector2i.Zero) {
                    break;
                }

                if (sideDistance.X < sideDistance.Y) {
                    sideDistance.X += deltaDistance.X;
                    tile.X += step.X;
                }
                else {
                    sideDistance.Y += deltaDistance.Y;
                    tile.Y += step.Y;
                }

                if (tile.X < 0 || tile.Y < 0) {
                    break;
                }
            }
        }
    }

    public void trySetTile(int x, int y, int color) {
        if (tiles[x, y] != null && tiles[x, y].currentColor != color) {
            setTile(x, y, color);
            Game.client?.sendSetTile(x, y, color, currentTime);
        }
    }

    void setTile(int x, int y, int color, bool ignoreChain = false) {
        shouldAutosave = true;

        Tile tile = tiles[x, y];

        // Check if old color is target to update count
        if (tile.targetColor == tile.currentColor) {
            colorPicker.changeColorCount(tile.currentColor, 1);

            if (!ignoreChain) {
                if (tile.previousTile != Vector2i.Negative) {
                    tiles[tile.previousTile.X, tile.previousTile.Y].nextTile = tile.nextTile;
                }
                else {
                    colorStart = new(tile.nextTile.X, tile.nextTile.Y);
                }

                if (tile.nextTile != Vector2i.Negative) {
                    tiles[tile.nextTile.X, tile.nextTile.Y].previousTile = tile.previousTile;
                }
                else {
                    colorCurrent = new(tile.previousTile.X, tile.previousTile.Y);
                }

                tile.previousTile = Vector2i.Negative;
                tile.nextTile = Vector2i.Negative;
            }
        }

        tile.setColor(color, currentTime);

        // Check if new color is target to update count
        if (tile.targetColor == tile.currentColor) {
            colorPicker.changeColorCount(tile.currentColor, -1);

            if (!ignoreChain) {
                if (colorCurrent != Vector2i.Negative) {
                    tiles[colorCurrent.X, colorCurrent.Y].nextTile = new(x, y);
                    tile.previousTile = colorCurrent;
                }
                else {
                    colorStart = new(x, y);
                }

                colorCurrent = new(x, y);
            }

            if (colorPicker.isDone()) {
                complete();
            }
        }

        Raylib.BeginTextureMode(renderTexture);

        int drawX = x * tileSize;
        int drawY = y * tileSize;

        if (tile.currentColor == -1) {
            drawEmptyTile(drawX, drawY, tile.targetColor, Color.Black);
        }
        else if (tile.currentColor != tile.targetColor) {
            drawEmptyTile(drawX, drawY, tile.targetColor, colorPicker.colors[tile.currentColor].color);
        }
        else {
            Color colorValue = colorPicker.colors[tile.currentColor].color;
            Raylib.DrawRectangle(drawX, drawY, tileSize, tileSize, colorValue);
        }

        Raylib.EndTextureMode();
    }

    void drawEmptyTile(int x, int y, int targetColor, Color borderColor) {
        Raylib.DrawRectangle(x, y, tileSize, tileSize, Color.White);
        Raylib.DrawRectangleLines(x, y, tileSize, tileSize, borderColor);

        string numText = (targetColor + 1).ToString();
        int fontSize = Util.getScaledFontSizeFromHeight(numText, tileSize, (int)(tileSize * 0.8f));

        Vector2 textSize = Raylib.MeasureTextEx(Game.font, numText, fontSize, 1);

        int textOffsetX = (int)((tileSize / 2f) - (textSize.X / 2f));
        int textOffsetY = (int)((tileSize / 2f) - (textSize.Y / 2f));

        Raylib.DrawTextEx(Game.font, numText, new() { X = x + textOffsetX, Y = y + textOffsetY }, fontSize, 1, Util.getContrastCorrectedColor(borderColor, Color.Gray, Color.White));
    }
    #endregion

    #region State Management
    void complete() {
        gameState = State.DONE;

        TimerController.clearTimer(timerHandle);
        timerHandle = TimerController.createTimer(completeReplayDelay, startReplay);
    }

    void startReplay() {
        colorCurrent = colorStart;

        Raylib.BeginTextureMode(renderTexture);
        if (colorPicker.hasTransparency) {
            for (int x = 0; x < tiles.GetLength(0); ++x) {
                for (int y = 0; y < tiles.GetLength(1); ++y) {
                    Tile tile = tiles[x, y];
                    if (tile != null) {
                        Raylib.DrawRectangle(x * tileSize, y * tileSize, tileSize, tileSize, Color.White);
                    }
                }
            }
        }
        else {
            Raylib.ClearBackground(Color.White);
        }
        Raylib.EndTextureMode();
    }

    void scheduleAutosave() {
        timerHandle = TimerController.createTimer(autoSaveInterval, tryAutosave);
    }

    void tryAutosave() {
        if (shouldAutosave && gameState == State.PLAYING) {
            save();
            shouldAutosave = false;
        }

        scheduleAutosave();
    }

    void save() {
        if (isRemote) { return; }

        LoaderV000200.save(this);
    }
    #endregion

    #region Utility Functions
    public Vector2 tileSpaceToScreenSpace(Vector2 tilePos) {
        float zoomScale = tileSize / camera.zoom;
        float castX = (UIScaler.bounds.X / 2) - ((camera.x - tilePos.X) * zoomScale);
        float castY = (UIScaler.bounds.Y / 2) - ((camera.y - tilePos.Y) * zoomScale);
        return new(castX, castY);
    }

    public Vector2 screenSpaceToTileSpace(Vector2 screenPos) {
        float zoomScale = tileSize / camera.zoom;
        float castX = ((screenPos.X - (UIScaler.bounds.X / 2)) / zoomScale) + camera.x;
        float castY = ((screenPos.Y - (UIScaler.bounds.Y / 2)) / zoomScale) + camera.y;
        return new(castX, castY);
    }

    public Vector2 mouseToTileSpace() {
        Vector2 mousePos = Raylib.GetMousePosition();
        return screenSpaceToTileSpace(mousePos);
    }

    public bool isPositionValid(int x, int y) {
        return
            x >= 0 &&
            x < tiles.GetLength(0) &&
            y >= 0 &&
            y < tiles.GetLength(1);
    }

    Tile getTileFromVec2(Vector2 position) {
        int roundedX = (int)position.X;
        int roundedY = (int)position.Y;

        if (!isPositionValid(roundedX, roundedY)) { return null; }
        return tiles[roundedX, roundedY];
    }

    // public Vector2 getTileCoordsFromMouse() {
    //     Vector2 mousePos = Raylib.GetMousePosition();

    //     return getTileCoordsFromPosition(mousePos.X, mousePos.Y);
    // }

    // public Vector2 getTileCoordsFromPosition(float x, float y) {
    //     Vector2 castPos = getTileCoordsFromScreenCoords(x, y);

    //     if (castPos.X < 0 || castPos.X >= tiles.GetLength(0) || castPos.Y < 0 || castPos.Y >= tiles.GetLength(1)) { return new() { X = -1, Y = -1 }; }

    //     return new() { X = castPos.X, Y = castPos.Y };
    // }

    // public Vector2 getTileCoordsFromScreenCoords(float x, float y) {
    //     float zoomScale = tileSize / camera.zoom;

    //     float castX = ((x - (UIScaler.bounds.X / 2)) / zoomScale) + camera.x;
    //     float castY = ((y - (UIScaler.bounds.Y / 2)) / zoomScale) + camera.y;

    //     return new() { X = castX, Y = castY };
    // }
    #endregion
}