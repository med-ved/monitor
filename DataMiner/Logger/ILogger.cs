namespace DataMiner.Logger
{
    using System;

    public interface ILogger
    {
        bool LogToConsole { get; set; }

        void WriteLine(string fileName, string msg);
        void Log(string msg);
        void Log(string msg, params object[] values);
        void LogError(string msg);
        void LogError(Exception e, string msg = "");
    }
}
