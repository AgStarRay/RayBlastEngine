namespace RayBlast;

public static class Time {
    public static int frameCount;
    public static int renderedFrameCount;
    public static double timeScale = 1.0;
    public static double unscaledDeltaTime;
    public static double deltaTime;
    public static double time;
    public static double unscaledTime;
    public static double maximumDeltaTime;
    public static double dspTime;
    public static double accumulatedUpdateTime;
    public static double accumulatedRenderTime;

    private static double fps;
    private static double averageUpdateTime;
    private static double averageRenderTime;
    private static double averageOverheadTime;
    private static double timeOfLastFPS;
    private static int frameCountOfLastFPS;

    public static double FPS {
        get {
            UpdateProfiling();
            return fps;
        }
    }
    public static double AverageUpdateTime {
        get {
            UpdateProfiling();
            return averageUpdateTime;
        }
    }
    public static double AverageRenderTime {
        get {
            UpdateProfiling();
            return averageRenderTime;
        }
    }
    public static double AverageOverheadTime {
        get {
            UpdateProfiling();
            return averageOverheadTime;
        }
    }
    //TODO: Move the profiling from Spirit Drop to RayBlast

    private static void UpdateProfiling() {
        if(renderedFrameCount > frameCountOfLastFPS && unscaledTime > timeOfLastFPS + 1.0) {
            fps = (renderedFrameCount - frameCountOfLastFPS) / (unscaledTime - timeOfLastFPS);
            averageUpdateTime = accumulatedUpdateTime / (renderedFrameCount - frameCountOfLastFPS);
            averageRenderTime = accumulatedRenderTime / (renderedFrameCount - frameCountOfLastFPS);
            averageOverheadTime = (unscaledTime - timeOfLastFPS - accumulatedUpdateTime - accumulatedRenderTime)
                                / (renderedFrameCount - frameCountOfLastFPS);
            frameCountOfLastFPS = renderedFrameCount;
            timeOfLastFPS = unscaledTime;
            accumulatedUpdateTime = 0.0;
            accumulatedRenderTime = 0.0;
        }
    }
}
