using UnityEngine;
using System.IO;
using System.Linq;

namespace Offworld.SystemCore
{
    public static class FileUtilities
    {
        //directories should be separated by '/', http://answers.unity3d.com/questions/168337/pathname-slashbackslash.html
        public static void ValidateFilePathExists(string filePath)
        {
            filePath = ConvertDirectorySlashes(filePath);
            string directoryPath = new FileInfo(filePath).DirectoryName;
            ValidateDirectoryPathExists(directoryPath);
        }

        //directories should be separated by '/', http://answers.unity3d.com/questions/168337/pathname-slashbackslash.html
        public static void ValidateDirectoryPathExists(string directoryPath)
        {
            try
            {
                directoryPath = ConvertDirectorySlashes(directoryPath);
                if(!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError("[File] ValidateDirectoryPathExists failed for " + directoryPath);
                Debug.LogException(e);
            }
        }

        public static bool MoveFileWithOverwrite(string source, string destination)
        {
            //http://stackoverflow.com/questions/5920882/file-move-does-not-work-file-already-exists
            try
            {
                //check if file
                if(File.Exists(source))
                {
                    if (File.Exists(destination))
                        File.Delete(destination);

                    ValidateFilePathExists(destination);
                    File.Move(source, destination);
                    return true; //success
                }
                else if(Directory.Exists(source)) //handle moving directories
                {
                    Directory.Move(source, destination);
                    return true; //success
                }
            }
            catch(System.Exception ex)
            {
                Debug.LogWarning("MoveFile failed: " + ex);
            }

            return false; //failed
        }

        public static bool CopyFilesRecursive(string sourceDirectory, string targetDirectory, string searchPattern, string excludePattern = null)
        {
            try
            {
                //modified from http://stackoverflow.com/questions/58744/best-way-to-copy-the-entire-contents-of-a-directory-in-c-sharp
                string [] sourceFiles = Directory.GetFiles(sourceDirectory, searchPattern, SearchOption.AllDirectories);
                if(!string.IsNullOrEmpty(excludePattern))
                    sourceFiles = sourceFiles.Except(Directory.GetFiles(sourceDirectory, excludePattern, SearchOption.AllDirectories)).ToArray();

                foreach (string sourcePath in sourceFiles)
                {
                    string targetPath = sourcePath.Replace(sourceDirectory, targetDirectory);
                    FileUtilities.ValidateFilePathExists(targetPath);
                    File.Copy(sourcePath, targetPath, true);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        //directories should be separated by '/', http://answers.unity3d.com/questions/168337/pathname-slashbackslash.html
        //public static string GetUnityPathFromAbsolutePath(string absolutePath)
        //{
        //    string path = absolutePath.Replace(Application.dataPath, "Assets");
        //    path = ConvertDirectorySlashes(path);
        //    return path;
        //}

        //converts backslashes to cross platform forwardslashes, http://answers.unity3d.com/questions/168337/pathname-slashbackslash.html
        public static string ConvertDirectorySlashes(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string GetDirectoryPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static string GetUniqueFilenameSuffix()
        {
            System.DateTime epoch = new System.DateTime(2014, 1, 1);
            double seconds = (System.DateTime.UtcNow - epoch).TotalSeconds;
            return "" + (int)seconds;
        }

        //http://stackoverflow.com/questions/611921/how-do-i-delete-a-directory-with-read-only-files-in-c
        public static void SetReadOnlyAttributes(FileSystemInfo fileSystemInfo, bool value)
        {
            try
            {
                if(value)
                    fileSystemInfo.Attributes |= FileAttributes.ReadOnly;
                else
                    fileSystemInfo.Attributes &= ~FileAttributes.ReadOnly;
                
                //recurse sub-directories and files
                var directoryInfo = fileSystemInfo as DirectoryInfo;
                if (directoryInfo != null)
                {
                    foreach (var childInfo in directoryInfo.GetFileSystemInfos())
                    {
                        SetReadOnlyAttributes(childInfo, value);
                    }
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static bool IsReadOnly(string path)
        {
            return IsReadOnly(new FileInfo(path));
        }

        public static bool IsReadOnly(FileSystemInfo fileSystemInfo)
        {
            if(fileSystemInfo == null)
                return false;

            if(!fileSystemInfo.Exists)
                return false;

            if((fileSystemInfo.Attributes & FileAttributes.ReadOnly) == 0)
                return false;

            return true;
        }

        //http://stackoverflow.com/questions/611921/how-do-i-delete-a-directory-with-read-only-files-in-c
        public static bool DeleteFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            try
            {
                if (fileSystemInfo.Exists)
                {
                    bool success = true;
                    DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
                    if (directoryInfo != null)
                    {
                        foreach (FileSystemInfo childInfo in directoryInfo.GetFileSystemInfos())
                        {
                            success &= DeleteFileSystemInfo(childInfo);
                        }
                    }

                    fileSystemInfo.Attributes = FileAttributes.Normal;
                    fileSystemInfo.Delete();
                    return success;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("MoveFile failed: " + ex);
            }

            return false; //failure
        }

        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static FileStream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        }
    }
}