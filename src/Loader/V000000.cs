using System.Text;
using Raylib_cs;

namespace PixelColor;

class LoaderV000000 : Loader {
    /*
        Layout:
            int version (already read)
            int tolerance
            The rest is tiles, lay out sequentially, skipping empties
                int currentColor
                int targetColor
                int previousX
                int previousY
                int nextX
                int nextY
    */

    Image image;
    BinaryReader reader;
    int colorTolerance;
    ColorPicker colorPicker;
    Vector2i size;
    Vector2i currentTile;

    public LoaderV000000(string name, BinaryReader reader) {
        image = Raylib.LoadImage(Game.contentPath + $"{name}.png");

        this.reader = reader;

        colorTolerance = reader.ReadInt32();

        size = new(image.Width, image.Height);

        colorPicker = LoaderNew.loadImageColors(image, colorTolerance);

        currentTile = Vector2i.Zero;
    }

    public bool isFileChanged(string path) {
        return false;
    }

    public int getColorTolerance() {
        return colorTolerance;
    }

    public int getNextCurrentColor() {
        if (Raylib.GetImageColor(image, currentTile.X, currentTile.Y).A < 255) {
            incrementTile();
            return -2;
        }

        return reader.ReadInt32();
    }

    public Tile getNextTile() {
        int targetColor = reader.ReadInt32();
        int previousTileX = reader.ReadInt32();
        int previousTileY = reader.ReadInt32();
        int nextTileX = reader.ReadInt32();
        int nextTileY = reader.ReadInt32();

        Tile tile = new(targetColor) {
            previousTile = new(previousTileX, previousTileY),
            nextTile = new(nextTileX, nextTileY)
        };

        incrementTile();

        return tile;
    }

    void incrementTile() {
        currentTile.Y++;
        if (currentTile.Y >= size.Y) {
            currentTile.Y = 0;
            currentTile.X++;
        }
    }

    public Vector2i getSize() {
        return size;
    }

    public ColorPicker getColorPicker() {
        return colorPicker;
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
        writer.Write(000000);

        // Tolerance
        writer.Write(level.colorTolerance);

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
            }
        }
    }
}