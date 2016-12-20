using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using VB = Microsoft.VisualBasic;
using System.Web;
using HomeSeerAPI;

namespace HSPI_RACHIOSIID
{

	static internal class Web_UI_Util
	{
		// HTTP constants
		public const string HTML_StartHead = "<head>\r\n";
		public const string HTML_EndHead = "</head>\r\n";
		public const string HTML_StartPage = "<html>\r\n";
		public const string HTML_EndPage = "</html>\r\n";
		public const string HTML_StartBody = "<body>\r\n";
		public const string HTML_EndBody = "</body>\r\n";
		//Public Const HTML_StartForm As String = "<form method=""post"">" & vbCrLf
		public const string HTML_EndForm = "</form>\r\n";
		public const string HTML_NewLine = "<br>\r\n";
		//Public Const HTML_StartPara As String = "<p>"
		//Public Const HTML_EndPara As String = "</p>" & vbCrLf
		//Public Const HTML_Line As String = "<hr noshade>"
		public const string HTML_Line = "<hr>";
		public const string HTML_EndTable = "</table>\r\n";
		public const string HTML_EndTableAlign = "</table></div>\r\n";
		//Public Const HTML_StartRow As String = "<tr>"
		public const string HTML_EndRow = "</tr>\r\n";
		public const string HTML_EndCell = "</td>";
		//Public Const ALIGN_RIGHT As Integer = 1 ' for cell alignment in tables
		//Public Const ALIGN_LEFT As Integer = 2
		//Public Const ALIGN_CENTER As Integer = 3
		//Public Const ALIGN_TOP As Integer = 4
		//Public Const ALIGN_BOTTOM As Integer = 5
		//Public Const ALIGN_MIDDLE As Integer = 6
		public const string HTML_StartBold = "<b>";
		public const string HTML_EndBold = "</b>";
		public const string HTML_StartItalic = "<i>";
		public const string HTML_EndItalic = "</i>";
		public const string HTML_StartHead2 = "<h2>";
		public const string HTML_EndHead2 = "</h2>";
		public const string HTML_StartHead3 = "<h3>";
		public const string HTML_EndHead3 = "</h3>";
		public const string HTML_StartHead4 = "<h4>";
		public const string HTML_EndHead4 = "</h4>";
		public const string HTML_StartTitle = "<title>";
		public const string HTML_EndTitle = "</title>\r\n";

		public const string HTML_EndFont = "</font>";
		public const string COLOR_WHITE = "#FFFFFF";
		public const string COLOR_KEWARE = "#0080C0";
		public const string COLOR_RED = "#FF0000";
		public const string COLOR_BLACK = "#000000";
		public const string COLOR_NAVY = "#000080";
		public const string COLOR_LT_BLUE = "#D9F2FF";
		public const string COLOR_LT_GRAY = "#E1E1E1";
		public const string COLOR_LT_PINK = "#FFB6C1";
		public const string COLOR_ORANGE = "#D58000";
		public const string COLOR_GREEN = "#008000";

		public const string COLOR_PURPLE = "#4B088A";
		/* Not used in sample causes warning
		internal struct EventWebControlInfo
		{
			public bool Decoded;
			public int EventTriggerGroupID;
			public int GroupID;
			public int EvRef;
			public int TriggerORActionID;
			public string Name_or_ID;
			public string Additional;
		}
		*/

		[Serializable()]
		public enum HTML_Align
		{
			RIGHT = 1,
			LEFT = 2,
			CENTER = 3,
			JUSTIFY = 4
		}

		[Serializable()]
		public enum HTML_VertAlign
		{
			TOP = 1,
			MIDDLE = 2,
			BOTTOM = 3,
			BASELINE = 4
		}

		[Serializable()]
		public enum HTML_TableAlign
		{
			RIGHT = 1,
			LEFT = 2,
			INHERIT = 3
		}


		static internal string gMultiConfigResult = "";
		#region "    HTML Public Functions     "

		// =====================================================================================================
		//                                  START OF HTML FUNCTIONS
		// =====================================================================================================

#if FALSE
	private static object AddNavLinkPlugin(ref string url, ref string label, bool bSelected = false, string AltText = "")
	{
		string st = "";
		 // ERROR: Not supported in C#: OnErrorStatement


		if (AltText == null) {
			AltText = " ";
		}
		//<input type="button" class="linkrowbutton" value="Webring" onClick="location.href='/misc/webring.asp'">
		if (bSelected) {
			st = "<input type=\"button\" class=\"functionrowbuttonselected ui-corner-all\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + url + "'\" onmouseover=\"this.className='functionrowbutton ui-corner-all';\" onmouseout=\"this.className='functionrowbuttonselected ui-corner-all';\">";
		} else {
			st = "<input type=\"button\" class=\"functionrowbutton ui-corner-all\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + url + "'\" onmouseover=\"this.className='functionrowbuttonselected ui-corner-all';\" onmouseout=\"this.className='functionrowbutton ui-corner-all';\">";
		}

		return st;
	}
#endif
		static internal string AddNavTransLink(ref string url, ref string label, bool bSelected = false, string AltText = "")
		{
			string st = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (AltText == null)
			{
				AltText = " ";
			}
			//<input type="button" class="linkrowbutton" value="Webring" onClick="location.href='/misc/webring.asp'">
			if (bSelected)
			{
				st = "<input type=\"button\" class=\"buttontrans ui-corner-all\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + url + "'\" onmouseover=\"this.className='buttontrans ui-corner-all';\" onmouseout=\"this.className='buttontrans ui-corner-all';\">";
			}
			else
			{
				st = "<input type=\"button\" class=\"buttontrans ui-corner-all\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + url + "'\" onmouseover=\"this.className='buttontrans ui-corner-all';\" onmouseout=\"this.className='buttontrans ui-corner-all';\">";
			}

			return st;
		}

