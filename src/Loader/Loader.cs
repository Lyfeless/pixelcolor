namespace PixelColor;

interface Loader {
    public bool isFileChanged(string path);
    public int getColorTolerance();
    public int getNextCurrentColor();
    public Tile getNextTile();
    public Vector2i getSize();
    public ColorPicker getColorPicker();
    public void close();

    public static void save(Level level) { }
}