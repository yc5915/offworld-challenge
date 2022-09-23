using Offworld.SystemCore;

public class VersionAndMod
{
    private static ulong mHeader = 3740122863UL;
    private static int mMajor = 1;
    private static int mMinor = 23;
    private static int mChangelist = 44549;
    private static bool mInternalBuild = false;
    private static string mModName = "NONE";
    private static int mCRC = 0;

    public static int Minor => mMinor;

    public static void Serialize(object stream)
    {
        SimplifyIO.Data(stream, ref mHeader, "header");
        SimplifyIO.DataInt32(stream, ref mMajor, "major");
        SimplifyIO.DataInt32(stream, ref mMinor, "minor");
        SimplifyIO.DataInt32(stream, ref mChangelist, "changelist");
        SimplifyIO.Data(stream, ref mInternalBuild, "internalbuild");
        SimplifyIO.Data(stream, ref mModName, "modName");
        SimplifyIO.DataInt32(stream, ref mCRC, "CRC");
    }
}