		public static string HTML_Graphic(string file, int width, int height)
		{
			string st = "";
			if (width == 0)
			{
				st = "<img src=\"" + file + "\" />";
			}
			else
			{
				st = "<img src=\"" + file + "\" height=\"" + height.ToString() + "\"" + "width=\"" + width.ToString() + "\" />";
			}
			return st;
		}

		public static object AddHidden(ref string Name, ref string Value)
		{
			string st = "";
			// ERROR: Not supported in C#: OnErrorStatement


			st = "<input type=\"hidden\" name=\"" + Name + "\" value=\"" + Value + "\">";
			return st;
		}

		public static object AddHiddenWithID(ref string Name, ref string Value)
		{
			string st = "";
			// ERROR: Not supported in C#: OnErrorStatement


			st = "<input type=\"hidden\" ID=\"" + Name + "\" name=\"" + Name + "\" value=\"" + Value + "\">";
			return st;
		}

		public static string HTML_StartFont(string Color)
		{
			return HTML_Font(Color, "", "");
		}

		public static string HTML_Font(string color = "", string Style = "", string ClassName = "")
		{
			string st = "<font ";
			// ERROR: Not supported in C#: OnErrorStatement


			if (color != null && !string.IsNullOrEmpty(color.Trim()))
			{
				st += "color='" + color.Trim() + "' ";
			}
			if (ClassName != null && !string.IsNullOrEmpty(ClassName.Trim()))
			{
				st += "class='" + ClassName.Trim() + "' ";
			}
			if (Style != null && !string.IsNullOrEmpty(Style.Trim()))
			{
				st += "style='" + Style.Trim() + "' ";
			}
			st += ">";
			return st;
		}

		public static string HTML_StartCell(string Class_name, short colspan, HTML_Align align = 0, bool nowrap = false, int RowHeight = 0, string Style = "", HTML_VertAlign VertAlign = 0)
		{
			//Dim st As String = ""
			//Dim stalign As String = ""
			//Dim wrap As String = ""
			//Dim rheight As String = ""
			string s = "<td ";
			string TotalStyle = "";

			// ERROR: Not supported in C#: OnErrorStatement


			//If RowHeight > 0 Then
			//rheight = " height=""" & RowHeight.ToString & """"
			//End If
			if (Class_name != null && !string.IsNullOrEmpty(Class_name))
			{
				s += "class='" + Class_name + "' ";
			}

			if (Style != null && !string.IsNullOrEmpty(Style.Trim()))
			{
				TotalStyle = Style.Trim();
			}

			if (nowrap)
			{
				//wrap = " nowrap"
				if (!string.IsNullOrEmpty(TotalStyle.Trim()))
				{
					if (TotalStyle.Trim().EndsWith(";"))
					{
						// Nothing to do.
					}
					else
					{
						TotalStyle += ";";
					}
				}
				TotalStyle += " white-space: nowrap;";
			}

			if (RowHeight > 0)
			{
				if (!string.IsNullOrEmpty(TotalStyle.Trim()))
				{
					if (TotalStyle.Trim().EndsWith(";"))
					{
						// Nothing to do.
					}
					else
					{
						TotalStyle += ";";
					}
				}
				TotalStyle += " height:" + RowHeight.ToString() + "px;";
			}

			if (Enum.IsDefined(typeof(HTML_Align), align))
			{
				s += "align='";
				switch (align)
				{
					case HTML_Align.LEFT:
						s += "left";
						break;
					case HTML_Align.CENTER:
						s += "center";
						break;
					case HTML_Align.RIGHT:
						s += "right";
						break;
					case HTML_Align.JUSTIFY:
						s += "justify";
						break;
				}
				s += "' ";
			}

			if (Enum.IsDefined(typeof(HTML_VertAlign), VertAlign))
			{
				s += "valign='";
				switch (VertAlign)
				{
					case HTML_VertAlign.TOP:
						s += "top";
						break;
					case HTML_VertAlign.MIDDLE:
						s += "middle";
						break;
					case HTML_VertAlign.BOTTOM:
						s += "bottom";
						break;
					case HTML_VertAlign.BASELINE:
						s += "baseline";
						break;
				}
				s += "' ";
			}

			if (colspan > 0)
			{
				s += " colspan='" + colspan.ToString() + "' ";
			}

			if (!string.IsNullOrEmpty(TotalStyle))
			{
				s += " style='" + TotalStyle + "' ";
			}

			s += ">";

			return s;

		}

		public static string AddNBSP(short amount)
		{
			StringBuilder s = new StringBuilder();
			short x = 0;

			if (amount < 1)
			{
				return "";
			}
			for (x = 1; x <= amount; x++)
			{
				s.Append(" ");
			}
			return s.ToString();

		}


		//Public Const HTML_StartRow As String = "<tr>"
		public static string HTML_StartRow(string ClassName = "", string Style = "", HTML_Align Align = 0, string BackColor = "", HTML_VertAlign VertAlign = 0)
		{

			string s = "<tr ";

			// ERROR: Not supported in C#: OnErrorStatement


			if (ClassName != null && !string.IsNullOrEmpty(ClassName.Trim()))
			{
				s += "class='" + ClassName + "' ";
			}

			if (Style != null && !string.IsNullOrEmpty(Style))
			{
				s += "style='" + Style + "' ";
			}

			if (Enum.IsDefined(typeof(HTML_Align), Align))
			{
				s += "align='";
				switch (Align)
				{
					case HTML_Align.LEFT:
						s += "left";
						break;
					case HTML_Align.CENTER:
						s += "center";
						break;
					case HTML_Align.RIGHT:
						s += "right";
						break;
					case HTML_Align.JUSTIFY:
						s += "justify";
						break;
				}
				s += "' ";
			}

			if (BackColor != null && !string.IsNullOrEmpty(BackColor.Trim()))
			{
				s += "bgcolor='" + BackColor + "' ";
			}

			if (Enum.IsDefined(typeof(HTML_VertAlign), VertAlign))
			{
				s += "valign='";
				switch (VertAlign)
				{
					case HTML_VertAlign.TOP:
						s += "top";
						break;
					case HTML_VertAlign.MIDDLE:
						s += "middle";
						break;
					case HTML_VertAlign.BOTTOM:
						s += "bottom";
						break;
					case HTML_VertAlign.BASELINE:
						s += "baseline";
						break;
				}
				s += "' ";
			}

			s += ">";

			return s;

		}

