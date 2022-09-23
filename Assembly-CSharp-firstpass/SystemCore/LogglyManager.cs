//using System;
//using System.Threading;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using System.IO;

//namespace Offworld.SystemCore
//{

//    public struct LogInfo
//    {
//        public string message;
//        public string stackTrace;
//        public LogType type;
//        public int logID;
//        public DateTime dateTime;

//        public LogInfo(string message, string stackTrace, LogType type, int logID, DateTime dateTime)
//        {
//            this.message = message;
//            this.stackTrace = stackTrace;
//            this.type = type;
//            this.logID = logID;
//            this.dateTime = dateTime;
//        }
//    }


//    public class LogglyManager
//    {

//        private const int COOL_DOWN_MINUTES = 60;
//        private const int MESSAGE_TIMER_SECONDS = 60;
//        private const int MAX_MESSAGES_IN_TIMER = 200;
//        private const int MAX_BACKLOG_MESSAGES = 100;

//        private string token;
//        private Guid myGuid;

//        private int globalLogID = 0;

//        private static bool ignoreMessages = false;
//        private bool isActive = true;

//        private static bool? sendIndividual = null;
//        public static bool SendIndividual {
//            get { return sendIndividual.GetValueOrDefault(true); }
//            set {
//                if (value != sendIndividual) LogToUnity("[LogglyManager] Sending " + (value ? "INDIVIDUAL messages" : "REPORT at end"));
//                sendIndividual = value;
//            }
//        }

//        // Settings from Remote Settings
//        private static bool? activateOnError = null;
//        private static bool? sendFullLogIfError = null;
//        private static bool? isEnabledRemotely = null;
//        public static bool ActivateOnError {
//            get { return activateOnError.GetValueOrDefault(); }
//            set {
//                if (value != activateOnError) LogToUnity("[LogglyManager] Activate on error is " + (value ? "ON" : "OFF"));
//                activateOnError = value;
//            }
//        }
//        public static bool SendFullLogIfError {
//            get { return sendFullLogIfError.GetValueOrDefault(); }
//            set {
//                if (value != sendFullLogIfError) LogToUnity("[LogglyManager] Send full log if error is " + (value ? "ON" : "OFF"));
//                sendFullLogIfError = value;
//            }
//        }
//        public static bool IsEnabledRemotely {
//            get { return isEnabledRemotely.GetValueOrDefault(true); }
//            set {
//                if (value != isEnabledRemotely) LogToUnity("[LogglyManager] " + (value ? "ENABLED" : "DISABLED"));
//                isEnabledRemotely = value;
//            }
//        }

//        private static object mLock = new object();

//        // for cooldown when overflow of messages
//        private DateTime messageMinuteTimer;
//        private int messageCountLastMinute = 0;
//        private DateTime cooldownBegin;
//        private bool onCooldown = false;

//        private Queue<UnityWebRequest> requests;

//        // backlog
//        private Queue<LogInfo> backlog;
//        private bool releaseBacklog = false;

//        public LogglyManager(string token, bool sendIndividualMessages, bool activate = true)
//        {
//            this.token = token;
//            SendIndividual = sendIndividualMessages;
//            myGuid = Guid.NewGuid();
//            requests = new Queue<UnityWebRequest>();
//            backlog = new Queue<LogInfo>();
//            isActive = activate;
//        }

//        public void SetActive(bool active)
//        {
//            isActive = active;
//        }

//        private void SendToBacklog(string text, string stackTrace, LogType type, int logID, DateTime dateTime)
//        {
//            lock (mLock)
//            {
//                LogInfo newLog = new LogInfo(text, stackTrace, type, logID, dateTime);
//                backlog.Enqueue(newLog);
//                if (backlog.Count > MAX_BACKLOG_MESSAGES)
//                    backlog.Dequeue();
//            }
//        }

//        private void Log(LogInfo logInfo, bool ignoreCooldown)
//        {
//            SendToLoggly(logInfo.message, logInfo.stackTrace, logInfo.type, logInfo.logID, logInfo.dateTime, ignoreCooldown);
//        }

//        public void Log(string text, string stackTrace, LogType type, bool ignoreCooldown)
//        {
//            if (!isActive)
//            {
//                // backlog messages in case we need to send them later
//                SendToBacklog(text, stackTrace, type, Interlocked.Increment(ref globalLogID), DateTime.Now);

//                // if we error and we should activate, release the backlog and activate loggly
//                if (type != LogType.Log && type != LogType.Warning && ActivateOnError)
//                {
//                    releaseBacklog = true;
//                    isActive = true;
//                }
//            }
//            else
//            {
//                SendToLoggly(text, stackTrace, type, Interlocked.Increment(ref globalLogID), DateTime.Now, ignoreCooldown);
//            }
//        }

//        private void SendToLoggly(string text, string stackTrace, LogType type, int logID, DateTime dateTime, bool ignoreCooldown)
//        {
//            lock (mLock)
//            {
//                if (!IsEnabledRemotely)
//                    return;

