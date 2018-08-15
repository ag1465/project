using IO.Swagger.Model;
using App_Code.DB;
using App_Code.Nagios;


namespace ErrorReporting.App_Code.Nagios
{
    class Response
    {
        static string curstate = "";
        public static void DBSendDataCheckResponse()
        {
            using (DBConn DB = new DBConn())
            {
                // Collect count
                if (DB.Connect())
                {
                    // Temporary variables for count of rows in SendData
                    string sSQL = "SELECT count(*) AS Count FROM SendData";
                    long count = 0;
                    DB.OpenCursor(sSQL);
                    if (DB.HasRows())
                    {
                        while (DB.Fetch()) // Fetch row-by-row
                        {
                            count = DB.Col2Long("Count");
                        }
                    }
                    // 0 - 9: ok 10-24: Warning 25+: Critical 
                    Checkresults checkresults = new Checkresults();
                    if ((count >= 10) && (count < 25))
                    {
                        if (curstate != "1")
                        {
                            curstate = "1";
                            checkresults.Add(new Checkresultitem(new Checkresult("service"), "172.20.22.242", "SendData", "1", "Warning: SendData has more than 10 rows!"));
                            NagiosClient.ReportError(checkresults);
                        }
                    }
                    else if (count >= 25)
                    {
                        if (curstate != "2")
                        {
                            curstate = "2";
                            checkresults.Add(new Checkresultitem(new Checkresult("service"), "172.20.22.242", "SendData", "2", "Critical: SendData has more than 25 rows!"));
                            NagiosClient.ReportError(checkresults);
                        }
                    }
                    else
                    {
                        if (curstate != "0")
                        {
                            curstate = "0";
                            checkresults.Add(new Checkresultitem(new Checkresult("service"), "172.20.22.242", "SendData", "0", "SendData is Ok!"));
                            NagiosClient.ReportError(checkresults);
                        }
                    }
                }
            }
        }
        public static void LogResponse(int errorLevel, string description)
        {
            Checkresults checkresults = new Checkresults();

            checkresults.Add(new Checkresultitem(new Checkresult("service"), "172.20.22.242", "Log File 03122831", (errorLevel - 1).ToString(), description));
            NagiosClient.ReportError(checkresults);
        }
    }
}
