using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Scheduler;
using HSPI_Rachio_Irrigation_Plugin.Models;
using static Scheduler.PageBuilderAndMenu;
using Newtonsoft.Json;


namespace HSPI_Rachio_Irrigation_Plugin
{
    public class OptionsPage : PageBuilderAndMenu.clsPageBuilder
    {

        public string apiKey { get; set; }
        public string unitType { get; set; }
        public int updateInterval { get; set; }
        public string loggingType { get; set; }
        public List<bool> ZoneChecks { get; set; }
        public int updateCount { get; set; }
        public int updateSuccess { get; set; }

        public OptionsPage(string pagename) : base(pagename)
        {
            string json = Util.hs.GetINISetting("RACHIO", "login", "", Util.IFACE_NAME + ".ini");
            using (Login Login = RachioConnection.getLoginInfo(json))
            {
                if (Login != null)
                {
                    this.apiKey = Login.accessToken;
                    this.unitType = Login.units;
                    this.updateInterval = Login.updateFrequency;
                    this.loggingType = Login.loggingLevel;
                    ZoneChecks = new List<bool>();

                    if (Login.ZoneView == null)
                    {

                        for (int i = 0; i < 16; i++)
                        {
                            ZoneChecks.Add(true);
                        }
                    }
                    else
                    {

                        ZoneChecks = Login.ZoneView;
                    }
                }
            }
        }

        // Handles any controls on the Options Page
        public override string postBackProc(string page, string data, string user, int userRights)
        {

            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            Console.WriteLine(data);
            string id = parts["id"];
            if (parts["APIToken"] != null)
            {
                Console.WriteLine("api");
                
                apiKey = parts["APIToken"];
                Console.WriteLine(apiKey);
            }
            Console.WriteLine("1");
            if (id == "unitType")
            {
                if (data.Contains("1"))
                {
                    unitType = "US";
                }
                else
                {
                    unitType = "METRIC";
                }

            }
            Console.WriteLine("2");
            if (id == "updateInterval")
            {
                updateInterval = Int16.Parse(data.Substring(33));
                Console.WriteLine(updateInterval + " minute update interval");
                HSPI.test_timer.Interval = 60000* updateInterval;
                Console.WriteLine("Time interval set to " + HSPI.test_timer.Interval / 1000 + " seconds");

            }
            Console.WriteLine("3");
            if (id == "loggingType")
            {
                if (data == "Off")
                {
                    loggingType = "Off";
                }
                else
                {
                    loggingType = "Debug";
                }

            }
            Console.WriteLine("4");
            if (data.Contains("ZoneCheck"))
            {
                if (id.Contains("ZoneCheck"))
                {
                    string zString = data.Split('=')[0].Substring(9);
                    int zNum = Int16.Parse(zString);
                    if (data.Contains("unchecked"))
                    {
                        ZoneChecks[zNum - 1] = false;
                    }
                    else
                    {
                        ZoneChecks[zNum - 1] = true;
                    }
                } 
            }
            try
            {
                Console.WriteLine("5");
                using (var login = new Login(apiKey, unitType, updateInterval, loggingType, ZoneChecks))
                {
                    string json = JsonConvert.SerializeObject(login);
                    Console.WriteLine(json);
                    Util.hs.SaveINISetting("RACHIO", "login", json, Util.IFACE_NAME + ".ini");
                    Console.WriteLine("Saved Preferences");
                }
                return base.postBackProc(page, data, user, userRights);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.StackTrace);
            }

            return null;
        }




        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder pluginSB = new StringBuilder();
            OptionsPage page = this;

