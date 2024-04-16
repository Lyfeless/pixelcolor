using System.Numerics;
using Raylib_cs;

namespace PixelColor;

class MenuList {
    readonly int tileHeight = 8;
    public string title = "";

    public readonly float widthScale;

    float scroll;

    float scale = 1;
    float maxWidth = 1;

    MenuListEntry[] entries;
    int selectedIndex = 0;

    public bool active { get; private set; }
    bool loop;

    bool ignoreKeys = false;

    int longestLine;

    public MenuList(float widthScale, MenuListEntry[] entries, bool loop) {
        this.widthScale = widthScale;

        this.entries = entries;

        this.loop = loop;

        this.entries[0].selected = true;

        longestLine = 0;
        for (int i = 1; i < entries.Length; ++i) {
            if (entries[i].getDisplayString().Length > entries[longestLine].getDisplayString().Length) {
                longestLine = i;
            }
        }
    }

    public void update() {
        if (ignoreKeys) { ignoreKeys = false; return; }

        entries[selectedIndex].update();

        float scrollAmount = Raylib.GetMouseWheelMove();
        if (!scrollAmount.Equals(0)) {
            changeScroll(-scrollAmount);
        }

        Vector2 mouseMovement = Raylib.GetMouseDelta();
        if (!mouseMovement.Equals(Vector2.Zero)) {
            updateMouse();
        }

        // if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) {
        if (Raylib.IsKeyPressed(KeyboardKey.Down)) {
            changeIndex(1);
        }

        // if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) {
        if (Raylib.IsKeyPressed(KeyboardKey.Up)) {
            changeIndex(-1);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsMouseButtonPressed(MouseButton.Left)) {
            entries[selectedIndex].run();
        }
    }

    public void draw() {
        maxWidth = Math.Min(UIScaler.bounds.X * widthScale * (UIScaler.bounds.Y / (float)UIScaler.bounds.X), UIScaler.bounds.X);
        float drawX = (UIScaler.bounds.X - maxWidth) / 2;

        scale = Util.getScaleFromTextLength(entries[longestLine].getDisplayString(), 30, maxWidth);

        Vector2 titleSize = Raylib.MeasureTextEx(Game.font, title, scale, 1);
        Raylib.DrawTextEx(Game.font, title, new Vector2((UIScaler.bounds.X / 2) - (titleSize.X / 2), (int)(UIScaler.tileSize - scale) - (scroll * scale)), scale, 1, Color.White);


        for (int i = 0; i < entries.Length; ++i) {
            entries[i].draw(new Vector2(drawX, (int)(UIScaler.tileSize + (i * scale)) - (scroll * scale)), scale);
        }
    }

    void updateMouse() {
        Vector2 mousePos = Raylib.GetMousePosition() - new Vector2((UIScaler.bounds.X - maxWidth) / 2, UIScaler.tileSize - (scroll * scale));
        if (mousePos.X < 0 || mousePos.X > maxWidth) { return; }

        int newIndex = (int)(mousePos.Y / scale);

        if (newIndex < 0 || newIndex >= entries.Length) { return; }
        setIndex(newIndex);
    }

    public void setActive(bool value) {
        active = value;
        ignoreKeys = value;
    }

    public MenuListEntry getActiveEntry() {
        return entries[selectedIndex];
    }

    void changeScroll(float amount) {
        setScroll(scroll + amount);
    }

    void setScroll(float value) {
        float fullLength = entries.Length * scale;
        float scrollBounds = tileHeight * UIScaler.tileSize;

        if (fullLength < scrollBounds) {
            scroll = 0;
            return;
        }

        scroll = Math.Clamp(value, 0, entries.Length - (tileHeight * UIScaler.tileSize / scale));
    }

    void changeIndex(int amount) {
        setIndex(selectedIndex + amount);
    }

    void setIndex(int value) {
        entries[selectedIndex].selected = false;
        selectedIndex = value;

        if (loop) {
            selectedIndex = Util.mod(selectedIndex, entries.Length);
        }
        else {
            selectedIndex = Math.Clamp(selectedIndex, 0, entries.Length - 1);
        }

        entries[selectedIndex].selected = true;
    }
}