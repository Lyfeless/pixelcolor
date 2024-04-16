using System.Numerics;
using System.Security.Cryptography;
using Raylib_cs;

namespace PixelColor;

static class Util {
    public static int getScaledFontSizeFromHeight(string str, int height, int maxWidth) {
        float defaultWidth = Raylib.MeasureTextEx(Game.font, str, height, 1).X;
        if (defaultWidth < maxWidth) { return height; }
        return (int)(height / defaultWidth * maxWidth);
    }

    public static float getScaleFromTextLength(string str, int defaultSize, float maxWidth) {
        Vector2 maxLineSize = Raylib.MeasureTextEx(Game.font, str, defaultSize, 1);
        return defaultSize / maxLineSize.X * maxWidth;
    }

    public static int mod(int a, int b) {
        return ((a %= b) < 0) ? a + b : a;
    }

    public static float getDecimal(float value) {
        int sign = Math.Sign(value);
        return ((value * sign) - (float)Math.Truncate(value)) * sign;
    }

    public static bool compareColor(Color c1, Color c2, int threshold) {
        return
            Math.Abs(c1.R - c2.R) <= threshold &&
            Math.Abs(c1.G - c2.G) <= threshold &&
            Math.Abs(c1.B - c2.B) <= threshold;
    }

    public static readonly float contrastThreshold = 0.05f;
    public static Color getContrastCorrectedColor(Color target, Color alternate, Color compare) {
        float targetBrightness = getLuminance(target);
        float compareBrightness = getLuminance(compare);

        return Math.Abs(targetBrightness - compareBrightness) < contrastThreshold ? alternate : target;
    }

    public static float getLuminance(Color color) {
        return ((0.2126f * color.R) + (0.7152f * color.G) + (0.0722f * color.B)) / 1000;
    }

    public static string createFileHash(string path) {
        using MD5 md5 = MD5.Create();
        using FileStream stream = File.OpenRead(path);

        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
    }

    public static long getCurrentTimeMillis(int offset = 0) {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() + offset;
    }
}