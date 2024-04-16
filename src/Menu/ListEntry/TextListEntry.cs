using System.Numerics;
using Raylib_cs;

namespace PixelColor;

class TextListEntry(string text, Action callback) : MenuListEntry(callback) {
    string text = text;

    public override string getDisplayString() {
        return text;
    }

    public override void draw(Vector2 position, float scale) {
        Color textColor = selected ? Color.SkyBlue : Color.White;
        Raylib.DrawTextEx(Game.font, text, position, scale, 1, textColor);
    }

    public override void update() { }
}