		public static string HTML_StartTable(int border, int CellSpacing = -1, short TableWidthPercent = -1, string Style = "", string ClassName = "", HTML_TableAlign Align = 0, short CellPadding = -1, int TableWidthPixels = -1)
		{

			string s = "<table ";
			string TotalStyle = "";

			// ERROR: Not supported in C#: OnErrorStatement


			if (ClassName != null && !string.IsNullOrEmpty(ClassName))
			{
				s += "class='" + ClassName + "' ";
			}

			if (Style != null && !string.IsNullOrEmpty(Style.Trim()))
			{
				TotalStyle = Style.Trim();
			}


			if (Enum.IsDefined(typeof(HTML_TableAlign), Align))
			{
				if (!string.IsNullOrEmpty(TotalStyle.Trim()))
				{
					if (TotalStyle.Trim().EndsWith(";"))
					{
						// Nothing to do.
					}
					else
					{
						TotalStyle += ";";
					}
				}
				TotalStyle += " float:";
				switch (Align)
				{
					case HTML_TableAlign.LEFT:
						TotalStyle += "left";
						break;
					case HTML_TableAlign.RIGHT:
						TotalStyle += "right";
						break;
					case HTML_TableAlign.INHERIT:
						TotalStyle += "inherit";
						break;
				}
				TotalStyle += ";";
			}

			if (CellPadding >= 0)
			{
				s += "cellpadding='" + CellPadding.ToString() + "' ";
			}

			if (CellSpacing >= 0)
			{
				s += "cellspacing='" + CellSpacing.ToString() + "' ";
			}

			if (TableWidthPixels >= 0)
			{
				if (TableWidthPercent >= 0)
				{
					s += "width='" + TableWidthPercent.ToString() + "%' ";
				}
				else
				{
					s += "width='" + TableWidthPixels.ToString() + "' ";
				}
			}
			else
			{
				if (TableWidthPercent >= 0)
				{
					s += "width='" + TableWidthPercent.ToString() + "%' ";
				}
			}

			if (!string.IsNullOrEmpty(TotalStyle.Trim()))
			{
				s += "style='" + TotalStyle + "'";
			}

			s += ">";

			return s;

		}

		public static string AddLink(string dvRef, string label, object image = null, int Width = 0, int Height = 0, string ClassName = "")
		{
			string sReturn = "";
			string sClass = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (!string.IsNullOrEmpty(ClassName))
			{
				sClass = " class=\"" + ClassName + "\" ";
			}

			if ((image == null))
			{
				sReturn = "<a " + sClass + "href=\"" + dvRef + "\">" + label + "</a>\r\n";
			}
			else
			{
				if (Width == 0)
				{
					sReturn = "<a " + sClass + "href=\"" + dvRef + "\">" + label + "<img src=\"" + image + "\" border=\"0\"></a>\r\n";
				}
				else
				{
					sReturn = "<a " + sClass + "href=\"" + dvRef + "\">" + label + "<img src=\"" + image + "\" width=\"" + Width.ToString() + "\" height=\"" + Height.ToString() + "\" border=\"0\"></a>\r\n";
				}
			}
			return sReturn;
		}

		public static object AddNavLink(ref string dvRef, ref string label, bool bSelected = false, string AltText = "", string target = "")
		{
			string st = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (AltText == null)
			{
				AltText = " ";
			}
			//<input type="button" class="linkrowbutton" value="Webring" onClick="location.href='/misc/webring.asp'">
			if (bSelected)
			{
				if (!string.IsNullOrEmpty(target))
				{
					st = "<input type=\"button\" class=\"linkrowbuttonselected\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"window.open('" + dvRef + "','" + target + "')" + "\" onmouseover=\"this.className='linkrowbutton';\" onmouseout=\"this.className='linkrowbuttonselected';\">";
				}
				else
				{
					st = "<input type=\"button\" class=\"linkrowbuttonselected\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + dvRef + "'\" onmouseover=\"this.className='linkrowbutton';\" onmouseout=\"this.className='linkrowbuttonselected';\">";
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(target))
				{
					st = "<input type=\"button\" class=\"linkrowbutton\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"window.open('" + dvRef + "','" + target + "')" + "\" onmouseover=\"this.className='linkrowbuttonselected';\" onmouseout=\"this.className='linkrowbutton';\">";
				}
				else
				{
					st = "<input type=\"button\" class=\"linkrowbutton\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + dvRef + "'\" onmouseover=\"this.className='linkrowbuttonselected';\" onmouseout=\"this.className='linkrowbutton';\">";
				}
			}

			return st;
		}

		public static object AddNavLinkPlugin(ref string dvRef, ref string label, bool bSelected = false, string AltText = "")
		{
			string st = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (AltText == null)
			{
				AltText = " ";
			}
			//<input type="button" class="linkrowbutton" value="Webring" onClick="location.href='/misc/webring.asp'">
			if (bSelected)
			{
				st = "<input type=\"button\" class=\"linkrowbuttonselectedplugin\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + dvRef + "'\" onmouseover=\"this.className='linkrowbuttonplugin';\" onmouseout=\"this.className='linkrowbuttonselectedplugin';\">";
			}
			else
			{
				st = "<input type=\"button\" class=\"linkrowbuttonplugin\" value=\"" + label + "\" alt=\"" + AltText + "\" onClick=\"location.href='" + dvRef + "'\" onmouseover=\"this.className='linkrowbuttonselectedplugin';\" onmouseout=\"this.className='linkrowbuttonplugin';\">";
			}

