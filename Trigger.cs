using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ErrorReporting.App_Code.Nagios
{
    public class Trigger
    {
        //Work as a thread
        public Trigger()
        {
            Task.Factory.StartNew(() =>
            {
                DBCheckTrigger();
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    LogReader();
                    Thread.Sleep(5000);
                }
            }, TaskCreationOptions.LongRunning);
        }
        public static void DBCheckTrigger()
        { //loop for Checkresponse
            while (true)
            {
                Response.DBSendDataCheckResponse();
                Thread.Sleep(100);
            }
        }
        public static void LogReader()
        {
            string text;
            int errorCount = 0;
            List<Tuple<string, string>> errorDescriptions = new List<Tuple<string, string>>();
            int warningCount = 0;
            List<Tuple<string, string>> warningDescriptions = new List<Tuple<string, string>>();
            int errorLevel = 1;
            string description = "";
            int descriptioncount = 0;

            using (StreamReader sr = new StreamReader(@"D:\000000000_ErrorReporting\sort0000\run\Save_Log\03122831.log"))
            {
                while (!sr.EndOfStream)
                {
                    text = sr.ReadLine();
                    string[] fields = text.Split(new char[] { '\t' });

                    // Error log is in field 0
                    if (fields[0] == "@3")
                    {
                        errorCount++;
                        errorDescriptions.Add(new Tuple<string, string>((fields[1] + " " + fields[2]), fields[9]));
                    }
                    else if (fields[0] == "@2")
                    {
                        warningCount++;
                        warningDescriptions.Add(new Tuple<string, string>((fields[1] + " " + fields[2]), fields[9]));
                    }
                }
            }
            //Critical
            if (errorCount > 0)
            {
                errorLevel = 3;

                foreach (var errorDescription in errorDescriptions)
                {
                    DateTime dt = DateTime.ParseExact(errorDescription.Item1, "dd.MM.yyyy HH:mm:ss,fff", CultureInfo.InvariantCulture);

                    if (DateTime.Now <= dt.AddMinutes(5))
                    {
                        description += errorDescription.Item2 + "\n";
                        descriptioncount++;
                    }
                }
            }
            //Warning
            if ((warningCount > 0) && (descriptioncount == 0))
            {
                errorLevel = 2;

                foreach (var warningDescription in warningDescriptions)
                {
                    DateTime dt = DateTime.ParseExact(warningDescription.Item1, "dd.MM.yyyy HH:mm:ss,fff", CultureInfo.InvariantCulture);
                    if (DateTime.Now <= (dt.AddMinutes(5)))
                    {
                        description += warningDescription.Item2 + "\n";
                        descriptioncount++;
                    }
                }
            }
            // Normal
            if (descriptioncount == 0)
            {
                errorLevel = 1;
                description = "No Error in Logs.";
            }
            Response.LogResponse(errorLevel, description);
        }
    }
}