using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;

namespace Offworld.SystemCore
{
    public static class CompressionUtilities
    {
        public static bool GZipCompress(string sourceFile, string destinationFile)
        {
            try
            {
                if(!File.Exists(sourceFile))
                {
                    Debug.LogWarning("[Compression] Source file not found: " + sourceFile);
                    return false; //failure
                }

                using (FileStream source = FileUtilities.OpenRead(sourceFile))
                using (FileStream destination = FileUtilities.OpenWrite(destinationFile))
                {
                    bool success = GZipCompress(source, destination);
                    return success;
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("[Compression] Failed to compress " + sourceFile + " -> " + destinationFile);
                Debug.LogException(ex);
                return false; //failure
            }
        }

        //gzip compresses the source and leaves both streams open
        public static bool GZipCompress(Stream source, Stream destination)
        {
            try
            {
                //https://msdn.microsoft.com/en-us/library/system.io.compression.gzipstream(v=vs.110).aspx
                using (GZipStream gzipStream = new GZipStream(destination, CompressionMode.Compress, true))
                {
                    source.CopyTo(gzipStream);
                }
                return true; //success
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return false; //failure
            }
        }

        public static bool GZipDecompress(string sourceFile, string destinationFile)
        {
            try
            {
                if(!File.Exists(sourceFile))
                {
                    Debug.LogWarning("[Compression] Source file not found: " + sourceFile);
                    return false; //failure
                }

                using (FileStream source = FileUtilities.OpenRead(sourceFile))
                using (FileStream destination = FileUtilities.OpenWrite(destinationFile))
                {
                    bool success = GZipDecompress(source, destination);
                    return success;
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("[Compression] Failed to decompress " + sourceFile + " -> " + destinationFile);
                Debug.LogException(ex);
                return false; //failure
            }
        }

        //gzip decompresses the source and leaves both streams open
        public static bool GZipDecompress(Stream source, Stream destination)
        {
            try
            {
                //https://msdn.microsoft.com/en-us/library/system.io.compression.gzipstream(v=vs.110).aspx
                using (GZipStream gzipStream = new GZipStream(source, CompressionMode.Decompress, true))
                {
                    gzipStream.CopyTo(destination);
                }
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return false; //failure
            }
        }

        //https://searchcode.com/codesearch/view/63835894/
        public static void CopyTo(this Stream source, Stream destination, int bufferSize = 8192)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (destination == null)
                throw new ArgumentNullException("destination");

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
            }
        }
    }
}