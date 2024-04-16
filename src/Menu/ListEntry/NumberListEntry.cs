using System.Numerics;
using Raylib_cs;
using TextCopy;

namespace PixelColor;

class NumberListEntry(string text, int min, int max, Action callback) : MenuListEntry(callback) {
    string text = text;
    int min = min;
    int max = max;

    public int value { get; private set; } = min;

    public override string getDisplayString() {
        return text + ": " + max;
    }

    public override void draw(Vector2 position, float scale) {
        int padding = max.ToString().Length - value.ToString().Length;

        Color textColor = selected ? Color.SkyBlue : Color.White;
        Raylib.DrawTextEx(Game.font, text + ": " + new string(' ', padding) + value, position, scale, 1, textColor);
    }

    public override void update() {
        if (Raylib.IsKeyPressed(KeyboardKey.V) && Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
            string? clipboardText = ClipboardService.GetText();
            if (clipboardText != null && int.TryParse(clipboardText, out int clipboardNumber)) {
                appendDigits(clipboardNumber, clipboardText.Length);
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) {
            changeValue(-1);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) {
            changeValue(1);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace)) {
            setValue(value / 10);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Zero) || Raylib.IsKeyPressed(KeyboardKey.Kp0)) { appendDigit(0); }
        if (Raylib.IsKeyPressed(KeyboardKey.One) || Raylib.IsKeyPressed(KeyboardKey.Kp1)) { appendDigit(1); }
        if (Raylib.IsKeyPressed(KeyboardKey.Two) || Raylib.IsKeyPressed(KeyboardKey.Kp2)) { appendDigit(2); }
        if (Raylib.IsKeyPressed(KeyboardKey.Three) || Raylib.IsKeyPressed(KeyboardKey.Kp3)) { appendDigit(3); }
        if (Raylib.IsKeyPressed(KeyboardKey.Four) || Raylib.IsKeyPressed(KeyboardKey.Kp4)) { appendDigit(4); }
        if (Raylib.IsKeyPressed(KeyboardKey.Five) || Raylib.IsKeyPressed(KeyboardKey.Kp5)) { appendDigit(5); }
        if (Raylib.IsKeyPressed(KeyboardKey.Six) || Raylib.IsKeyPressed(KeyboardKey.Kp6)) { appendDigit(6); }
        if (Raylib.IsKeyPressed(KeyboardKey.Seven) || Raylib.IsKeyPressed(KeyboardKey.Kp7)) { appendDigit(7); }
        if (Raylib.IsKeyPressed(KeyboardKey.Eight) || Raylib.IsKeyPressed(KeyboardKey.Kp8)) { appendDigit(8); }
        if (Raylib.IsKeyPressed(KeyboardKey.Nine) || Raylib.IsKeyPressed(KeyboardKey.Kp9)) { appendDigit(9); }
    }

    void appendDigits(int digit, int count) {
        setValue((value * (int)Math.Pow(10, count)) + digit);
    }

    void appendDigit(int digit) {
        setValue((value * 10) + digit);
    }

    void changeValue(int amount) {
        setValue(value + amount);
    }

    void setValue(int value) {
        this.value = Math.Clamp(value, min, max);
        run();
    }
}