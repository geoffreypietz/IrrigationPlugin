using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using Scheduler;

namespace HSPI_RACHIOSIID
{
    public class WebPageAddDevice : PageBuilderAndMenu.clsPageBuilder
    {

        public WebPageAddDevice(string pagename) : base(pagename)
        {
        }

        public override string postBackProc(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);

            return base.postBackProc(page, data, user, userRights);
        }



        // build and return the actual page
        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder stb = new StringBuilder();

            try
            {
                this.reset();

                stb.Append("This is the add device config");

                return stb.ToString();
            }
            catch (Exception)
            {
                //WriteMon("Error", "Building page: " & ex.Message)
                return "error";
            }
        }


    }

}
