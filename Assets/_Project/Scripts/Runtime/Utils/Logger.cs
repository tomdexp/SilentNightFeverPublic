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
            LogInternal(logType, LogLevel.Trace, message, networkObject);
#endif
        }
        
        /// <summary>
        /// For all logging messages that are helpful for debugging.
        /// </summary>
        public static void LogDebug(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            // Since debug is performance heavy, we only log it in development builds.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogInternal(logType, LogLevel.Debug, message, networkObject);
#endif
        }
        
        /// <summary>
        /// For all logging messages that are helpful for understanding the flow.
        /// </summary>
        public static void LogInfo(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            LogInternal(logType, LogLevel.Info, message, networkObject);
        }
        
        /// <summary>
        /// For non-critical issues that should be logged.
        /// </summary>
        public static void LogWarning(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            LogInternal(logType, LogLevel.Warning, message, networkObject);
        }
        
        /// <summary>
        /// For all errors that should be logged.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        /// <param name="networkObject"></param>
        public static void LogError(string message, LogType logType = LogType.Local, NetworkObject networkObject = null)
        {
            LogInternal(logType, LogLevel.Error, message, networkObject);
        }

        private static void LogInternal(LogType logType, LogLevel logLevel, string message, NetworkObject networkObject)
        {
            string logMessage = FormatMessage(logType, logLevel, message, networkObject);

            if (networkObject)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        Debug.Log(logMessage, networkObject.gameObject);
                        break;
                    case LogLevel.Debug:
                        Debug.Log(logMessage, networkObject.gameObject);
                        break;
                    case LogLevel.Info:
                        Debug.Log(logMessage, networkObject.gameObject);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(logMessage, networkObject.gameObject);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(logMessage, networkObject.gameObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }
            else
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        Debug.Log(logMessage);
                        break;
                    case LogLevel.Debug:
                        Debug.Log(logMessage);
                        break;
                    case LogLevel.Info:
                        Debug.Log(logMessage);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(logMessage);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(logMessage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }
        }

        private static string FormatMessage(LogType logType, LogLevel logLevel, string message, NetworkObject networkObject)
        {
            string logTypeColor = logTypeColors.GetValueOrDefault(logType, "<color=white>");
            string logLevelColor = logLevelColors.GetValueOrDefault(logLevel, "<color=white>");
            string ownerIdTag = networkObject != null ? $" [OwnerId:{networkObject.OwnerId}]" : "";

            // Apply color only to the log type and log level tags, leaving the message in the default color.
            return $"{logTypeColor}[{logType}]</color> {logLevelColor}[{logLevel}]</color>{ownerIdTag} {message}";
        }

    }
}
