using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Reflection;
using DevExpress.Web.ASPxHeadline;
using DevExpress.Web.ASPxSiteMapControl;
using DevExpress.Web.ASPxEditors;

public partial class BasePage : System.Web.UI.Page 
{
	protected enum DemoPageStatus { Default, New, Updated };

    public const string DefaultThemeName = "BlackGlass";

	const int InvalidHighlightIndex = Int32.MinValue;

    private Dictionary<string, DemoPageStatus> DemoPageGroupsStatus = null;
    private Dictionary<string, DemoPageStatus> DemoPageItemsStatus = null;
	private Dictionary<string, int> DemoPageHighlightedIndex = null;
    //private Dictionary<string, List<string>> DemoPageSourceCodeFiles = null;
    //private Dictionary<string, Unit> DemoPageCustomSourceCodeWidth = null;
    private string cssLink = "";
    private string demoName = "";
    private AuthenUser authenUser = new AuthenUser();

    protected string CSSLink {
        set { cssLink = value; }
    }
    protected string DemoName {
        get { return demoName; }
    }

	public UnboundSiteMapProvider SiteMapProvider {
		get {
			//if(!IsSiteMapCreated)
				Session["ESB_DemoUnboundProvider"] = CreateSiteMapProvider();
                return (UnboundSiteMapProvider)Session["ESB_DemoUnboundProvider"];
		}
	}
	public bool IsSiteMapCreated { get { return Application["DemoUnboundProvider"] != null; } }

    protected string GetThemeCookieName() {
        string cookieName = "EsbCurrentTheme";
        string path = Page.Request.ApplicationPath;

        int startPos = path.IndexOf("ASPx");
        if(startPos != -1) {
            int endPos = path.IndexOf("/", startPos);
            if(endPos != -1)
                cookieName = path.Substring(startPos, endPos - startPos);
        }
        return cookieName;
    }

    public AuthenUser AuthUser
    {
        get { return authenUser; }
        set { authenUser = value; }
    }


