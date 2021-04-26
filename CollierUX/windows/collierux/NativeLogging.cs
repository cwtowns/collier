using Microsoft.ReactNative.Managed;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace collierux
{
    /// <summary>
    /// Native logging for troubleshooting.  In release modes you do not have the ability to see console.log statements.
    /// </summary>
    [ReactModule("nativeLogging", EventEmitterName = "nativeLoggingEmitter")]
    class NativeLogging
    {
        [ReactConstant("logFile")]
        public string logFile => LogFilePath;

        private static string LogFilePath;

        [ReactMethod("info")]
        public void Info(string logContent)
        {
            Serilog.Log.Logger.Information(logContent);
        }

        public static void ConfigureLogger()
        {
            LogFilePath = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs/nativeLogs.txt"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger.Information("Application started");
        }
    }
}
