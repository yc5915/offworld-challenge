//using UnityEngine;
//using System;
//using System.IO;
//using System.Diagnostics;
//using System.Collections.Generic;

//namespace Offworld.SystemCore
//{
//    //modified with permission from FLog.cs implementation for Servo
//    public class MLog
//    {
//        private static StreamWriter mLogFile;
//        private static string mFilenameWithoutExtension;
//        private static string mExtension;
//        private static int mKeepOldCount;
//        private static bool mAnyErrors = false;
//        private static bool mRecordingErrors = true;
//        private static string mFirstError = "";

//        private static LogglyManager logglyManager;
        
//        // For mutex.
//        private static object mLock = new object();
//        private static DateTime mStartTime;
//        [ThreadStatic] private static bool mIgnoreUnityOutput; //ThreadStatic creates a seperate variable for each thread
//        [ThreadStatic] private static bool mIgnoreLogglyCooldown; //ThreadStatic creates a seperate variable for each thread

//        public static event Action<string> OutputEvent;
//        public static event Action<string> WarningEvent;
//        public static event Action<string> ErrorEvent;

//        public static bool AnyErrors            { get { return mAnyErrors;              } set { mAnyErrors = value;             } }
//        public static bool RecordingErrors      { get { return mRecordingErrors;        } set { mRecordingErrors = value;       } }
//        public static string FirstErrorMessage  { get { return mFirstError;             } set { mFirstError = value;            } }
//        public static bool IgnoreUnityOutput    { get { return mIgnoreUnityOutput;      } set { mIgnoreUnityOutput = value;     } }
//        public static bool IgnoreLogglyCooldown { get { return mIgnoreLogglyCooldown;   } set { mIgnoreLogglyCooldown = value;  } }

//        private static string GetOldFilename(int revisionNumber)
//        {
//            return mFilenameWithoutExtension + "_old" + revisionNumber + "." + mExtension;
//        }
        
//        public static void AddFilenameList(List<string> files)
//        {
//            if(!string.IsNullOrEmpty(mFilenameWithoutExtension))
//            {
//                files.Add(mFilenameWithoutExtension + "." + mExtension);
//                for(int i=1; i<=mKeepOldCount; i++)
//                    files.Add(GetOldFilename(i));
//            }
//        }

//        private static void UnityLogCallback(string logString, string stackTrace, LogType type)
//        {
//            if(mIgnoreUnityOutput)
//                return;

//            switch (type)
//            {
//                case LogType.Error:     LogError(logString, stackTrace);  break;
//                case LogType.Assert:    LogError(logString, stackTrace);  break;
//                case LogType.Exception: LogError(logString, stackTrace);  break;
//                case LogType.Warning:   LogWarning(logString);            break;
//                case LogType.Log:       Log(logString);                   break;
//                default:                LogError(logString, stackTrace);  break;
//            }

//            if (logglyManager != null)
//            {
//                logglyManager.Log(logString, stackTrace, type, mIgnoreLogglyCooldown);
//            }
//        }

//        private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
//        {
//            Exception e = (Exception) args.ExceptionObject;
//            if(e.InnerException != null)
//                LogError("UnhandledException: " + e.InnerException.Message, e.InnerException.StackTrace);
//            LogError("UnhandledException: " + e.Message, e.StackTrace);
//        }

//        //Call Init with a null filepath to recieve messages without any log files
//        public static void Init(string filepath, int keepOldCount = 3, string logglyToken = "", bool useLoggly = false, bool logglyIndividualMessages = false)
//        {
//            using (new UnityProfileScope("MLog.Init"))
//            {
//                //listen for Unity output and unhandled exceptions
//                try
//                {
//                    mStartTime = DateTime.Now;
//                    Application.logMessageReceivedThreaded -= UnityLogCallback;
//                    Application.logMessageReceivedThreaded += UnityLogCallback;
//                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

//                    if (filepath == null) //initialize without a log file
//                    {
//                        mKeepOldCount = 0;
//                        mFilenameWithoutExtension = "";
//                        mExtension = "";
//                        mLogFile = null;
//                    }
//                    else
//                    {
//                        mKeepOldCount = keepOldCount;
//                        int extPos = filepath.LastIndexOf(".");
//                        if (extPos >= 0)
//                        {
//                            mFilenameWithoutExtension = filepath.Substring(0, extPos);
//                            mExtension = filepath.Substring(extPos + 1);
//                        }
//                        else
//                        {
//                            mFilenameWithoutExtension = filepath;
//                            mExtension = "";
//                        }

