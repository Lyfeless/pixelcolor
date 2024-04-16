using System.Numerics;
using Raylib_cs;
using TextCopy;

namespace PixelColor;

class TextInputListEntry(string text, int maxLength, Action callback) : MenuListEntry(callback) {
    string text = text;
    public string inputText { get; private set; } = string.Empty;
    int maxLength = maxLength;
    int spacePadding = maxLength;

    public override string getDisplayString() {
        return text + ": " + new string(' ', maxLength);
    }

    public override void draw(Vector2 position, float scale) {
        Color textColor = selected ? Color.SkyBlue : Color.White;
        Raylib.DrawTextEx(Game.font, text + ": " + new string(' ', spacePadding) + inputText, position, scale, 1, textColor);
    }

    public override void update() {
        int key;
        do {
            key = Raylib.GetKeyPressed();
            if (key > 33 && key < 122) {
                string keyString = ((char)key).ToString();

                if (keyString == "V" && Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
                    string? clipboardText = ClipboardService.GetText();
                    if (clipboardText != null) { addString(clipboardText); }
                    continue;
                }

                if (!(Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.LeftShift))) {
                    keyString = keyString.ToLower();
                }

                addCharacter(keyString);
            }
        } while (key != 0);

        if (Raylib.IsKeyPressed(KeyboardKey.Space)) {
            addCharacter(" ");
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace)) {
            removeCharacter();
        }
    }

    void addCharacter(string c) {
        if (spacePadding <= 0) { return; }

        inputText += c;
        spacePadding--;
    }

    void removeCharacter() {
        if (spacePadding >= maxLength) { return; }

        inputText = inputText[..^1];
        spacePadding++;
    }

    void addString(string text) {
        if (spacePadding <= 0) { return; }

        int addLength = Math.Min(spacePadding, text.Length);
        inputText += text[..addLength];
        spacePadding -= addLength;
    }
}