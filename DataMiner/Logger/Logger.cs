namespace DataMiner.Logger
{
    using System;
    using DataMiner.Database;

    public class Logger : ILogger
    {
        public bool LogToConsole { get; set; }
        private readonly string logFileName = "stdlog.txt";
        private readonly string errorsLogFileName = "errors.txt";

        private readonly IDatabase _db;

        public Logger(IDatabase db)
        {
            _db = db;
        }

        public void WriteLine(string fileName, string msg)
        {
            if (LogToConsole)
            {
                Console.WriteLine(DateTime.Now.ToString() + " > " + msg);
            }
            else
            {
                _db.WriteLog(fileName, msg);
            }
        }

        public void Log(string msg)
        {
            WriteLine(logFileName, msg);
        }

        public void Log(string msg, params object[] values)
        {
            string message = string.Format(msg, values);
            Log(message);
        }

        public void LogError(string msg)
        {
            WriteLine(errorsLogFileName, "ERROR: " + msg);
        }

        public void LogError(Exception e, string msg = "")
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                WriteLine(errorsLogFileName, msg);
            }

            WriteLine(errorsLogFileName, e.ToString());
        }
    }
}