//                        for (int destOldIndex = keepOldCount; destOldIndex >= 2; destOldIndex--)
//                        {
//                            string dest = GetOldFilename(destOldIndex);
//                            string src = GetOldFilename(destOldIndex - 1);
//                            FileUtilities.MoveFileWithOverwrite(src, dest);
//                        }

//                        if (keepOldCount > 0)
//                            FileUtilities.MoveFileWithOverwrite(filepath, GetOldFilename(1));

//                        FileUtilities.ValidateFilePathExists(filepath);
//                        FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
//                        mLogFile = new StreamWriter(stream);
//                        UnityEngine.Debug.Log("Logfile is located at: " + filepath);
//                        mLogFile.Flush();
//                    }

//                    if (!String.IsNullOrEmpty(logglyToken))
//                    {
//                        logglyManager = new LogglyManager(logglyToken, logglyIndividualMessages, useLoggly);
//                    }

//                }
//                catch (Exception ex)
//                {
//                    UnityEngine.Debug.LogWarning("Logfile failed to initialize: " + ex);

//                    mKeepOldCount = 0;
//                    mFilenameWithoutExtension = "";
//                    mExtension = "";
//                    mLogFile = null;
//                }
//            }
//        }
        
//        public static void Log(string text)
//        {
//            if(OutputEvent != null)
//                OutputEvent(text);
            
//            Output(text, true, true);
//        }
        
//        public static void LogWarning(string text)
//        {
//            if(WarningEvent != null)
//                WarningEvent(text);
            
//            Output(text, true, true);
//        }
        
//        public static void LogError(string text, string stackTrace = null)
//        {
//            //HACK to skip annoying Input device errors
//            if(text.Contains("<RI"))
//                return;

//            if(ErrorEvent != null)
//                ErrorEvent(text);

//            //record first error
//            bool firstError = false;
//            if(mRecordingErrors && !mAnyErrors)
//            {
//                firstError = true;
//                mAnyErrors = true;
//            }

//            Output("\n------------------- Error ----------------------------", true, false);
//            Output(text, true, true);
//            if (stackTrace != null)
//                Output(stackTrace, false, false);
//            Output("------------------------------------------------------\n", true, false);

//            if(firstError)
//            {
//                mFirstError = "\n------------------- Error ----------------------------\n"
//                              + text + "\n"
//                              + ((stackTrace != null) ? stackTrace : "")
//                              + "------------------------------------------------------\n";
//            }
//        }

//        private static void Output(string text, bool writeLine, bool writeTime)
//        {
//            lock(mLock)
//            {
//                if(mLogFile != null)
//                {
//                    //output time as "MM:SS - "
//                    if(writeTime)
//                    {
//                        int time = (int)(DateTime.Now - mStartTime).TotalSeconds;
//                        int seconds = time % 60;
//                        int minutes = time / 60;
//                        if(minutes < 10)
//                            mLogFile.Write('0');
//                        mLogFile.Write(minutes);
//                        mLogFile.Write(':');
//                        if(seconds < 10)
//                            mLogFile.Write('0');
//                        mLogFile.Write(seconds);
//                        mLogFile.Write(" - ");
//                    }

//                    if (writeLine)
//                        mLogFile.WriteLine(text);
//                    else
//                        mLogFile.Write(text);

//                    mLogFile.Flush();
//                }
//            }
//        }
        
//        public static void Close()
//        {
//            lock(mLock)
//            {
//                if(mLogFile != null)
//                    mLogFile.Close();
//                mLogFile = null;

//                if (logglyManager != null && !String.IsNullOrEmpty(mFilenameWithoutExtension))
//                    logglyManager.SendFinalLog(new StreamReader(mFilenameWithoutExtension + "." + mExtension), mAnyErrors);
//            }
//        }

//        public static void Update()
//        {
//            if (logglyManager != null)
//                logglyManager.Update();
//        }

//        public struct IgnoreUnityOutputScope : IDisposable
//        {
//            private bool oldValue;

//            public IgnoreUnityOutputScope(bool newValue)
//            {
//                oldValue = IgnoreUnityOutput;
//                IgnoreUnityOutput = newValue;
//            }

//            public void Dispose()
//            {
//                IgnoreUnityOutput = oldValue;
//            }
//        }

//        public struct IgnoreLogglyCooldownScope : IDisposable
//        {
//            private bool oldValue;

//            public IgnoreLogglyCooldownScope(bool newValue)
//            {
//                oldValue = IgnoreLogglyCooldown;
//                IgnoreLogglyCooldown = newValue;
//            }

//            public void Dispose()
//            {
//                IgnoreLogglyCooldown = oldValue;
//            }
//        }
//    }
//}