            try
            {
                page.reset();

                // handle queries with special data
                /*System.Collections.Specialized.NameValueCollection parts = null;
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
                }*/

                this.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME, "", "", false, true));
                //pluginSB.Append("<link rel = 'stylesheet' href = 'HSPI_Rachio_Irrigation_Plugin/css/style.css' type = 'text/css' /><br>");
                //page.AddHeader(pluginSB.ToString());



                //page.RefreshIntervalMilliSeconds = 5000
                // handler for our status div
                //stb.Append(page.AddAjaxHandler("/devicestatus?ref=3576", "stat"))
                pluginSB.Append(this.AddAjaxHandlerPost("action=updatetime", this.PageName));



                // page body starts here




                pluginSB.Append(clsPageBuilder.DivStart("pluginpage", ""));
                //Dim dv As DeviceClass = GetDeviceByRef(3576)
                //Dim CS As CAPIStatus
                //CS = dv.GetStatus

                pluginSB.AppendLine("<table class='full_width_table' cellspacing='0' width='100%' >");
                pluginSB.AppendLine("<tr><td  colspan='1' >");
                // Status/Options Tabs

                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("optionsTab", this.PageName);

                // Options Tab
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab = new clsJQuery.Tab();
                tab.tabTitle = "Options";
                tab.tabDIVID = "rachiosiid-options";

                var optionsString = new StringBuilder();

                optionsString.Append("<table cellspacing='0' cellpadding='5'  width='100%'>");

                // Rachio API Access Token
                optionsString.Append("<tr><td class='tableheader' colspan='2'>Rachio API Access Token</td></tr>");
                optionsString.Append("<tr><td class='tablecell'>API Access Token</td>");
                optionsString.Append("<td class='tablecell'>");
                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform1", "testpage", "post"));


                clsJQuery.jqTextBox tokenTextBox = new clsJQuery.jqTextBox("APIToken", "text", apiKey, this.PageName, 30, true);
                tokenTextBox.promptText = "Enter your Rachio API access token.";
                tokenTextBox.toolTip = "Access Token";
                tokenTextBox.dialogWidth = 600;
                optionsString.Append(tokenTextBox.Build());



                clsJQuery.jqOverlay ol = new clsJQuery.jqOverlay("ov1", this.PageName, false, "events_overlay");
                ol.toolTip = "Help with API Access Token";
                ol.label = "Help?";

                clsJQuery.jqButton apiBut = new clsJQuery.jqButton("apilink", "Rachio-API", this.PageName, true);
                apiBut.url = "https://app.rach.io/";

                ol.overlayHTML = PageBuilderAndMenu.clsPageBuilder.FormStart("overlayformm", "testpage", "post");
                ol.overlayHTML += "<div>If you don't have an<br>access token saved, follow<br>the button link below to<br>navigate to the Rachio API.<br>Sign in and copy the <br>API Access Token within<br> the settings menu on the<br>top-right of the page.<br><br>" + apiBut.Build() + "</div>";
                ol.overlayHTML += PageBuilderAndMenu.clsPageBuilder.FormEnd();
                optionsString.Append(ol.Build());
                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());
                optionsString.Append("</td></tr>");

                // Rachio Options
                optionsString.Append("<tr><td class='tableheader' colspan='2'>Rachio Options</td></tr>");

                optionsString.Append("<tr><td class='tablecell'>Unit Type</td>");
                optionsString.Append("<td class='tablecell'>");
                clsJQuery.jqDropList dl = new clsJQuery.jqDropList("unitType", this.PageName, false);
                dl.toolTip = "Select your preferred units.";
                if (unitType == null || unitType.Equals("US"))
                {
                    dl.AddItem("U.S. customary  units (miles, °F, etc...)", "1", true);
                    dl.AddItem("Metric system units (kms, °C, etc...)", "2", false);
                }
                else
                {
                    dl.AddItem("U.S. customary  units (miles, °F, etc...)", "1", false);
                    dl.AddItem("Metric system units (kms, °C, etc...)", "2", true);
                }
                dl.autoPostBack = true;
                optionsString.Append(dl.Build());

                clsJQuery.jqDropList dl2 = new clsJQuery.jqDropList("updateInterval", this.PageName, false);
                optionsString.Append("</td></tr>");

                optionsString.Append("<tr><td class='tablecell'>Update Frequency</td>");
                optionsString.Append("<td class='tablecell'>");
                dl2.toolTip = "Specify how often RachioSIID receives updates from the Rachio API servers.";

                for (int i = 2; i < 61; i++)
                {
                    dl2.AddItem(i.ToString() + " Minute(s)", i.ToString(), updateInterval == i);
                }

                dl2.autoPostBack = true;
                optionsString.Append(dl2.Build());

                optionsString.Append("</td></tr>");

                optionsString.Append("<tr><td class='tablecell'>Zones View</td>");
                optionsString.Append("<td class='tablecell'>");
                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform2", "testpage", "post"));

                clsJQuery.jqOverlay ol2 = new clsJQuery.jqOverlay("ov2", this.PageName, false, "events_overlay");
                ol2.toolTip = "Specify which Zone devices are in view";
                ol2.label = "Zones";
                ol2.overlayHTML = PageBuilderAndMenu.clsPageBuilder.FormStart("overlayformm", "testpage", "post");
                ol2.overlayHTML += "<div>Select which Zones are visible:<br><br>";

                for (int i = 1; i < 17; i++)
                {
                    clsJQuery.jqCheckBox zoneCheck = new clsJQuery.jqCheckBox("ZoneCheck" + i, "Zone " + i, this.PageName, true, false);
                    if (ZoneChecks != null)
                    {
                        zoneCheck.@checked = ZoneChecks[i - 1];
                    }
                    else
                    {
                        zoneCheck.@checked = true;
                    }
                    zoneCheck.enabled = true;
                    ol2.overlayHTML += zoneCheck.Build() + "<br>";
                }

                ol2.overlayHTML += "</div>";
                ol2.overlayHTML += PageBuilderAndMenu.clsPageBuilder.FormEnd();
                optionsString.Append(ol2.Build());
                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());
                optionsString.Append("</td></tr>");

                // Homeseer Device Options
                /*optionsString.Append("<tr><td class='header' colspan='2'>Homeseer Device Options</td></tr>");

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
                optionsString.Append("</td></tr>");*/

                // Web Page Access
                /*optionsString.Append("<tr><td class='header' colspan='2'>Web Page Access</td></tr>");

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

                optionsString.Append("</td></tr>");*/

                // Applications Options
                optionsString.Append("<tr><td class='tableheader' colspan='2'>Applications Options</td></tr>");
                optionsString.Append("<tr><td class='tablecell'>Logging Level</td>");
                optionsString.Append("<td class='tablecell'>");

                clsJQuery.jqDropList dl3 = new clsJQuery.jqDropList("loggingType", this.PageName, false);
                dl3.toolTip = "Specifiy the plugin logging level";
                if (loggingType == "Off")
                {
                    dl3.AddItem("Off", "1", true);
                    dl3.AddItem("Debug", "2", false);
                }
                else
                {
                    dl3.AddItem("Off", "1", false);
                    dl3.AddItem("Debug", "2", true);
                }
                /*var logLevel = new string[10] {"", "Emergency", "Alert", "Critical", "Error", "Warning", "Notice", "", "Trace", "Debug"};
                for (int i = 1; i < 10; i++)
                {
                    if(i==7)
                        dl.AddItem("Informational", i.ToString(), true);
                    else
                        dl.AddItem(logLevel[i], i.ToString(), false);
                }*/

                dl3.autoPostBack = true;
                optionsString.Append(dl3.Build());

                optionsString.Append("</td></tr>");

                optionsString.Append("</table>");



                tab.tabContent = optionsString.ToString();
                jqtabs.tabs.Add(tab);

                pluginSB.Append(jqtabs.Build());
                pluginSB.AppendLine("</td></tr></table>");


                // container test
                //Dim statCont As New clsJQuery.jqContainer("contid", "Office Lamp", "/homeseer/on.gif", 100, 100, "this is the content")
                //stb.Append(statCont.build)






            }
            catch (Exception ex)
            {
                pluginSB.Append("Status/Options error: " + ex.Message);
            }
            pluginSB.Append("<br>");

            pluginSB.Append(DivEnd());
            page.AddBody(pluginSB.ToString());

            return page.BuildPage();
        }
    }

}
