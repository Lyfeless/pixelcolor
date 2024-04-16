namespace PixelColor;

class Program {
    public static void Main() {
        // Game.init();
        // Game.run();
        // Game.cleanup();

        try {
            Game.init();
            Game.run();
            Game.cleanup();
        } catch (Exception e) {
            using FileStream stream = File.OpenWrite("crashlog.txt");
            using StreamWriter writer = new(stream);
            writer.WriteLine(e.ToString());
        }
    }
}