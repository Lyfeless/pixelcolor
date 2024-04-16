using System.Numerics;
using Raylib_cs;

namespace PixelColor;

class ColorPicker(List<ColorEntry> colors, bool hasTransparency) {
    static readonly Color backgroundColor = new(255, 255, 255, 200);
    static readonly Color completeColor = new(115, 255, 145, 255);

    public List<ColorEntry> colors { get; private set; } = new(colors);
    readonly List<int> useableColors = Enumerable.Range(0, colors.Count).ToList();
    public int activeColor { get; private set; } = 0;
    int activeColorIndex = 0;

    float scroll = 0;

    int totalPixelCount = colors.Sum(e => e.count);
    int totalUnfinishedPixelCount = 0;

    public bool hasTransparency { get; private set; } = hasTransparency;

    public void clearAllEmpties() {
        useableColors.RemoveAll(c => colors[c].count == 0);
        if (!canScroll()) { scroll = 0; }
    }

    public void update() {
        if (canScroll()) {
            float scrollAmount = Raylib.GetMouseWheelMove();
            if (!scrollAmount.Equals(0)) {
                changeScroll(scrollAmount * 0.3f);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D)) {
                changeScroll(0.2f);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.A)) {
                changeScroll(-0.2f);
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.Right) || Raylib.IsMouseButtonDown(MouseButton.Left)) {
                changeScroll(Raylib.GetMouseDelta().X * -0.01f);
            }
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsMouseButtonPressed(MouseButton.Middle)) {
            float mousePos = Raylib.GetMousePosition().X;
            int index = (int)Math.Floor((mousePos / UIScaler.tileSize) + scroll);
            setUsableColor(index);
        }
    }

    public void draw() {
        int drawY = UIScaler.bounds.Y - UIScaler.tileSize;

        Raylib.DrawRectangle(0, drawY, UIScaler.bounds.X, UIScaler.tileSize, backgroundColor);

        int scrollOffset = (int)Math.Floor(scroll * UIScaler.tileSize % UIScaler.tileSize);
        int startIndex = (int)Math.Floor(scroll * UIScaler.tileSize / UIScaler.tileSize);

        int loopCount = Math.Min((int)Math.Floor(UIScaler.tileCount.X) + 2, useableColors.Count);
        for (int i = 0; i < loopCount; ++i) {
            // Idk how to make this math not have an off-by-one issue at the end so here's a failsafe
            if (i + startIndex >= useableColors.Count) { continue; }

            int colorIndex = useableColors[i + startIndex];
            int itemOffset = i * UIScaler.tileSize;

            if (activeColor == colorIndex) { Raylib.DrawRectangle(itemOffset - scrollOffset, drawY, UIScaler.tileSize, UIScaler.tileSize, Color.Black); }

            int borderSize = UIScaler.tileSize / 10;

            int maxWidth = borderSize * 8;

            if (colors[colorIndex].count > 0) {
                string indexString = (colorIndex + 1).ToString();
                string countString = colors[colorIndex].count.ToString();
                int indexFontSize = Util.getScaledFontSizeFromHeight(indexString, borderSize * 6, maxWidth);
                int countFontSize = Util.getScaledFontSizeFromHeight(indexString, borderSize * 2, maxWidth);
                Color textColor = Util.getContrastCorrectedColor(Color.White, Color.Gray, colors[colorIndex].color);

                Raylib.DrawRectangle(itemOffset + 5 - scrollOffset, drawY + 5, UIScaler.tileSize - 10, UIScaler.tileSize - 10, colors[colorIndex].color);

                Raylib.DrawTextEx(Game.font, indexString, new(itemOffset + borderSize - scrollOffset, drawY + borderSize), indexFontSize, 1, textColor);
                Raylib.DrawTextEx(Game.font, countString, new(itemOffset + borderSize - scrollOffset, drawY + (borderSize * 7)), countFontSize, 1, textColor);
            }
            else {
                int fontSize = Util.getScaledFontSizeFromHeight("DONE", borderSize * 8, maxWidth);

                Raylib.DrawRectangle(itemOffset + 5 - scrollOffset, drawY + 5, UIScaler.tileSize - 10, UIScaler.tileSize - 10, completeColor);
                Raylib.DrawTextEx(Game.font, "DONE", new(itemOffset + borderSize - scrollOffset, drawY + (borderSize * 2.5f)), fontSize, 1, Color.Black);
            }
        }

        if (Raylib.IsKeyDown(KeyboardKey.Tab)) {
            string percent = Math.Round((float)totalUnfinishedPixelCount / totalPixelCount * 100, 1) + "%";
            float percentFontSize = Util.getScaledFontSizeFromHeight(percent, (int)(UIScaler.tileSize * 0.8f), (int)(UIScaler.tileSize * 0.8f) * 2);
            Vector2 percentTextSize = Raylib.MeasureTextEx(Game.font, percent, percentFontSize, 1);

            Raylib.DrawRectangle(0, UIScaler.bounds.Y - (UIScaler.tileSize * 2), UIScaler.tileSize * 2, UIScaler.tileSize, backgroundColor);
            Raylib.DrawTextEx(Game.font, percent, new Vector2(UIScaler.tileSize - (percentTextSize.X / 2), UIScaler.bounds.Y - (UIScaler.tileSize * 1.5f) - (percentTextSize.Y / 2)), percentFontSize, 1, Color.Black);
        }
    }

    public void changeUsableColor(int change) {
        if (activeColorIndex == -1) {
            setUsableColor(0);
        }
        else {
            setUsableColor(activeColorIndex + change);
        }
    }

    public void setUsableColor(int value) {
        if (useableColors.Count == 0) { return; }

        int emptyCount = useableColors.Where((e, i) => colors[e].count == 0 && i <= value).Count();
        clearAllEmpties();

        value -= emptyCount;

        activeColorIndex = Util.mod(value, useableColors.Count);
        activeColor = useableColors[activeColorIndex];

        // Make sure selected item isn't under the lowest visible item
        if (activeColorIndex < scroll) { setScroll(activeColorIndex); }
        // Make sure selected item isn't above the highest visible item
        if (activeColorIndex >= Math.Floor(scroll + UIScaler.tileCount.X)) { setScroll(activeColorIndex - UIScaler.tileCount.X + 1); }
    }

    public void setColor(int colorIndex) {
        int useableIndex = useableColors.IndexOf(colorIndex);
        if (useableIndex != -1) {
            setUsableColor(useableIndex);
        }
    }

    public void changeColorCount(int colorIndex, int amount) {
        colors[colorIndex].changeCount(amount);
        totalUnfinishedPixelCount -= amount;
        if (colors[colorIndex].count > 0 && colors[colorIndex].count - amount <= 0) {
            activateColor(colorIndex);
        }
    }

    public void activateColor(int index) {
        // Edge case for first item in list
        if (useableColors[0] > index) {
            useableColors.Insert(0, index);
            changeUsableColor(1);
            return;
        }

        // Otherwise do full add check
        for (int i = 0; i < useableColors.Count; ++i) {
            if (useableColors[i] < index && (i == useableColors.Count - 1 || useableColors[i + 1] > index)) {
                useableColors.Insert(i + 1, index);
                if (activeColor > index) {
                    changeUsableColor(1);
                }
                return;
            }
        }
    }

    public void setEraser() {
        activeColorIndex = -1;
        activeColor = -1;
    }

    public bool isMouseInPicker() {
        return Raylib.GetMousePosition().Y >= UIScaler.bounds.Y - UIScaler.tileSize;
    }

    public bool isDone() {
        return
            useableColors.Count == 0 ||
            (
                useableColors.Count == 1
                && colors[useableColors[0]].count == 0
            );
    }

    public bool isColorActive(int color) {
        return useableColors.Contains(color);
    }

    bool canScroll() {
        return useableColors.Count - (int)Math.Floor(UIScaler.tileCount.X) > 0;
    }

    void changeScroll(float amount) {
        setScroll(scroll + amount);
    }

    void setScroll(float value) {
        scroll = Math.Clamp(value, 0, useableColors.Count - UIScaler.tileCount.X);
    }
}