			return st;
		}

		public static object AddFuncLink(ref string dvRef, ref string label, bool bSelected = false)
		{
			string st = "";
			// ERROR: Not supported in C#: OnErrorStatement

			//<input type="button" class="linkrowbutton" value="Webring" onClick="location.href='/misc/webring.asp'">
			if (bSelected)
			{
				st = "<input type=\"button\" class=\"functionrowbuttonselected\" value=\"" + label + "\" onClick=\"location.href='" + dvRef + "'\"  onmouseover=\"this.className='functionrowbutton';\" onmouseout=\"this.className='functionrowbuttonselected';\">";
			}
			else
			{
				st = "<input type=\"button\" class=\"functionrowbutton\" value=\"" + label + "\" onClick=\"location.href='" + dvRef + "'\" onmouseover=\"this.className='functionrowbuttonselected';\" onmouseout=\"this.className='functionrowbutton';\">";
			}

			return st;
		}

		public static string AddSWPB(short wWidth = 250, short wHeight = 150, string func_name = "openwc")
		{
			StringBuilder s = new StringBuilder();

			s.Append("<script language=\"JavaScript\">\r\n");
			s.Append("function " + func_name + "(parms)\r\n");
			s.Append("{\r\n");
			s.Append("var w = " + wWidth.ToString() + ", h = " + wHeight.ToString() + ";\r\n");
			s.Append("  w += 32;\r\n");
			s.Append("  h += 96;\r\n");
			s.Append("  wleft = (screen.width - w) / 2;\r\n");
			s.Append("  wtop = (screen.height - h) / 2;\r\n");
			s.Append("  var win = window.open('swpb?' + parms,\r\n");
			s.Append("        '_blank',\r\n");
			s.Append("        'width=' + w + ', height=' + h + ', ' +\r\n");
			s.Append("        'left=' + wleft + ', top=' + wtop + ', ' +\r\n");
			s.Append("        'location=no, menubar=no,titlebar=no, ' +\r\n");
			s.Append("        'status=no, toolbar=no, scrollbars=no, resizable=no');\r\n");
			s.Append("  win.resizeTo(w, h);\r\n");
			s.Append("  win.moveTo(wleft, wtop);\r\n");
			s.Append("  win.focus();\r\n");
			s.Append("}\r\n");
			s.Append("</script>\r\n");

			/*
			s.Append("<script language=\"JavaScript\">\r\n");
			s.Append("function " + func_name + "(parms) {\r\n");
			s.Append("var w = 800, h = 600;\r\n");
			s.Append("if (document.all) {\r\n");
			s.Append("   w = document.body.clientWidth;\r\n");
			s.Append("   h = document.body.clientHeight;\r\n");
			s.Append("}\r\n");
			s.Append("else if (document.layers) {\r\n");
			s.Append("   w = window.innerWidth;\r\n");
			s.Append("   h = window.innerHeight;\r\n");
			s.Append("}\r\n");
			s.Append("var popW = " + wWidth.ToString() + ", popH = " + wHeight.ToString() + ";\r\n");
			s.Append("var leftPos = (w-popW)/2, topPos = (h-popH)/2;\r\n");
			//s.Append("wp = window.open(""swpb?"" + parms, ""_blank"", ""width=" & ww.ToString & ",height=" & wh.ToString & ",location=no,menubar=no,resizable=yes,scrollbars=no,status=no,titlebar=no,toolbar=no"");" & vbCrLf)
			s.Append("wp = window.open('swpb?' + parms, '_blank', 'width=" + wWidth.ToString() + ",height=" + wHeight.ToString() + ",top='+topPos+',left='+leftPos+',location=no,menubar=no,resizable=no,scrollbars=no,status=no,titlebar=no,toolbar=no');\r\n");
			s.Append("if (wp != null)      wp.focus();  }\r\n");
			s.Append("</script>\r\n");
			*/
			return s.ToString();

		}

		public static string FormCheckBox(string label, string name, string Value, bool @checked, bool onChange = false)
		{
			string st = "";
			string chk = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (@checked)
			{
				chk = " checked ";
			}
			else
			{
				chk = "";
			}
			if (onChange)
			{
				st = "<input class=\"formcheckbox\" type=\"checkbox\"" + chk + " name=\"" + name + "\" value=\"" + Value + "\" onClick=\"submit();\" > " + label + "\r\n";
			}
			else
			{
				st = "<input class=\"formcheckbox\" type=\"checkbox\"" + chk + " name=\"" + name + "\" value=\"" + Value + "\" > " + label + "\r\n";
			}
			return st;
		}

		public static object FormCheckBoxEx(string label, string name, string Value, bool @checked, bool PrevNNL = false, bool NNL = false, short ColSpan = 0)
		{
			StringBuilder st = new StringBuilder();
			string chk = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (ColSpan < 1)
				ColSpan = 1;

			if (PrevNNL)
			{
				st.Append(HTML_StartCell("", ColSpan, HTML_Align.LEFT, true));
			}
			else
			{
				//st.Append(HTML_StartTable(0) & HTML_StartCell("", 1, ALIGN_LEFT, True))
				st.Append(HTML_EndRow + HTML_StartRow() + HTML_StartCell("", ColSpan, HTML_Align.LEFT, true));
			}

			if (@checked)
			{
				chk = " checked ";
			}
			else
			{
				chk = "";
			}

			st.Append("<input class=\"FormCheckBoxEx\" type=\"checkbox\"" + chk + " name=\"" + name + "\" value=\"" + Value + "\" > " + label + "\r\n");

