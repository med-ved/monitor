﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiner
{
    class Logger
    {
        private static string logFileName = "stdlog.txt";
        private static string errorsLogFileName = "errors.txt";

        private static void WriteLine(string fileName, string msg)
        {
            Console.WriteLine(msg);
            File.AppendAllText(fileName, "> " +DateTime.Now.ToString() + " > " + msg + Environment.NewLine);
        }

        public static void Log(string msg)
        {
            WriteLine(logFileName, msg);
        }

        public static void Log(string msg, params object[] values)
        {
            string message = String.Format(msg, values);
            Log(message);
        }

        public static void LogError(string msg)
        {
            WriteLine(errorsLogFileName, "ERROR: " + msg);
        }

        public static void LogError(Exception e, string msg = "")
        {
            WriteLine(errorsLogFileName, "");
            if (string.IsNullOrWhiteSpace(msg))
            {
                WriteLine(errorsLogFileName, msg);
            }

            WriteLine(errorsLogFileName, e.ToString());
        }
    }
}