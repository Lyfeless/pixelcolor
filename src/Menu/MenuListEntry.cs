using System.Numerics;

namespace PixelColor;

abstract class MenuListEntry(Action callback) {
    public bool selected = false;

    public Delegate callback { get; private set; } = callback;

    public abstract string getDisplayString();
    public abstract void draw(Vector2 position, float scale);
    public abstract void update();
    public void run() {
        callback?.DynamicInvoke();
    }
}