			if (NNL)
			{
				st.Append(HTML_EndCell);
			}
			else
			{
				//st.Append(HTML_EndCell & HTML_EndTable)
				st.Append(HTML_EndCell + HTML_EndRow);
			}

			return st.ToString();
		}

		public static object FormRadio(string label, string name, string Value, bool @checked, bool OnChange = false)
		{
			string st = "";
			string chk = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (@checked)
			{
				chk = " checked ";
			}
			else
			{
				chk = "";
			}
			if (OnChange)
			{
				st = "<input class=\"formradio\" type=\"radio\"" + chk + " name=\"" + name + "\" value=\"" + Value + "\" onClick=\"submit();\" > " + label + "\r\n";
			}
			else
			{
				st = "<input class=\"formradio\" type=\"radio\"" + chk + " name=\"" + name + "\" value=\"" + Value + "\"> " + label + "\r\n";
			}

			return st;
		}

		public static object FormRadioEx(string label, string name, string Value, bool @checked, bool PrevNNL = false, bool NNL = false, short ColSpan = 0)
		{
			string st = "";
			string chk = "";
			// ERROR: Not supported in C#: OnErrorStatement


			if (ColSpan < 1)
				ColSpan = 1;

			if (PrevNNL)
			{
				st = HTML_StartCell("", ColSpan, HTML_Align.LEFT, true);
			}
			else
			{
				st = HTML_StartTable(0) + HTML_StartCell("", ColSpan, HTML_Align.LEFT, true);
			}

			if (@checked)
			{
				chk = " checked ";
			}
			else
			{
				chk = "";
			}
			st = st + "<input class=\"FormRadioEx\" type=\"radio\"" + chk + " name=\"" + name + "\" value=\"" + Value + "\"> " + label + "\r\n";

			if (NNL)
			{
				st = st + HTML_EndCell;
			}
			else
			{
				st = st + HTML_EndCell + HTML_EndTable;
			}

			return st;
		}

		// rjh 2.2.0.2
		public static string FormLinkButton(ref string URL, ref string label, int Width = -1, int Height = -1)
		{
			string st = "";
			string more = "";
			if (Util.StringIsNullOrEmpty(ref URL))
				URL = "";
			if (Util.StringIsNullOrEmpty(ref label))
				label = "";

			// ERROR: Not supported in C#: OnErrorStatement


			st = "<input type=\"button\" ";
			if (Width > 0 | Height > 0)
			{
				more = "";
				st += "style=\"";
				if (Width > 0)
				{
					st += "width:" + Width.ToString() + "px";
					more = ";";
				}
				if (Height > 0)
				{
					st += more + "height:" + Height.ToString();
				}
				st += "\" ";
			}
			st += "class=\"formbutton\" value=\"" + label + "\" ";
			st += "onClick=\"location.href='" + URL + "'\" ";
			st += " onmouseover=\"this.className='formbutton';\" onmouseout=\"this.className='formbutton';\">";
			//If Width > 0 Then
			//    Return "<input type=""button"" style=""width:" & Width.ToString & """ class=""formbutton"" value=""" & label & """ onClick=""location.href='" & URL & "'""  onmouseover=""this.className='formbutton';"" onmouseout=""this.className='formbutton';"">"
			//Else
			//    Return "<input type=""button"" class=""formbutton"" value=""" & label & """ onClick=""location.href='" & URL & "'""  onmouseover=""this.className='formbutton';"" onmouseout=""this.className='formbutton';"">"
			//End If
			return st;
		}

		[Serializable()]
		public class AddStyle
		{

