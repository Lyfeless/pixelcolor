using Raylib_cs;

namespace PixelColor;

class NumberEntry(ColorPicker colorPicker) {
    static readonly int sendTime = 3000;
    static readonly int hideTime = 1000;

    static readonly Color defaultColor = new(255, 255, 255, 200);
    static readonly Color correctColor = new(0, 255, 0, 200);
    static readonly Color wrongColor = new(255, 0, 0, 200);

    readonly ColorPicker colorPicker = colorPicker;

    string input = "";
    string storedInput = "";
    readonly int maxLength = (colorPicker.colors.Count - 1).ToString().Length;

    bool visible = false;
    bool wrongInput = false;

    long currentActionRunTime = -1;

    public void update() {
        if (Raylib.IsKeyPressed(KeyboardKey.Zero) || Raylib.IsKeyPressed(KeyboardKey.Kp0)) { addInput(0); }
        if (Raylib.IsKeyPressed(KeyboardKey.One) || Raylib.IsKeyPressed(KeyboardKey.Kp1)) { addInput(1); }
        if (Raylib.IsKeyPressed(KeyboardKey.Two) || Raylib.IsKeyPressed(KeyboardKey.Kp2)) { addInput(2); }
        if (Raylib.IsKeyPressed(KeyboardKey.Three) || Raylib.IsKeyPressed(KeyboardKey.Kp3)) { addInput(3); }
        if (Raylib.IsKeyPressed(KeyboardKey.Four) || Raylib.IsKeyPressed(KeyboardKey.Kp4)) { addInput(4); }
        if (Raylib.IsKeyPressed(KeyboardKey.Five) || Raylib.IsKeyPressed(KeyboardKey.Kp5)) { addInput(5); }
        if (Raylib.IsKeyPressed(KeyboardKey.Six) || Raylib.IsKeyPressed(KeyboardKey.Kp6)) { addInput(6); }
        if (Raylib.IsKeyPressed(KeyboardKey.Seven) || Raylib.IsKeyPressed(KeyboardKey.Kp7)) { addInput(7); }
        if (Raylib.IsKeyPressed(KeyboardKey.Eight) || Raylib.IsKeyPressed(KeyboardKey.Kp8)) { addInput(8); }
        if (Raylib.IsKeyPressed(KeyboardKey.Nine) || Raylib.IsKeyPressed(KeyboardKey.Kp9)) { addInput(9); }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter)) { sendInput(); }

        long currentTime = Util.getCurrentTimeMillis();
        if (currentActionRunTime != -1 && currentActionRunTime <= currentTime) {
            if (input != "") {
                sendInput();
                currentActionRunTime = currentTime + hideTime;
            }
            else {
                visible = false;
                wrongInput = false;
                currentActionRunTime = -1;
            }
        }
    }

    public void draw() {
        if (visible) {
            Color bgColor = defaultColor;
            if (input == "") { bgColor = wrongInput ? wrongColor : correctColor; }

            int borderSize = UIScaler.tileSize / 6;
            int fontSize = UIScaler.tileSize - borderSize - borderSize;
            int width = (int)Raylib.MeasureTextEx(
                Game.font,
                new string('0', maxLength),
                fontSize,
                1
            ).X + borderSize + borderSize;

            int drawY = UIScaler.bounds.Y - (UIScaler.tileSize * 2);
            int drawX = UIScaler.bounds.X - width;

            Raylib.DrawRectangle(drawX, drawY, width, UIScaler.tileSize, bgColor);

            Raylib.DrawTextEx(
                Game.font,
                storedInput.ToString(),
                new(
                    drawX + borderSize,
                    drawY + borderSize
                ),
                fontSize,
                1,
                Color.Black);
        }
    }

    void addInput(int value) {
        visible = true;
        currentActionRunTime = Util.getCurrentTimeMillis(sendTime);

        input += value;
        storedInput = input;

        if (input.ToString().Length >= maxLength) {
            sendInput();
        }
        else {
        }
    }

    void sendInput() {
        if (input != "") {
            int inputValue = int.Parse(input) - 1;
            if (colorPicker.isColorActive(inputValue)) {
                colorPicker.setColor(inputValue);
                wrongInput = false;
            }
            else {
                wrongInput = true;
            }

            input = "";
        }

        currentActionRunTime = Util.getCurrentTimeMillis(hideTime);
    }
}