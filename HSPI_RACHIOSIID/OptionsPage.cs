using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using Scheduler;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using HSPI_RACHIOSIID.Models;

namespace HSPI_RACHIOSIID
{
    public class OptionsPage : PageBuilderAndMenu.clsPageBuilder
    {

        public string apiKey { get; set; }
        public string unitType { get; set; }
        public int updateInterval { get; set; }
        public int forecastDays { get; set; }
        public bool deviceImage { get; set; }
        public int accessLevel { get; set; }
        public string loggingLevel { get; set; }
        public int updateCount { get; set; }
        public int updateSuccess { get; set; }

        public OptionsPage(string pagename) : base(pagename)
        {
            string data = System.IO.File.ReadAllText(@"Data/hspi_rachiosiid/userprefs.txt");
            updateInterval = 1;
        }

        public override string postBackProc(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);

            Console.WriteLine("boop");

            if (parts["id"] == "tokentb")
            {
                Console.WriteLine("new api token");
            }
            if (parts[""] == "")
            {

            }

            return base.postBackProc(page, data, user, userRights);
        }




        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder pluginSB = new StringBuilder();
            OptionsPage page = this;

            try
            {
                page.reset();

                // handle queries with special data
                System.Collections.Specialized.NameValueCollection parts = null;
                if ((!string.IsNullOrEmpty(queryString)))
                {
                    parts = HttpUtility.ParseQueryString(queryString);
                }
                if (parts != null)
                {
                    if (parts["myslide1"] == "myslide_name_open")
                    {
                        // handle a get for tab content
                        string name = parts["name"];
                        return ("<table><tr><td>cell1</td><td>cell2</td></tr><tr><td>cell row 2</td><td>cell 2 row 2</td></tr></table>");
                        //Return ("<div><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br></div>")
                    }
                    if (parts["myslide1"] == "myslide_name_close")
                    {
                        return "";
                    }
                }

                pluginSB.Append("<link rel='Stylesheet' href='/hspi_rachiosiid/css/style.css' type='text / css' />");
                page.AddHeader(pluginSB.ToString());

                //page.RefreshIntervalMilliSeconds = 5000
                // handler for our status div
                //stb.Append(page.AddAjaxHandler("/devicestatus?ref=3576", "stat"))
                pluginSB.Append(this.AddAjaxHandlerPost("action=updatetime", this.PageName));
                


                // page body starts here

                this.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME + " Controls Test", "", "", false, true));

                //Dim dv As DeviceClass = GetDeviceByRef(3576)
                //Dim CS As CAPIStatus
                //CS = dv.GetStatus

                // Status/Options Tabs
                pluginSB.Append("<hr>RachioSIID Status/Options<br><br>");
                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("optionsTab", this.PageName);

                // Status Tab
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab.tabTitle = "Status";
                tab.tabDIVID = "rachiosiid-status";

                var statusString = new StringBuilder();
                statusString.Append("placeholder");

                tab.tabContent = statusString.ToString();
                jqtabs.postOnTabClick = true;
                jqtabs.tabs.Add(tab);

                // Options Tab
                tab = new clsJQuery.Tab();
                tab.tabTitle = "Options";
                tab.tabDIVID = "rachiosiid-options";

                var optionsString = new StringBuilder();

                optionsString.Append("<table id='optionstable'>");

                // Rachio API Access Token
                optionsString.Append("<tr><td class='header' colspan='2'>Rachio API Access Token</td></tr>");
                optionsString.Append("<tr><td>API Access Token</td>");
                optionsString.Append("<td>");
                clsJQuery.jqTextBox tokenTextBox = new clsJQuery.jqTextBox("tokentb", "text", "", this.PageName, 30, true);
                tokenTextBox.promptText = "Enter your Rachio API access token.";
                tokenTextBox.toolTip = "Access Token";
                tokenTextBox.dialogWidth = 600;
                optionsString.Append(tokenTextBox.Build());
                optionsString.Append("</td></tr>");

                // Rachio Options
                optionsString.Append("<tr><td class='header' colspan='2'>Rachio Options</td></tr>");
                
                optionsString.Append("<tr><td>Unit Type</td>");
                optionsString.Append("<td>");
                clsJQuery.jqDropList dl = new clsJQuery.jqDropList("unittype", this.PageName, false);
                dl.toolTip = "Select your preferred units.";
                dl.AddItem("U.S. customary  units (miles, °F, etc...)", "1", true);
                dl.AddItem("Metric system units (kms, °C, etc...)", "2", false);
                dl.autoPostBack = true;
                optionsString.Append(dl.Build());
                dl.ClearItems();
                optionsString.Append("</td></tr>");

                optionsString.Append("<tr><td>Update Frequency</td>");
                optionsString.Append("<td>");
                dl.toolTip = "Specify how often should the RachioSIID update from the Rachio API servers.";

                dl.AddItem("1 Minute", "1", true);
                for (int i=2; i<61; i++)
                {
                    dl.AddItem(i.ToString() + " Minutes", i.ToString(), false);
                }
                                
                dl.autoPostBack = true;
                optionsString.Append(dl.Build());
                dl.ClearItems();
                optionsString.Append("</td></tr>");

                // Homeseer Device Options
                optionsString.Append("<tr><td class='header' colspan='2'>Homeseer Device Options</td></tr>");

                optionsString.Append("<tr><td>Forecast Days</td>");
                optionsString.Append("<td>");
                dl.toolTip = "Specify the number of days to create weather forecast devices.";


                dl.AddItem("Disabled", "1", false);
                dl.AddItem("Today's Forecast", "2", true);
                dl.AddItem("Today's and Tomorrow's Forecast", "3", false);
                for (int i = 4; i < 9; i++)
                {
                        dl.AddItem((i-2).ToString() + " Day Forecast", i.ToString(), false);
                }

                dl.autoPostBack = true;
                optionsString.Append(dl.Build());
                dl.ClearItems();
                optionsString.Append("</td></tr>");

                optionsString.Append("<tr><td>Device Image</td>");
                optionsString.Append("<td>");
                dl.toolTip = "Select your preferred units.";
                dl.AddItem("Yes", "1", true);
                dl.AddItem("No", "2", false);
                dl.autoPostBack = true;
                optionsString.Append(dl.Build());
                dl.ClearItems();
                optionsString.Append("</td></tr>");

                // Web Page Access

                optionsString.Append("<tr><td class='header' colspan='2'>Web Page Access</td></tr>");

                optionsString.Append("<tr><td>Forecast Days</td>");
                optionsString.Append("<td>");
                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("FormCheckbox", "userroles", "post"));

                clsJQuery.jqCheckBox guestCheck = new clsJQuery.jqCheckBox("guestCheck", "Guest", this.PageName, true, false);
                guestCheck.@checked = false;
                optionsString.Append(guestCheck.Build());

                clsJQuery.jqCheckBox adminCheck = new clsJQuery.jqCheckBox("adminCheck", "Admin", this.PageName, true, false);
                adminCheck.@checked = true;
                adminCheck.enabled = false;
                optionsString.Append(adminCheck.Build());

                clsJQuery.jqCheckBox normalCheck = new clsJQuery.jqCheckBox("normalCheck", "Normal", this.PageName, true, false);
                normalCheck.@checked = false;
                optionsString.Append(normalCheck.Build());

                clsJQuery.jqCheckBox localCheck = new clsJQuery.jqCheckBox("localCheck", "Local", this.PageName, true, false);
                localCheck.@checked = false;
                optionsString.Append(localCheck.Build());

                optionsString.Append("</td></tr>");

                // Applications Options
                optionsString.Append("<tr><td class='header' colspan='2'>Applications Options</td></tr>");
                optionsString.Append("<tr><td>Logging Level</td>");
                optionsString.Append("<td>");
                dl.toolTip = "Specifies the plugin logging level";

                var logLevel = new string[10] {"", "Emergency", "Alert", "Critical", "Error", "Warning", "Notice", "", "Trace", "Debug"};
                for (int i = 1; i < 10; i++)
                {
                    if(i==7)
                        dl.AddItem("Informational", i.ToString(), true);
                    else
                        dl.AddItem(logLevel[i], i.ToString(), false);
                }

                dl.autoPostBack = true;
                optionsString.Append(dl.Build());
                dl.ClearItems();
                optionsString.Append("</td></tr>");

                optionsString.Append("</table>");

                

                tab.tabContent = optionsString.ToString();
                jqtabs.tabs.Add(tab);

                pluginSB.Append(jqtabs.Build());




                // container test
                //Dim statCont As New clsJQuery.jqContainer("contid", "Office Lamp", "/homeseer/on.gif", 100, 100, "this is the content")
                //stb.Append(statCont.build)


                pluginSB.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());



            }
            catch (Exception ex)
            {
                pluginSB.Append("Status/Options error: " + ex.Message);
            }
            pluginSB.Append("<br>");


            page.AddBody(pluginSB.ToString());

            return page.BuildPage();
        }
    }

}