			public string Build()
			{
				System.Collections.Generic.List<string> s = new System.Collections.Generic.List<string>();
				bool GotSomething = false;

				if (vBackgroundColor != ColorName._Unspecified_)
				{
					if (vBackgroundColor == ColorName._HexCode_Provided_)
					{
						if (!Util.StringIsNullOrEmpty(ref vBackgroundColorHex))
						{
							GotSomething = true;
							s.Add("background-color:" + vBackgroundColorHex);
						}
					}
					else
					{
						GotSomething = true;
						s.Add("background-color:" + vBackgroundColor.ToString());
					}
				}

				if (!Util.StringIsNullOrEmpty(ref vBackgroundImage))
				{
					GotSomething = true;
					s.Add("background-image:url(\"" + vBackgroundImage + "\")");
				}

				if (vBackgroundRepeat != Background_Repeat_enum._Unspecified_)
				{
					GotSomething = true;
					switch (vBackgroundRepeat)
					{
						case Background_Repeat_enum.Repeat_X:
							s.Add("background-repeat:repeat-x");
							break;
						case Background_Repeat_enum.Repeat_Y:
							s.Add("background-repeat:repeat-y");
							break;
						case Background_Repeat_enum.Repeat_XY:
							s.Add("background-repeat:repeat-xy");
							break;
					}
				}

				if (vBackgroundPosition != Background_Position_enum._Unspecified_)
				{
					GotSomething = true;
					switch (vBackgroundPosition)
					{
						case Background_Position_enum._x_y_percent_specified_:
							s.Add("background-position: " + vBackgroundPositionX.ToString() + "% " + vBackgroundPositionY.ToString() + "%");
							break;
						case Background_Position_enum._xpos_ypos_specified_:
							s.Add("background-position: " + vBackgroundPositionX.ToString() + "px " + vBackgroundPositionY.ToString() + "px");
							break;
						default:
							s.Add("background-position:" + vBackgroundPosition.ToString().Replace("_", " "));
							break;
					}
				}

				if (!Util.StringIsNullOrEmpty(ref vFontFamily))
				{
					GotSomething = true;
					s.Add("font-family:" + vFontFamily);
				}

				if (vFontSize != Font_Size_enum._Unspecified_)
				{
					if (vFontSize == Font_Size_enum.aaa_value_specified_aaa)
					{
						if (!Util.StringIsNullOrEmpty(ref vFontSizeLength))
						{
							GotSomething = true;
							s.Add("font-size:" + vFontSizeLength);
						}
					}
					else if (vFontSize == Font_Size_enum.aaa_percent_specified_aaa)
					{
						if (vFontSizePercent > 0 & vFontSizePercent < 101)
						{
							GotSomething = true;
							s.Add("font-size:" + vFontSizePercent.ToString());
						}
					}
					else
					{
						GotSomething = true;
						s.Add("font-size:" + vFontSize.ToString().Replace("_", "-"));
					}
				}

				if (vFontStyle != Font_Style_enum._Unspecified_)
				{
					GotSomething = true;
					s.Add("font-style:" + vFontStyle.ToString());
				}

				if (vFontVariant != Font_Variant_enum._Unspecified_)
				{
					GotSomething = true;
					s.Add("font-variant:" + vFontVariant.ToString().Replace("_", "-"));
				}

				if (vFontWeight != Font_Weight_enum._Unspecified_)
				{
					if (vFontWeight == Font_Weight_enum.aaa_100_900_Specified_aaa)
					{
						if (vFontWeight100900 > 99 & vFontWeight100900 < 901)
						{
							GotSomething = true;
							s.Add("font-weight:" + vFontWeight100900.ToString());
						}
					}
					else
					{
						GotSomething = true;
						s.Add("font-weight:" + vFontWeight.ToString());
					}
				}

				if (vTextColor != ColorName._Unspecified_)
				{
					if (vTextColor == ColorName._HexCode_Provided_)
					{
						if (!Util.StringIsNullOrEmpty(ref vTextColorHex))
						{
							GotSomething = true;
							s.Add("color:" + vTextColorHex);
						}
					}
					else
					{
						GotSomething = true;
						s.Add("color:" + vTextColor.ToString());
					}
				}

				if (vTextAlign != Text_Align_enum._Unspecified_)
				{
					GotSomething = true;
					s.Add("text-align:" + vTextAlign.ToString());
				}

				if (vTextDecoration != Text_Decoration_enum._Unspecified_)
				{
					GotSomething = true;
					s.Add("text-decoration:" + vTextDecoration.ToString().Replace("_", "-"));
				}

				//If vTextIndent <> Text_Indent_enum._Unspecified_ Then

				//End If

				if (vTextWhiteSpace != White_Space_enum._Unspecified_)
				{
					GotSomething = true;
					s.Add("text-decoration:" + vTextDecoration.ToString().Replace("_", "-"));
				}

				if (vTextWordSpacing != Word_Spacing_enum._Unspecified_)
				{

					if (vTextWordSpacing == Word_Spacing_enum.aaa_length_specified_aaa)
					{
					}
					else
					{
						GotSomething = true;
						s.Add("word-spacing:" + vTextWordSpacing.ToString());
					}
				}


				if (GotSomething)
				{
					return "Style='" + String.Join(";", s.ToArray()) + "'";
				}
				else
				{
					return "";
				}

			}

			private ColorName vBackgroundColor = ColorName._Unspecified_;
			private string vBackgroundColorHex = "";
			public ColorName Background_Color
			{
				set { vBackgroundColor = value; }
			}
			public string Background_Color_Hex
			{
				set
				{
					if (Util.StringIsNullOrEmpty(ref value))
					{
						vBackgroundColorHex = "";
					}
					else
					{
						if (!value.Trim().StartsWith("#"))
						{
							vBackgroundColorHex = "";
							return;
						}
						if (!(value.Trim().Length == 7))
						{
							vBackgroundColorHex = "";
							return;
						}
						vBackgroundColorHex = value.Trim();
					}
				}
			}

			private string vBackgroundImage = "";
			public string Background_Image
			{
				set
				{
					if (Util.StringIsNullOrEmpty(ref value))
					{
						vBackgroundImage = "";
					}
					else
					{
						vBackgroundImage = value.Trim();
					}
				}
			}

			private Background_Repeat_enum vBackgroundRepeat = Background_Repeat_enum._Unspecified_;
			public Background_Repeat_enum Background_Repeat
			{
				set { vBackgroundRepeat = value; }
			}

			private Background_Position_enum vBackgroundPosition = Background_Position_enum._Unspecified_;
			private int vBackgroundPositionX;
			private int vBackgroundPositionY;
			public Background_Position_enum Background_Position
			{
				set { vBackgroundPosition = value; }
			}
			public void Background_Position_Percent(int xPercent, int yPercent)
			{
				if (xPercent < 1)
					return;
				if (xPercent > 100)
					return;
				if (yPercent < 1)
					return;
				if (yPercent > 100)
					return;
				vBackgroundPositionX = xPercent;
				vBackgroundPositionY = yPercent;
			}
			public void Background_Position_Pixels(int xPosition, int yPosition)
			{
				if (xPosition < 0)
					return;
				if (yPosition < 0)
					return;
				vBackgroundPositionX = xPosition;
				vBackgroundPositionY = yPosition;
			}

			private string vFontFamily = "";
			public string Font_Family
			{
				set
				{
					if (Util.StringIsNullOrEmpty(ref value))
					{
						vFontFamily = "";
					}
					else
					{
						vFontFamily = value.Trim();
						if (vFontFamily.Contains(" "))
						{
							if (!vFontFamily.Contains("'") & !vFontFamily.Contains("\""))
							{
								vFontFamily = "\"" + vFontFamily + "\"";
							}
							else if (vFontFamily.Contains("'"))
							{
								vFontFamily = vFontFamily.Replace("'", "\"");
							}
						}
					}
				}
			}

			private Font_Size_enum vFontSize = Font_Size_enum._Unspecified_;
			private string vFontSizeLength = "12px";

