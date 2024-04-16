using Raylib_cs;

namespace PixelColor;

class ColorEntry(Color color, int count = 0) {
    public Color color { get; private set; } = color;
    public int count { get; private set; } = count;
    public int maxCount { get; private set; } = count;

    public void changeStartAmount(int amount) {
        count += amount;
        maxCount += amount;
    }

    public void changeCount(int amount) {
        count += amount;
    }
}