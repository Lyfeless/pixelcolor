using Raylib_cs;

namespace PixelColor;

class LoaderNew : Loader {
    /*
        Construct a new level using only image file as reference, no layout
    */

    Image image;
    int colorTolerance;
    ColorPicker colorPicker;
    Vector2i size;
    Vector2i currentTile;

    public LoaderNew(string name, int colorTolerance) {
        image = Raylib.LoadImage(Game.contentPath + $"{name}.png");

        size = new(image.Width, image.Height);

        this.colorTolerance = colorTolerance;

        colorPicker = loadImageColors(image, colorTolerance);

        currentTile = Vector2i.Zero;
    }

    public int getColorTolerance() {
        return colorTolerance;
    }

    public int getNextCurrentColor() {
        byte alpha = Raylib.GetImageColor(image, currentTile.X, currentTile.Y).A;

        if (alpha < 255) {
            incrementTile();
        }

        return alpha < 255 ? -2 : -1;
    }

    public Tile getNextTile() {
        Color color = Raylib.GetImageColor(image, currentTile.X, currentTile.Y);
        ColorEntry entry = colorPicker.colors.Find(e => Util.compareColor(e.color, color, colorTolerance));
        int index = colorPicker.colors.IndexOf(entry);

        incrementTile();

        return new(index);
    }

    void incrementTile() {
        currentTile.Y++;
        if (currentTile.Y >= size.Y) {
            currentTile.Y = 0;
            currentTile.X++;
        }
    }

    public bool isFileChanged(string path) {
        return false;
    }

    public Vector2i getSize() {
        return size;
    }

    public ColorPicker getColorPicker() {
        return colorPicker;
    }

    public void close() { }

    public static void save(Level level) { }

    public static ColorPicker loadImageColors(Image image, int colorTolerance) {
        List<ColorEntry> colors = [];
        bool transparency = false;

        for (int x = 0; x < image.Width; ++x) {
            for (int y = 0; y < image.Height; ++y) {
                Color color = Raylib.GetImageColor(image, x, y);
                if (color.A < 255) {
                    transparency = true;
                    continue;
                }

                int index = colors.FindIndex(e => Util.compareColor(e.color, color, colorTolerance));
                if (index == -1) {
                    colors.Add(new ColorEntry(color));
                    index = colors.Count - 1;
                }

                colors[index].changeStartAmount(1);
            }
        }

        return new([.. colors.OrderByDescending(e => e.count)], transparency);
    }
}