using System;
using System.IO;
using Offworld.GameCore;
using Offworld.SystemCore;
using UnityEngine;

public class ReplayFile
{
    private static readonly int MS_BETWEEN_UPDATES = 50;
    private static readonly float SEC_BETWEEN_UPDATES = MS_BETWEEN_UPDATES / 1000f;

    private Stream mLog;
    private bool mPlayBack;
    public ReplayFile(Stream playbackReplayStream = null)
    {
        mLog = new MemoryStream();
        object readerWriter;
        if (playbackReplayStream == null)
        {
            readerWriter = new BinaryWriter(mLog);
            mPlayBack = false;
        }
        else
        {
            LZF.DecompressStream(playbackReplayStream, mLog);
            mLog.Position = 0;
            readerWriter = new BinaryReader(mLog);
            mPlayBack = true;
        }
        VersionAndMod.Serialize(readerWriter);
        GameModeType gameMode = Globals.AppInfo.GameMode;
        SimplifyIO.Data(readerWriter, ref gameMode, "GameMode");
    }

    ~ReplayFile()
    {
        Close();
    }

    public void Close(Stream replayStream = null)
    {
        if (!mLog.CanSeek)
            return;

        if (replayStream != null && !mPlayBack)
        {
            BinaryWriter binaryWriter = new BinaryWriter(mLog);
            long num1 = 0L;
            ulong num2 = 3740122863;
            binaryWriter.Write(num2);
            binaryWriter.Write(num1);

            mLog.Position = 0;
            LZF.CompressStream(mLog, replayStream);
            //Debug.Log("[Replay] Compressing replay");
        }

        mLog.Close();
    }
    public bool Write(byte[] data)
    {
        float time = AppMain.gApp.gameServer().getSystemUpdateCount() * SEC_BETWEEN_UPDATES;
        byte[] bytes1 = BitConverter.GetBytes(time);
        mLog.Write(bytes1, 0, bytes1.Length);
        byte[] bytes2 = BitConverter.GetBytes(data.Length);
        mLog.Write(bytes2, 0, bytes2.Length);
        mLog.Write(data, 0, data.Length);
        return true;
    }

    public byte[] Read()
    {
        if (mLog.Position >= mLog.Length - 16)
            return null;

        byte[] buffer = new byte[4];

        if (mLog.Read(buffer, 0, buffer.Length) != buffer.Length) // time
            throw new Exception("ReplayFile.Read: Invalid data in replay file. Is someone cheating?");


        if (mLog.Read(buffer, 0, buffer.Length) != buffer.Length) // message length
            throw new Exception("ReplayFile.Read: Invalid data in replay file. Is someone cheating?");

        int msgLength = BitConverter.ToInt32(buffer, 0);
        if (msgLength <= 0)
            throw new Exception("ReplayFile.Read: Invalid data in replay file. Is someone cheating?");

        buffer = new byte[msgLength];
        if (mLog.Read(buffer, 0, msgLength) != msgLength)
            throw new Exception("ReplayFile.Read: Invalid data in replay file. Is someone cheating?");

        return buffer;
    }
}
