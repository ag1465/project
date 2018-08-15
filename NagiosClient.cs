using System;
using System.Collections.Generic;
using App_Code.Global;
using App_Code.Parameters;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using Newtonsoft.Json;

namespace App_Code.Nagios
{
    public static class NagiosClient
    {
        static string token;
        //Load Parameters in a Constructor
        static NagiosClient()
        {
            LoadParameters(); 
        }

        public static void ReportError(Checkresults checkresultitems)
        {
            //Generate the checkresults array to Json with each checkresult
            Checkresultitem[] checkresults = checkresultitems.ToArray();
            var checkresultswrapper = new { checkresults };
            string json = JsonConvert.SerializeObject(checkresultswrapper);
            
            ValidateCheckresult(checkresultitems);
            NRDPApi api = new NRDPApi(BeGlobalData.ApiConfig);
            // submit the json, token and submitcheck command
            ApiResponse<Resultwrapper> response = api.NrdpPostWithHttpInfo(token, "submitcheck", json);
        }
        //Load the token from the parameter excel file.
        public static void LoadParameters()
        {
            TKey key;
            TData Param;

            BeEXCL ExcelReader = new BeEXCL(BeGlobalData.ExcelPath, BeGlobalData.ExcelPath);

            // Class, Group, Element
            key = new TKey("WEB", "NAGIOS", "WEB1");

            //Read excel
            Param = ExcelReader.ReadPara("PARAMETER", key);

            if (Param.asPara.Count > 0 || Param.alPara.Count > 0)
            {
                if (Param.asPara[0][0] != null && Param.asPara[0][0] != "")
                    token = Param.asPara[0][0];
                else
                    throw new ApplicationException("Unable to find 'Token' parameter");
            }
            else
            {
                throw new ApplicationException("Unable to find the following parameter line: Class = " + key.sKey1 + ", Group = " + key.sKey2 + ", Element = " + key.sKey3);
            }
        }
        //makes sure the checkresults are written correctly or throw exceptions.
        public static void ValidateCheckresult(Checkresults checkresults)
        {
            int count = checkresults.Count;
            if(count == 0)
            {
                throw new ApplicationException("There is no checkresult in the list of checkresults.");
            }
            foreach (Checkresultitem checkresultitem in checkresults)
            {
                List<string> listtype = new List<string> { "host", "service" };
                string type = checkresultitem.Checkresult.Type;
                if (!listtype.Contains(type))
                {
                    throw new ApplicationException("Type can only be 'host' or 'service'");
                }
            }
            foreach (Checkresultitem checkresultitem in checkresults)
            {
                List<string> liststate = new List<string> { "0", "1", "2", "3" };
                string state = checkresultitem.State;
                if (!liststate.Contains(state))
                {
                    throw new ApplicationException("State can only be '0', '1','2','3'");
                }
            }
        }
    }
}