			private short vFontSizePercent = 100;
			public Font_Size_enum Font_Size
			{
				set { vFontSize = value; }
			}
			public string Font_Size_Length
			{
				set
				{
					if (Util.StringIsNullOrEmpty(ref value))
					{
						vFontSizeLength = "";
					}
					else
					{
						vFontSizeLength = value.Trim();
					}
				}
			}
			public short Font_Size_Percent
			{
				set
				{
					if (value < 1)
						return;
					if (value > 100)
						return;
					vFontSizePercent = value;
				}
			}

			private Font_Style_enum vFontStyle = Font_Style_enum._Unspecified_;
			public Font_Style_enum Font_Style
			{
				set { vFontStyle = value; }
			}

			private Font_Variant_enum vFontVariant = Font_Variant_enum._Unspecified_;
			public Font_Variant_enum Font_Variant
			{
				set { vFontVariant = value; }
			}

			private Font_Weight_enum vFontWeight = Font_Weight_enum._Unspecified_;
			private short vFontWeight100900 = 400;
			public Font_Weight_enum Font_Weight
			{
				set { vFontWeight = value; }
			}
			public short Font_Weight_Specified
			{
				set
				{
					if (value < 100)
						return;
					if (value > 900)
						return;
					if (value % 100 != 0)
						return;
					vFontWeight100900 = value;
				}
			}

			private ColorName vTextColor = ColorName._Unspecified_;
			private string vTextColorHex = "";
			public ColorName Text_Color
			{
				set { vTextColor = value; }
			}
			public string Text_Color_Hex
			{
				set
				{
					if (Util.StringIsNullOrEmpty(ref value))
					{
						vTextColorHex = "";
					}
					else
					{
						if (!value.Trim().StartsWith("#"))
						{
							vTextColorHex = "";
							return;
						}
						if (!(value.Trim().Length == 7))
						{
							vTextColorHex = "";
							return;
						}
						vTextColorHex = value.Trim();
					}
				}
			}


			private Text_Align_enum vTextAlign = Text_Align_enum._Unspecified_;
			public Text_Align_enum Text_Align
			{
				set { vTextAlign = value; }
			}

			private Text_Decoration_enum vTextDecoration = Text_Decoration_enum._Unspecified_;
			public Text_Decoration_enum Text_Decoration
			{
				set { vTextDecoration = value; }
			}

			//Private vTextIndent As Text_Indent_enum = Text_Indent_enum._Unspecified_
			//Private vTextIndentLength As String = ""
			//Private vTextIndentPercent As Short = 100
			//Public WriteOnly Property Text_Indent As Text_Indent_enum
			//    Set(ByVal value As Text_Indent_enum)
			//        vTextIndent = value
			//    End Set
			//End Property
			//Public WriteOnly Property Text_Indent_Length As String
			//    Set(ByVal value As String)
			//        If Util.StringIsNullOrEmpty(ref value) Then
			//            vTextIndentLength = ""
			//        Else
			//            vTextIndentLength = value.Trim
			//        End If
			//    End Set
			//End Property
			//Public WriteOnly Property Text_Indent_Percent As Short
			//    Set(ByVal value As Short)
			//        If value < 1 Then Exit Property
			//        If value > 100 Then Exit Property
			//        vTextIndentPercent = value
			//    End Set
			//End Property

			private White_Space_enum vTextWhiteSpace = White_Space_enum._Unspecified_;
			public White_Space_enum Text_White_Space
			{
				set { vTextWhiteSpace = value; }
			}

			private Word_Spacing_enum vTextWordSpacing = Word_Spacing_enum._Unspecified_;
			public Word_Spacing_enum Text_Word_Spacing
			{
				set { vTextWordSpacing = value; }
			}

			public enum ColorName
			{
				_Unspecified_,
				_HexCode_Provided_,
				AliceBlue,
				AntiqueWhite,
				Aqua,
				Aquamarine,
				Azure,
				Beige,
				Bisque,
				Black,
				BlanchedAlmond,
				Blue,
				BlueViolet,
				Brown,
				BurlyWood,
				CadetBlue,
				Chartreuse,
				Chocolate,
				Coral,
				CornflowerBlue,
				Cornsilk,
				Crimson,
				Cyan,
				DarkBlue,
				DarkCyan,
				DarkGoldenRod,
				DarkGray,
				DarkGrey,
				DarkGreen,
				DarkKhaki,
				DarkMagenta,
				DarkOliveGreen,
				Darkorange,
				DarkOrchid,
				DarkRed,
				DarkSalmon,
				DarkSeaGreen,
				DarkSlateBlue,
				DarkSlateGray,
				DarkSlateGrey,
				DarkTurquoise,
				DarkViolet,
				DeepPink,
				DeepSkyBlue,
				DimGray,
				DimGrey,
				DodgerBlue,
				FireBrick,
				FloralWhite,
				ForestGreen,
				Fuchsia,
				Gainsboro,
				GhostWhite,
				Gold,
				GoldenRod,
				Gray,
				Grey,
				Green,
				GreenYellow,
				HoneyDew,
				HotPink,
				IndianRed,
				Indigo,
				Ivory,
				Khaki,
				Lavender,
				LavenderBlush,
				LawnGreen,
				LemonChiffon,
				LightBlue,
				LightCoral,
				LightCyan,
				LightGoldenRodYellow,
				LightGray,
				LightGrey,
				LightGreen,
				LightPink,
				LightSalmon,
				LightSeaGreen,
				LightSkyBlue,
				LightSlateGray,
				LightSlateGrey,
				LightSteelBlue,
				LightYellow,
				Lime,
				LimeGreen,
				Linen,
				Magenta,
				Maroon,
				MediumAquaMarine,
				MediumBlue,
				MediumOrchid,
				MediumPurple,
				MediumSeaGreen,
				MediumSlateBlue,
				MediumSpringGreen,
				MediumTurquoise,
				MediumVioletRed,
				MidnightBlue,
				MintCream,
				MistyRose,
				Moccasin,
				NavajoWhite,
				Navy,
				OldLace,
				Olive,
				OliveDrab,
				Orange,
				OrangeRed,
				Orchid,
				PaleGoldenRod,
				PaleGreen,
				PaleTurquoise,
				PaleVioletRed,
				PapayaWhip,
				PeachPuff,
				Peru,
				Pink,
				Plum,
				PowderBlue,
				Purple,
				Red,
				RosyBrown,
				RoyalBlue,
				SaddleBrown,
				Salmon,
				SandyBrown,
				SeaGreen,
				SeaShell,
				Sienna,
				Silver,
				SkyBlue,
				SlateBlue,
				SlateGray,
				SlateGrey,
				Snow,
				SpringGreen,
				SteelBlue,
				Tan,
				Teal,
				Thistle,
				Tomato,
				Turquoise,
				Violet,
				Wheat,
				White,
				WhiteSmoke,
				Yellow,
				YellowGreen
			}

