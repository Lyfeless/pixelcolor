namespace PixelColor;

class Timer {
    public long triggerTime;
    public Delegate finishCallback;
}

static class TimerController {
    static readonly List<Timer> timers = new();

    public static void update() {
        long currentMilliseconds = Util.getCurrentTimeMillis();

        for (int i = timers.Count - 1; i >= 0; --i) {
            if (timers[i].triggerTime < currentMilliseconds) {
                timers[i].finishCallback.DynamicInvoke();
                timers.RemoveAt(i);
            }
        }
    }

    public static Timer createTimer(int milliseconds, Action callback) {
        Timer timer = new() { triggerTime = Util.getCurrentTimeMillis(milliseconds), finishCallback = callback };
        timers.Add(timer);
        return timer;
    }

    public static void clearTimer(Timer timer) {
        timers.Remove(timer);
    }
}