    /* Page PreInit */
    protected void Page_PreInit(object sender, EventArgs e) {
        string themeName = DefaultThemeName;
        if (Page.Request.Cookies[GetThemeCookieName()] != null) {
            themeName = Page.Request.Cookies[GetThemeCookieName()].Value;
        }

        string clientScriptBlock = "var DXCurrentThemeCookieName = \"" + GetThemeCookieName() + "\";";
        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "DXCurrentThemeCookieName", clientScriptBlock, true);
        this.Theme = themeName;
        
    }


	/* Page Init */
	protected override void OnInit(EventArgs e) {
        base.OnInit(e);
        //Tony ��ʼ����Ȩ
        InitAuthenUser();
		ClearDemoProperties();
        InitDemoProperties();  

	}
    /* Page Load */
    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);        

        // Scripts
        RegisterScript("Utilities", "~/Scripts/Utilities.js");
        // CSS
        RegisterCSSLink("~/CSS/styles.css");
        if (!string.IsNullOrEmpty(this.cssLink))
            RegisterCSSLink(this.cssLink);
    }

    /// <summary>
    /// ��ʼ����Ȩ�û�
    /// </summary>
    void InitAuthenUser()
    {
        HttpCookie authCookies = Request.Cookies[FormsAuthentication.FormsCookieName];

        if (authCookies == null || String.IsNullOrEmpty(authCookies.Value))
        {
            string strValue = "������Ȩ�Ѿ�ʧЧ�������µ�½��";
            string strAll = "<SCRIPT lanquage='JScript'>window.alert('" + strValue + "');window.location.href='/Portal/Logout.aspx'<" + "/SCRIPT>";

            Response.Write(strAll);
            Response.End();
        }
        else
        {
            FormsAuthenticationTicket authenTicket = FormsAuthentication.Decrypt(authCookies.Value);
            authenUser = AuthenUser.GetAuthenUserByLoginName(authenTicket.Name);
        }
    }

    /// <summary>
    /// ����Դ��쿴��
    /// </summary>
    protected void HideSourceCodeTable()
    {
        //Form.FindControl("tblSourceCode").Visible = false;
    }

    /// <summary>
    /// ���ߣ�Tony 2011-02-23
    /// ���ܣ�����ASPxComboBox��None��ʾֵ��һ����ASPxComboBox��DataBound�¼��н��е���
    /// </summary>
    /// <param name="cb"></param>
    /// <param name="noneDisplayText"></param>
    protected void SetASPxComboBoxNullItem(ASPxComboBox cb, string noneDisplayText)
    {
        if (string.IsNullOrEmpty(noneDisplayText))
            noneDisplayText = "(none)";

        if (cb != null)
        {
            ListEditItem item = new ListEditItem("noneDisplayText", "");
            cb.Items.Insert(0, item);
            cb.SelectedItem = item;
        }
    }

    /* Functions */
	public void PrepareStatusHeadlineGroups(ASPxHeadline sender) {
		PrepareStatusHeadlineCore(sender, this.DemoPageGroupsStatus);
    }

	public void PrepareStatusHeadlineItems(ASPxHeadline sender) {
        PrepareStatusHeadlineCore(sender, this.DemoPageItemsStatus);
    }

    protected virtual bool IsCurrentPage(object oUrl) {
        if (oUrl == null)
            return false;

        bool result = false;
        string url = oUrl.ToString();
        if (url.ToLower() == Page.Request.AppRelativeCurrentExecutionFilePath.ToLower())
            result = true;
        return result;
    }


    protected virtual bool GetStatus(object dataItem, string name) {
        IHierarchyData hierarchyData = (dataItem as IHierarchyData);
        XmlElement xmlElement = hierarchyData.Item as XmlElement;
        return GetStatusCore(xmlElement, name);
    }

    /* Private Functions */
	private void PrepareStatusHeadlineCore(ASPxHeadline hl, Dictionary<string, DemoPageStatus> colStatus) {
		if(hl != null && colStatus != null && colStatus.ContainsKey(GetStatusKey(hl.ContentText, hl.NavigateUrl))) {
			switch(colStatus[GetStatusKey(hl.ContentText, hl.NavigateUrl)]) {
                case DemoPageStatus.New:
                    hl.TailImage.Url = "~/Images/New.png";
                    hl.TailImage.Width = Unit.Pixel(20);
                    hl.TailImage.Height = Unit.Pixel(11);
                    break;
                case DemoPageStatus.Updated:
                    hl.TailImage.Url = "~/Images/Updated.png";
                    hl.TailImage.Width = Unit.Pixel(34);
                    hl.TailImage.Height = Unit.Pixel(11);
                    break;
            }
        }
    }

	private string GetStatusKey(string text, string url) {
		return text + "-" + url;
	}

	protected DemoPageStatus GetItemStatus(DevExpress.Web.ASPxNavBar.NavBarItem item) {
		string key = GetStatusKey(item.Text, item.NavigateUrl);
		if(DemoPageItemsStatus.ContainsKey(key)) return DemoPageItemsStatus[key];
		return DemoPageStatus.Default;
	}

	public bool IsHighlighted(DevExpress.Web.ASPxNavBar.NavBarItem item) {
		string key = GetStatusKey(item.Text, item.NavigateUrl);
		return DemoPageHighlightedIndex.ContainsKey(key);
	}
	public int GetHighlightedIndex(DevExpress.Web.ASPxNavBar.NavBarItem item) {
		string key = GetStatusKey(item.Text, item.NavigateUrl);
		return DemoPageHighlightedIndex[key];
	}

    private void ClearDemoProperties() {
        this.DemoPageGroupsStatus = null;
        this.DemoPageItemsStatus = null;
        this.DemoPageHighlightedIndex = null;
    }

    private void InitDemoProperties() {
        this.DemoPageGroupsStatus = new Dictionary<string, DemoPageStatus>();
        this.DemoPageItemsStatus = new Dictionary<string, DemoPageStatus>();
        this.DemoPageHighlightedIndex = new Dictionary<string, int>();

        XmlDocument menuXmlDocument = (Session["ESB_MENU"] as XmlDocument);

        if (string.IsNullOrEmpty(DemoName)) {
            this.demoName = "ASPxperience";
            if (menuXmlDocument.DocumentElement.Attributes["Name"] != null)
                this.demoName = menuXmlDocument.DocumentElement.Attributes["Name"].Value;
        }

        foreach (XmlNode node in menuXmlDocument.SelectNodes("//DemoGroup"))
        {
            AddPageStatus(this.DemoPageGroupsStatus, node);
            foreach (XmlNode nodeItem in node.SelectNodes("Demo")) {
                AddPageStatus(this.DemoPageItemsStatus, nodeItem);
                AddPageHighlightedIndex(this.DemoPageHighlightedIndex, nodeItem);
            }
        }

    }

    private void AddPageStatus(Dictionary<string, DemoPageStatus> ret, XmlNode node) {
        string url = GetAttributeValue(node.Attributes, "NavigateUrl");
        string text = GetAttributeValue(node.Attributes, "Text");
        DemoPageStatus status = DemoPageStatus.Default;
        if (GetStatusCore(node, "IsNew"))
            status = DemoPageStatus.New;
        else if (GetStatusCore(node, "IsUpdated"))
            status = DemoPageStatus.Updated;

        ret.Add(GetStatusKey(text, url), status);
    }

	private void AddPageHighlightedIndex(Dictionary<string, int> ret, XmlNode node) {
		int index = GetHighlightedIndexCore(node, "HighlightedIndex");
		if(index == InvalidHighlightIndex) return;

		string url = GetAttributeValue(node.Attributes, "NavigateUrl");
		string text = GetAttributeValue(node.Attributes, "Text");
		ret.Add(GetStatusKey(text, url), index);
	}

    private bool GetStatusCore(XmlElement element, string name) {
        bool ret = false;

        string value = GetAttributeValue(element.Attributes, name);
        bool.TryParse(value, out ret);
        return ret;
    }

    private bool GetStatusCore(XmlNode node, string name) {
        bool ret = false;
        string value = GetAttributeValue(node.Attributes, name);
        bool.TryParse(value, out ret);
        return ret;
    }

	private int GetHighlightedIndexCore(XmlNode node, string name) {
		int ret = InvalidHighlightIndex;
		string value = GetAttributeValue(node.Attributes, name);
		if(!int.TryParse(value, out ret)) return InvalidHighlightIndex;
		return ret;
	}

    private string GetAttributeValue(XmlAttributeCollection attributes, string name) {
        if (attributes[name] != null)
            return attributes[name].Value;
        else
            return "";
    }

    private void RegisterScript(string key, string url) {
        Page.ClientScript.RegisterClientScriptInclude(key, Page.ResolveUrl(url));
    }

    private void RegisterCSSLink(string url) {
        HtmlLink link = new HtmlLink();
        Page.Header.Controls.Add(link);
        link.EnableViewState = false;
        link.Attributes.Add("type", "text/css");
        link.Attributes.Add("rel", "stylesheet");
        link.Href = url;
    }

	protected UnboundSiteMapProvider CreateSiteMapProvider() {
		UnboundSiteMapProvider provider = new UnboundSiteMapProvider("", "");

		SiteMapNode categoryDemoNode = provider.RootNode;
        XmlDocument xmlDoc = Session["ESB_MENU"] as XmlDocument;

        foreach (XmlNode groupNode in xmlDoc.SelectNodes("//DemoGroup[not(@Visible=\"false\")]"))
        {
			bool groupIsNew = false;
			if(groupNode.Attributes["IsNew"] != null) {
				string value = groupNode.Attributes["IsNew"].Value;
				bool.TryParse(value, out groupIsNew);
			}
			bool groupIsUpdated = false;
			if(groupNode.Attributes["IsUpdated"] != null) {
				string value = groupNode.Attributes["IsUpdated"].Value;
				bool.TryParse(value, out groupIsUpdated);
			}

			System.Collections.Specialized.NameValueCollection attributes = new System.Collections.Specialized.NameValueCollection();
			attributes.Add("IsNew", groupIsNew.ToString());
			attributes.Add("IsUpdated", groupIsUpdated.ToString());
			SiteMapNode groupDemoNode = provider.CreateNode("", groupNode.Attributes["Text"].Value, "", null, attributes);

			bool beginCategory = false;
			if(groupNode.Attributes["BeginCategory"] != null &&
					bool.TryParse(groupNode.Attributes["BeginCategory"].Value, out beginCategory) &&
					beginCategory) {
				categoryDemoNode = provider.CreateNode("", "");
				provider.AddSiteMapNode(categoryDemoNode, provider.RootNode);
			}

			provider.AddSiteMapNode(groupDemoNode, categoryDemoNode);

			foreach(XmlNode itemNode in groupNode.SelectNodes("Demo")) {
				bool itemIsNew = false;
				if(itemNode.Attributes["IsNew"] != null) {
					string value = itemNode.Attributes["IsNew"].Value;
					bool.TryParse(value, out itemIsNew);
				}
				bool itemIsUpdated = false;
				if(itemNode.Attributes["IsUpdated"] != null) {
					string value = itemNode.Attributes["IsUpdated"].Value;
					bool.TryParse(value, out itemIsUpdated);
				}
				attributes = new System.Collections.Specialized.NameValueCollection();
				attributes.Add("IsNew", itemIsNew.ToString());
				attributes.Add("IsUpdated", itemIsUpdated.ToString());
				SiteMapNode itemDemoNode = provider.CreateNode(itemNode.Attributes["NavigateUrl"].Value, itemNode.Attributes["Text"].Value, "", null, attributes);
				provider.AddSiteMapNode(itemDemoNode, groupDemoNode);
			}
		}
		return provider;
	}
	public virtual void EnsureSiteMapIsBound() { }
}

public partial class AppearancePage : BasePage {
    protected virtual void mSelector_ItemDataBound(object source, DevExpress.Web.ASPxMenu.MenuItemEventArgs e) {
        if (GetStatus(e.Item.DataItem, "IsUpdated"))
            e.Item.Text += " <span style=\"color: #2D9404;\">(updated)</span>";
        if (GetStatus(e.Item.DataItem, "IsNew"))
            e.Item.Text += " <span style=\"color: #BD0808;\">(new)</span>";
    }
    protected virtual string GetHeaderTitle(string title, string text) {
        string name = title + " - ";
        if (text.IndexOf("<") > 0)
            name += text.Substring(0, text.IndexOf("<"));
        else
            name += text;
        return name;
    }
    protected virtual string GetCurrentAppearanceName() {
        return "Appearances/" + GetCurrentAppearanceNameCore() + ".ascx";
    }
    protected virtual string GetCurrentAppearanceNameCore() {
        string name = Page.Request.QueryString["Name"];
        if (String.IsNullOrEmpty(name))
            name = "";
        return name;
    }
}