			public enum Background_Repeat_enum
			{
				_Unspecified_,
				Repeat_X,
				Repeat_Y,
				Repeat_XY
			}

			public enum Background_Position_enum
			{
				_Unspecified_,
				_x_y_percent_specified_,
				_xpos_ypos_specified_,
				inherit,
				left,
				top,
				left_center,
				left_bottom,
				right_top,
				right_center,
				right_bottom,
				center_top,
				center_center,
				center_bottom
			}

			public enum Font_Size_enum
			{
				_Unspecified_,
				aaa_value_specified_aaa,
				xx_small,
				small,
				medium,
				large,
				x_large,
				xx_large,
				smaller,
				larger,
				aaa_percent_specified_aaa,
				inherit
			}

			public enum Font_Style_enum
			{
				_Unspecified_,
				normal,
				italic,
				oblique,
				inherit
			}

			public enum Font_Variant_enum
			{
				_Unspecified_,
				normal,
				small_caps,
				inherit
			}

			public enum Font_Weight_enum
			{
				_Unspecified_,
				normal,
				bold,
				bolder,
				lighter,
				aaa_100_900_Specified_aaa
			}

			public enum Text_Align_enum
			{
				_Unspecified_,
				left,
				right,
				center,
				justify,
				inherit
			}

			public enum Text_Decoration_enum
			{
				_Unspecified_,
				none,
				underline,
				overline,
				line_through,
				blink,
				inherit
			}

			public enum Text_Indent_enum
			{
				_Unspecified_,
				aaa_length_specified_aaa,
				aaa_percent_specified_aaa,
				inherit
			}

			public enum White_Space_enum
			{
				_Unspecified_,
				normal,
				nowrap,
				pre,
				pre_line,
				pre_wrap,
				inherit
			}

			public enum Word_Spacing_enum
			{
				_Unspecified_,
				normal,
				aaa_length_specified_aaa,
				inherit
			}

		}


		public static string HTML_WrapSpan(string id, string wraptext, bool bdisplay = false, bool WithTable = false)
		{
			StringBuilder st = new StringBuilder();
			// ERROR: Not supported in C#: OnErrorStatement


			if (WithTable)
			{
				st.Append("<td>\r\n");
			}
			if (bdisplay)
			{
				st.Append("<span  id=\"" + id + "\">\r\n");
			}
			else
			{
				st.Append("<span style=\"display:none;\" id=\"" + id + "\">\r\n");
			}
			st.Append(wraptext);
			st.Append("</span>\r\n");
			if (WithTable)
			{
				st.Append("</td>");
			}

			return st.ToString();

		}

		public static string FormConfButton(ref string Name, ref string id, ref string Source, ref string alt_title, bool sel = false, short bHeight = 33, short bWidth = 100)
		{
			StringBuilder bup = new StringBuilder();

			bup.Append("<input type=\"image\" name=\"");
			bup.Append(Name);
			bup.Append("\" border=\"0\" id=\"");
			bup.Append(id);
			bup.Append("\" src=\"");
			if (sel)
			{
				bup.Append(Source + "s.gif");
			}
			else
			{
				bup.Append(Source + "n.gif");
			}
			bup.Append("\" height=\"" + bHeight.ToString() + "\" width=\"" + bWidth.ToString() + "\" alt=\"");
			bup.Append(alt_title);
			bup.Append("\" fp-style=\"fp-btn: Glass Tab 1; fp-font-color-hover: #0000FF; ");
			bup.Append("fp-font-color-press: #FF0000; fp-transparent: 1\" fp-title=\"");
			bup.Append(alt_title);
			bup.Append("\r\n");
			bup.Append("onmouseover = \"swapImg(1,0,/*id*/'" + id + "',/*url*/'" + Source + "h.gif')\"\r\n");
			if (sel)
			{
				bup.Append("onmouseout = \"swapImg(0,0,/*id*/'" + id + "',/*url*/'" + Source + "s.gif')\"\r\n");
			}
			else
			{
				bup.Append("onmouseout = \"swapImg(0,0,/*id*/'" + id + "',/*url*/'" + Source + "n.gif')\"\r\n");
			}
			if (sel)
			{
				bup.Append("onmousedown = \"swapImg(1,0,/*id*/'" + id + "',/*url*/'" + Source + "n.gif')\"\r\n");
			}
			else
			{
				bup.Append("onmousedown = \"swapImg(1,0,/*id*/'" + id + "',/*url*/'" + Source + "s.gif')\"\r\n");
			}
			bup.Append("onmouseup=\"swapImg(0,0,/*id*/'" + id + "',/*url*/'" + Source + "h.gif')\">\r\n");

			return bup.ToString();

		}

		#endregion




	}

}
