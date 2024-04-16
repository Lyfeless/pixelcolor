namespace PixelColor;

class Tile(int color) {
    public int targetColor { get; private set; } = color;
    public int currentColor { get; private set; } = -1;
    public Vector2i previousTile = Vector2i.Negative;
    public Vector2i nextTile = Vector2i.Negative;

    public long changeTime = -1;

    public void setColor(int color, long changeTime) {
        if (changeTime < this.changeTime) { return; }

        currentColor = color;
        this.changeTime = changeTime;
    }
}