//                if (!ignoreCooldown)
//                {
//                    if (onCooldown)
//                    {
//                        if ((DateTime.Now - cooldownBegin).TotalMinutes > COOL_DOWN_MINUTES)
//                        {
//                            onCooldown = false;
//                            Log("Loggly Manager message cooldown over, resuming message logging.", null, LogType.Error, false);
//                        }
//                        else
//                        {
//                            return;
//                        }
//                    }
//                    else
//                    {
//                        if ((DateTime.Now - messageMinuteTimer).TotalSeconds < MESSAGE_TIMER_SECONDS)
//                        {
//                            messageCountLastMinute++;
//                        }
//                        else
//                        {
//                            messageCountLastMinute = 1;
//                            messageMinuteTimer = DateTime.Now;
//                        }

//                        if (messageCountLastMinute > MAX_MESSAGES_IN_TIMER)
//                        {
//                            messageCountLastMinute = 1;
//                            Log("Message Overflow... Stopping loggly messages for " + COOL_DOWN_MINUTES + " minutes.", null, LogType.Error, false);
//                            onCooldown = true;
//                            cooldownBegin = DateTime.Now;
//                            return;
//                        }
//                    }
//                }

//                if (ignoreMessages)
//                    return;

//                if (!SendIndividual)
//                    return;

//                string modifiedText = "";

//                // add error and stack trace if needed
//                if (type != LogType.Log && type != LogType.Warning)
//                    modifiedText += "\n------------------- Error ----------------------------\n";

//                modifiedText += "[" + dateTime.ToString("hh:mm:ss") + "] - " + text + "\n";

//                if (type != LogType.Log) 
//                    modifiedText += stackTrace;

//                if (type != LogType.Log && type != LogType.Warning)
//                    modifiedText += "\n------------------------------------------------------\n";


//                // get the log type to tag the loggly entry
//                string logTypeTag = "";
//                switch (type)
//                {
//                    case LogType.Error: logTypeTag = "Error"; break;
//                    case LogType.Assert: logTypeTag = "Assert"; break;
//                    case LogType.Exception: logTypeTag = "Exception"; break;
//                    case LogType.Warning: logTypeTag = "Warning"; break;
//                    case LogType.Log: logTypeTag = "Log"; break;
//                    default: logTypeTag = "Error"; break;
//                }

//                WWWForm formData = new WWWForm();
//                formData.AddField("message", modifiedText);
//                formData.AddField("GUID", myGuid.ToString());
//                formData.AddField("sendTime", (dateTime.Ticks + logID).ToString()); // if (dateTime.Ticks + logID) ever gets out of sync and becomes a problem between server instances, we should keep track of the last tick and just increment instead

//                UnityWebRequest www = UnityWebRequest.Post("http://logs-01.loggly.com/inputs/" + token + "/tag/Unity3D," + logTypeTag + "," + Application.productName.Replace(" ", "_"), formData);
//                www.timeout = 10;
//                www.Send();

//                requests.Enqueue(www);
//            }
//        }

//        public void Update()
//        {
//            lock (mLock)
//            {
//                if (!IsEnabledRemotely)
//                    return;

//                if (isActive)
//                {
//                    if (requests.Count != 0)
//                    {
//                        UnityWebRequest req = requests.Peek();
//                        if (req.isDone || req.isError)
//                        {
//                            requests.Dequeue();
//                            if (req.isError)
//                            {
//                                LogToUnity("[LogglyManager] " + req.error);
//                            }
//                        }
//                    }

//                    if (releaseBacklog && backlog.Count != 0)
//                    {
//                        Log(backlog.Dequeue(), true); //ignore cooldown for backlog messages
//                    }
//                }
//            }
//        }

//        private void WaitForRequest(UnityWebRequest www)
//        {
//            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
//            timer.Start();

//            while (!www.isDone && !www.isError && timer.Elapsed.TotalSeconds < 10)
//            {
//                System.Threading.Thread.Sleep(100);
//            }

//            timer.Stop();

//            lock (mLock)
//            {
//                if (timer.Elapsed.TotalSeconds >= 10)
//                {
//                    LogToUnity("[LogglyManager] message timed out");
//                }
//                if (www.isError)
//                {
//                    LogToUnity("[LogglyManager] " + www.error);
//                }
//            }
//        }

//        public void SendFinalLog(StreamReader stream, bool didError = false)
//        {
//            lock (mLock)
//            {
//                if (!IsEnabledRemotely)
//                    return;

//                if (SendFullLogIfError && didError)
//                {
//                    isActive = true;
//                    SendIndividual = false;
//                }

//                if (SendIndividual || !isActive)
//                    return;

//                WWWForm formData = new WWWForm();
//                formData.AddField("message", stream.ReadToEnd());
//                formData.AddField("GUID", myGuid.ToString());

//                UnityWebRequest www = UnityWebRequest.Post("http://logs-01.loggly.com/inputs/" + token + "/tag/Unity3D,OutputFile" + (didError ? ",Error" : "") + "," + Application.productName.Replace(" ", "_"), formData);
//                www.timeout = 10;
//                www.Send();

//                stream.Close();

//                WaitForRequest(www);
//            }
//        }


//        private static void LogToUnity(string message)
//        {
//            ignoreMessages = true;
//            new Thread(() => { Debug.Log(message); }).Start();
//            ignoreMessages = false;
//        }

//        private static void LogWarningToUnity(string message)
//        {
//            ignoreMessages = true;
//            new Thread(() => { Debug.LogWarning(message); }).Start();
//            ignoreMessages = false;
//        }

//    }
//}