using System.Text;
using Raylib_cs;

namespace PixelColor;

class LoaderV000200 : Loader {
    /*
        Layout:
            int version (already read)
            int tolerance
            int width
            int height
            bool hasTransparency
            int colorCount
            Color list, length of colorCount
                byte red
                byte green
                byte blue
                int maxCount
            The rest is tiles, lay out sequentially
                int currentColor
                if empty
                    thats it thats the end
                else
                    int targetColor
                    int previousX
                    int previousY
                    int nextX
                    int nextY
    */

    BinaryReader reader;
    int colorTolerance;
    Vector2i size;
    ColorPicker colorPicker;

    public LoaderV000200(BinaryReader reader) {
        this.reader = reader;

        colorTolerance = reader.ReadInt32();

        int sizeX = reader.ReadInt32();
        int sizeY = reader.ReadInt32();
        size = new(sizeX, sizeY);

        bool transparency = reader.ReadBoolean();

        int colorCount = reader.ReadInt32();
        List<ColorEntry> colors = [];

        for (int i = 0; i < colorCount; ++i) {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            int count = reader.ReadInt32();

            colors.Add(new ColorEntry(new Color(r, g, b, (byte)255), count));
        }

        colorPicker = new(colors, transparency);
    }

    public bool isFileChanged(string path) {
        return false;
    }

    public int getColorTolerance() {
        return colorTolerance;
    }

    public ColorPicker getColorPicker() {
        return colorPicker;
    }

    public Vector2i getSize() {
        return size;
    }

    public int getNextCurrentColor() {
        return reader.ReadInt32();
    }

    public Tile getNextTile() {
        int targetColor = reader.ReadInt32();
        int previousX = reader.ReadInt32();
        int previousY = reader.ReadInt32();
        int nextX = reader.ReadInt32();
        int nextY = reader.ReadInt32();

        Tile tile = new(targetColor) {
            previousTile = new(previousX, previousY),
            nextTile = new(nextX, nextY)
        };

        return tile;
    }

    public void close() {
        reader.BaseStream.Close();
        reader.Close();
    }

    public static void save(Level level) {
        string path = Game.savePath + level.name + Game.saveExtension;

        if (!Directory.Exists(Game.savePath)) {
            Directory.CreateDirectory(Game.savePath);
        }

        using FileStream stream = File.Open(path, FileMode.Create);
        using BinaryWriter writer = new(stream, Encoding.UTF8, false);

        // Version
        writer.Write(000200);

        // Tolerance
        writer.Write(level.colorTolerance);

        // Size
        writer.Write(level.size.X);
        writer.Write(level.size.Y);

        // Transparency
        writer.Write(level.colorPicker.hasTransparency);

        // Colors
        writer.Write(level.colorPicker.colors.Count);

        for (int i = 0; i < level.colorPicker.colors.Count; ++i) {
            writer.Write(level.colorPicker.colors[i].color.R);
            writer.Write(level.colorPicker.colors[i].color.G);
            writer.Write(level.colorPicker.colors[i].color.B);
            writer.Write(level.colorPicker.colors[i].maxCount);
        }

        for (int x = 0; x < level.size.X; ++x) {
            for (int y = 0; y < level.size.Y; ++y) {
                Tile tile = level.tiles[x, y];

                if (tile != null) {
                    writer.Write(tile.currentColor);
                    writer.Write(tile.targetColor);
                    writer.Write(tile.previousTile.X);
                    writer.Write(tile.previousTile.Y);
                    writer.Write(tile.nextTile.X);
                    writer.Write(tile.nextTile.Y);
                }
                else {
                    writer.Write(-2);
                }
            }
        }
    }
}