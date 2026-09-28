using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
namespace Chud.Diagnostics
{
    public enum LogLevel
    {
        Trace = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public static class Log
    {
        private const string Prefix = "[Chud] ";

        public static LogLevel MinimumLevel = LogLevel.Info;

        private const float DuplicateSuppressSeconds = 5f;

        private const int MaxTrackedMessages = 256;

        private static readonly Dictionary<string, float> LastSeen = new Dictionary<string, float>(StringComparer.Ordinal);

        public static int SuppressedCount { get; private set; }

        public static int ErrorCount { get; private set; }

        public static void Trace(string message)
        {
            Emit(LogLevel.Trace, message, null);
        }

        public static void Info(string message)
        {
            Emit(LogLevel.Info, message, null);
        }

        public static void Warn(string message)
        {
            Emit(LogLevel.Warning, message, null);
        }

        public static void Warn(string message, Exception ex)
        {
            Emit(LogLevel.Warning, message, ex);
        }

        public static void Error(string message)
        {
            Emit(LogLevel.Error, message, null);
        }

        public static void Error(string message, Exception ex)
        {
            Emit(LogLevel.Error, message, ex);
        }

        public static void Guard(string context, Action action)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Emit(LogLevel.Error, context, ex);
            }
        }

        public static T Guard<T>(string context, Func<T> action, T fallback = default(T))
        {
            if (action == null)
            {
                return fallback;
            }

            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Emit(LogLevel.Error, context, ex);
                return fallback;
            }
        }

        public static void GuardCritical(string context, Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex) when (IsRecoverable(ex))
            {
                Emit(LogLevel.Error, context, ex);
            }
        }

        public static void ResetThrottle()
        {
            LastSeen.Clear();
            SuppressedCount = 0;
        }

        private static bool IsRecoverable(Exception ex)
        {
            return !(ex is OutOfMemoryException) && !(ex is StackOverflowException) && !(ex is ThreadAbortException);
        }

        private static void Emit(LogLevel level, string message, Exception ex)
        {
            if (level < MinimumLevel)
            {
                return;
            }

            if (level == LogLevel.Error)
            {
                ErrorCount++;
            }

            string text = (message ?? "<null>") + (ex == null ? string.Empty : " :: " + ex);

            if (level != LogLevel.Trace && !ShouldReport(text))
            {
                SuppressedCount++;
                return;
            }

            switch (level)
            {
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(Prefix + text);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(Prefix + text);
                    break;
                default:
                    UnityEngine.Debug.Log(Prefix + text);
                    break;
            }
        }

        private static bool ShouldReport(string text)
        {
            float now = Time.realtimeSinceStartup;
            if (LastSeen.TryGetValue(text, out float previous) && now - previous < DuplicateSuppressSeconds)
            {
                LastSeen[text] = now;
                return false;
            }

            if (LastSeen.Count >= MaxTrackedMessages)
            {
                LastSeen.Clear();
            }

            LastSeen[text] = now;
            return true;
        }
    }

    public static class Frame
    {
        public const string WristMenuUpdate = "WristMenu.Update";
        public const string WristMenuLateUpdate = "WristMenu.LateUpdate";
        public const string ModsUpdate = "Mods.Update";
        public const string ModsLateUpdate = "Mods.LateUpdate";
        public const string ModsActive = "Mods.UpdateActiveMods";
        public const string ModsPostTick = "Mods.ApplyRigVisuals";
    }
}