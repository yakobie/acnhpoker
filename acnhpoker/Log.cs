using System;
using System.IO;

namespace ACNHPoker
{

    class Log
    {
        private const string logFolder = @"save\";
        private const string logFile = @"log.csv";
        private const string logPath = logFolder + logFile;
        public Log()
        {
        }

        public static void logEvent(string Location, string Message)
        {
            if (!File.Exists(logPath))
            {
                string logheader = "Timestamp" + "," + "Form" + "," + "Message";

                using (StreamWriter sw = File.CreateText(logPath))
                {
                    sw.WriteLine(logheader);
                }
            }

            DateTime localDate = DateTime.Now;

            string newLog = localDate.ToString() + "," + Location + "," + Message;

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(newLog);
            }
        }
    }
}
