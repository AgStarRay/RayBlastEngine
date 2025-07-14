namespace RayBlast;

public static class DiscordIntegration {
    public static string activityDetails = "";
    public static bool activityInstance;
    public static string activityState = "";
    public static string assetsLargeImage = "";
    public static string assetsLargeText = "";
    public static string assetsSmallImage = "";
    public static string assetsSmallText = "";
    public static bool inParty;
    public static int partyCurrentSize;
    public static int partyMaxSize;
    public static string partyID = "";
    public static bool timestampsActive;
    public static long startTimestamp;
    public static long endTimestamp;

    public static bool Active => false;

    public static void Initialize() {
        #if !UNITY_WEBGL
        #endif
    }

    public static void Dispose() {
        #if !UNITY_WEBGL
        #endif
    }

    public static void UpdateNow() {
        #if !UNITY_WEBGL
        #endif
    }

    public static void RunCallbacks() {
        #if !UNITY_WEBGL
        #endif
    }

    //TODO_AFTER: Get rid of allocations
    public static void UpdateActivity() {
        #if !UNITY_WEBGL
        #endif
    }
}
