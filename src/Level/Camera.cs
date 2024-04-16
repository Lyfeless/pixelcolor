using System.Numerics;
using Raylib_cs;

namespace PixelColor;

class Camera(Vector2i moveBounds) {
    static readonly float scrollSpeed = 0.2f;
    static readonly float moveSpeed = 0.1f;
    static readonly float moveSpeedIncreaseScale = 2.0f;
    static readonly float moveSpeedDecreaseScale = 0.3f;
    static readonly float mouseMoveSpeed = 0.02f;
    static readonly float minZoom = 0.1f;
    static readonly float maxZoom = 1000f;
    static readonly float completeEase = 16f;

    public float x { get; private set; } = moveBounds.X / 2;
    public float y { get; private set; } = moveBounds.Y / 2;
    public float zoom { get; private set; } = 1;
    public bool isMoving { get; private set; }
    public Vector2 moveDelta { get; private set; }
    Vector2i moveBounds = moveBounds;

    public void update() {
        float scrollAmount = Raylib.GetMouseWheelMove();
        if (!scrollAmount.Equals(0)) {
            changeZoom(-scrollSpeed * scrollAmount);
        }
        else if (Raylib.IsKeyDown(KeyboardKey.Up)) {
            changeZoom(-scrollSpeed);
        }
        else if (Raylib.IsKeyDown(KeyboardKey.Down)) {
            changeZoom(scrollSpeed);
        }

        moveDelta = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W)) {
            move(0, -moveSpeed);
        }
        if (Raylib.IsKeyDown(KeyboardKey.S)) {
            move(0, moveSpeed);
        }
        if (Raylib.IsKeyDown(KeyboardKey.A)) {
            move(-moveSpeed, 0);
        }
        if (Raylib.IsKeyDown(KeyboardKey.D)) {
            move(moveSpeed, 0);
        }

        isMoving = false;
        if (Raylib.IsMouseButtonDown(MouseButton.Right)) {
            Vector2 mouseMove = Raylib.GetMouseDelta();
            isMoving = true;
            move(-mouseMove.X * mouseMoveSpeed, -mouseMove.Y * mouseMoveSpeed, false);
        }
    }

    public void changeZoom(float amount) {
        zoom = Math.Clamp(zoom + amount, minZoom, maxZoom);
    }

    public void move(float dx, float dy, bool trackDelta = true) {
        float scale = 1;
        if (Raylib.IsKeyDown(KeyboardKey.LeftShift)) { scale = moveSpeedIncreaseScale; }
        else if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) { scale = moveSpeedDecreaseScale; }

        Vector2 delta = new(
            dx * zoom * scale,
            dy * zoom * scale
        );

        x += delta.X;
        y += delta.Y;

        x = Math.Clamp(x, 0, moveBounds.X);
        y = Math.Clamp(y, 0, moveBounds.Y);

        moveDelta += delta;
    }

    public void moveTowardsFinishView(Texture2D texture) {
        x += ((moveBounds.X / 2f) - x) / completeEase;
        y += ((moveBounds.Y / 2f) - y) / completeEase;

        float zoomTarget;
        if (texture.Width / (float)texture.Height > UIScaler.bounds.X / (float)UIScaler.bounds.Y) {
            zoomTarget = texture.Width / (float)UIScaler.bounds.X;
        }
        else {
            zoomTarget = texture.Height / (float)UIScaler.bounds.Y;
        }

        zoom += (zoomTarget - zoom) / completeEase;
    }
}