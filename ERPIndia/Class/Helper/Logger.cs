using log4net;
using System;

namespace ERPIndia.Utilities
{
    public static class Logger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Logger));

        public static void Info(string message)
        {
            _log.Info(message);
        }

        public static void Warning(string message)
        {
            _log.Warn(message);
        }

        public static void Error(string message)
        {
            _log.Error(message);
        }

        public static void Error(string message, Exception ex)
        {
            _log.Error(message, ex);
        }

        public static void Debug(string message)
        {
            _log.Debug(message);
        }
    }
}
