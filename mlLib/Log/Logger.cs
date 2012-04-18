namespace mlLib.Log
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    /// <summary>
    /// Enum that stores the supported log levels
    /// </summary>
    public enum LogLevel : int
    {
        /// <summary>
        /// Informational message should be logged using this level. Usually debug messages.
        /// </summary>
        Info = 1,

        /// <summary>
        /// Warning messages should be logged using this level.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error messages should be logged using this level.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Application progress should be logged using this level. Gets written to system out.
        /// </summary>
        Progress = 8
    };

    /// <summary>
    /// Class implements logging utility for the lib.
    /// </summary>
    public static class Logger
    {
        #region Public Members

        /// <summary>
        /// The log stream to write log messages to.
        /// </summary>
        public static TextWriter LogWriter = System.Console.Out;

        /// <summary>
        /// Setting indicating what types of messages should be logged.
        /// </summary>
        public static int OnLogLevel = (int)(LogLevel.Progress | LogLevel.Info | LogLevel.Warning | LogLevel.Error);

        #endregion

        #region Public Methods

        /// <summary>
        /// Method used to log data.
        /// </summary>
        /// <param name="level">log level of the message</param>
        /// <param name="message">message to log</param>
        /// <param name="other">additional data</param>
        public static void Log(LogLevel level, string message, params object[] other)
        {
            // check to see if log level is enabled
            if (((int)level & OnLogLevel) != (int)level)
            {
                return;
            }

            if (LogLevel.Progress == level)
            {
                System.Console.Out.Write(
                    string.Format(message, other));
            }
            else
            {
                LogWriter.WriteLine(
                    string.Format(message, other));
            }
        }

        #endregion
    }
}
