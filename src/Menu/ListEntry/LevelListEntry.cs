using System.Numerics;
using Raylib_cs;

namespace PixelColor;

class LevelListEntry(string name, Action callback) : MenuListEntry(callback) {
    public string fileName { get; private set; } = name;
    public bool inProgress = File.Exists(Game.savePath + name + Game.saveExtension);

    public override string getDisplayString() {
        return fileName + (inProgress ? " [ In Progress ]" : "");
    }

    public override void draw(Vector2 position, float scale) {
        Vector2 textSize = Raylib.MeasureTextEx(Game.font, fileName, scale, 1);

        Color textColor = selected ? Color.SkyBlue : Color.White;

        Raylib.DrawTextEx(Game.font, fileName, position, scale, 1, textColor);
        if (inProgress) { Raylib.DrawTextEx(Game.font, " [ In Progress ]", position + new Vector2(textSize.X, 0), scale, 1, Color.Gold); }
    }

    public override void update() { }

    public void clearProgress() {
        inProgress = false;
    }
}