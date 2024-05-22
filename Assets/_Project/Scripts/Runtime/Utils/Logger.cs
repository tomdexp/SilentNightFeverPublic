using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public static class Logger
    {
        public enum LogLevel
        {
            Trace, // Hyper precise logging
            Debug, // Logging helpful for debugging
            Info, // Logging helpful for understanding the flow
            Warning, // Logging for potential issues
            Error // Logging for errors
        }

        public enum LogType
        {
            Local,
            Client,
            Server,
        }

        // Customizing colors for different LogTypes. 
        private static readonly Dictionary<LogType, string> logTypeColors = new Dictionary<LogType, string>
        {
            { LogType.Local, "<color=#FFFDD7>" },
            { LogType.Client, "<color=#FFE4CF>" },
            { LogType.Server, "<color=#FF5BAE>" }
        };
        
        // Customizing colors for different LogLevels. 
         static readonly Dictionary<LogLevel, string> logLevelColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, "<color=white>" },
            { LogLevel.Debug, "<color=white>" },
            { LogLevel.Info, "<color=white>" },
            { LogLevel.Warning, "<color=yellow>" },
            { LogLevel.Error, "<color=red>" }
        };
        
        /// <summary>
        /// For very precise and verbose logging.
        /// </summary>
        public static void LogTrace(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            // Since trace is performance heavy, we only log it in development builds.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //LogInternal(logType, LogLevel.Trace, message, networkObject);
            LogInternal(LogLevel.Trace, logType, message, networkObject?.OwnerId, networkObject?.gameObject);
#endif
        }
        
        /// <summary>
        /// For all logging messages that are helpful for debugging.
        /// </summary>
        public static void LogDebug(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            // Since debug is performance heavy, we only log it in development builds.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //LogInternal(logType, LogLevel.Debug, message, networkObject);
            LogInternal(LogLevel.Debug, logType, message, networkObject?.OwnerId, networkObject?.gameObject);
#endif
        }
        
        /// <summary>
        /// For all logging messages that are helpful for understanding the flow.
        /// </summary>
        public static void LogInfo(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            //LogInternal(logType, LogLevel.Info, message, networkObject);
            LogInternal(LogLevel.Info, logType, message, networkObject?.OwnerId, networkObject?.gameObject);
        }
        
        /// <summary>
        /// For non-critical issues that should be logged.
        /// </summary>
        public static void LogWarning(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            //LogInternal(logType, LogLevel.Warning, message, networkObject);
            LogInternal(LogLevel.Warning, logType, message, networkObject?.OwnerId, networkObject?.gameObject);
        }
        
        /// <summary>
        /// For all errors that should be logged.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        /// <param name="networkObject"></param>
        public static void LogError(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            //LogInternal(logType, LogLevel.Error, message, networkObject);
            LogInternal(LogLevel.Error, logType, message, networkObject?.OwnerId, networkObject?.gameObject);
        }
        
        public static void LogTrace<T>(string message, LogType type = LogType.Local, T context = default)
        {
            LogInternal(LogLevel.Trace, type, message, context);
        }

        public static void LogDebug<T>(string message, LogType type = LogType.Local, T context = default)
        {
            LogInternal(LogLevel.Debug, type, message, context);
        }

        public static void LogInfo<T>(string message, LogType type = LogType.Local, T context = default)
        {
            LogInternal(LogLevel.Info, type, message, context);
        }

        public static void LogWarning<T>(string message, LogType type = LogType.Local, T context = default)
        {
            LogInternal(LogLevel.Warning, type, message, context);
        }

        public static void LogError<T>(string message, LogType type = LogType.Local, T context = default)
        {
            LogInternal(LogLevel.Error, type, message, context);
        }
        
        public static void Log<T>(LogLevel level, LogType type, string message, T context = default)
        {
            LogInternal(level, type, message, context);
        }
        
        // Unified internal logging method for both generic and non-generic use
        private static void LogInternal<T>(LogLevel level, LogType type, string message, T context)
        {
            string ownerIdTag = "";
            string className = typeof(T).Name + ".cs";

            if (context is NetworkBehaviour nb)
            {
                ownerIdTag = $" <color=#FFC0D9>[OwnerId:{nb.NetworkObject.OwnerId}]</color>";
            }

            string logMessage = FormatMessage(type, level, message, className, ownerIdTag);
            GameObject gameObject = (context as NetworkBehaviour)?.gameObject;

            OutputLog(level, logMessage, gameObject);
        }

        private static void LogInternal(LogLevel level, LogType type, string message, int? ownerID, GameObject context)
        {
            string ownerIdTag = ownerID.HasValue ? $" <color=#FFC0D9>[OwnerId:{ownerID.Value}]</color>" : "";
            string logMessage = FormatMessage(type, level, message, "NetworkObject", ownerIdTag);
            OutputLog(level, logMessage, context);
        }
        
        private static void OutputLog(LogLevel level, string message, GameObject context)
        {
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    if (context != null) Debug.Log(message, context);
                    else Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    if (context != null) Debug.LogWarning(message, context);
                    else Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    if (context != null) Debug.LogError(message, context);
                    else Debug.LogError(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
        
        private static string FormatMessage(LogType type, LogLevel level, string message, string className, string ownerIdTag)
        {
            string typeColor = logTypeColors.GetValueOrDefault(type, "<color=white>");
            string levelColor = logLevelColors.GetValueOrDefault(level, "<color=white>");

            return $"{typeColor}[{type}]</color> {levelColor}[{level}]</color> <color=grey>[{className}]</color>{ownerIdTag} {message}";
        }
    }
}
