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

    public class WebPage : PageBuilderAndMenu.clsPageBuilder
    {

        public WebPage(string pagename) : base(pagename)
        {
        }

        public override string postBackProc(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            // handle postbacks here
            // update DIV'S with:
            //Me.divToUpdate.Add(DIV_ID, HTML_FOR_DIV)
            // refresh a page (this page or other page):
            //Me.pageCommands.Add("newpage", url)
            // open a dialog
            //Me.pageCommands.Add("opendialog", "dyndialog")
            if (parts["id"] == "b1")
            {
                this.divToUpdate.Add("current_time", "This div was just updated with this");
            }
            if (parts["action"] == "updatetime")
            {
                // ajax timer has expired and posted back to us, update the time
                this.divToUpdate.Add("current_time", DateTime.Now.ToString());
                if (DateTime.Now.Second == 0)
                {
                    this.divToUpdate.Add("updatediv", "job complete");
                }
                else if (DateTime.Now.Second == 30)
                {
                    this.divToUpdate.Add("updatediv", "working...");
                }
            }

            return base.postBackProc(page, data, user, userRights);
        }



        // build and return the actual page
        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder stb = new StringBuilder();

            try
            {
                this.reset();

                // handle any queries like mode=something
                System.Collections.Specialized.NameValueCollection parts = null;
                if ((!string.IsNullOrEmpty(queryString)))
                {
                    parts = HttpUtility.ParseQueryString(queryString);
                }

                // add any custom menu items

                // add any special header items
                //page.AddHeader(header_string)

                // add the normal title
                this.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME + "", "", "", false, false));

                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("pluginpage", ""));

                // a message area for error messages from jquery ajax postback (optional, only needed if using AJAX calls to get data)
                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("errormessage", "class='errormessage'"));
                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());

                // specific page starts here

                stb.Append("<div id='current_time'>" + DateTime.Now.ToString() + "</div>\r\n");

                clsJQuery.jqButton b = new clsJQuery.jqButton("b1", "Button", Util.IFACE_NAME, false);
                stb.Append(b.Build());
                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());
                stb.Append("<br>pagename: " + pageName + "<br>");
                stb.Append("<br>This is instance: " + Util.Instance + "<br>");

                this.RefreshIntervalMilliSeconds = 2000;
                stb.Append(this.AddAjaxHandlerPost("action=updatetime", Util.IFACE_NAME));

                // add the body html to the page
                this.AddBody(stb.ToString());

                this.AddFooter(Util.hs.GetPageFooter());
                this.suppressDefaultFooter = true;

                // return the full page
                return this.BuildPage();
            }
            catch (Exception)
            {
                //WriteMon("Error", "Building page: " & ex.Message)
                return "error";
            }
        }


    }

}
