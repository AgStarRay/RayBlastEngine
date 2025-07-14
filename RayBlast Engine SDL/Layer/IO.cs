namespace RayBlast;

public static class IO {
    public static DirectoryInfo GameDirectoryInfo => RayBlastEngine.CurrentDirectoryInfo;
    public static DirectoryInfo PersistentDirectoryInfo => RayBlastEngine.RoamingAppDataDirectoryInfo;
    public static DirectoryInfo PrivateDirectoryInfo => RayBlastEngine.LocalAppDataDirectoryInfo;
}
