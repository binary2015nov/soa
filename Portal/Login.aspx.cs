using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Collections.Generic;
using JN.Esb.Portal.ServiceMgt.����Ŀ¼����;

public partial class LoginPage : System.Web.UI.Page{

    private HttpCookie authCookie;

	protected void Page_Load(object sender, EventArgs e) {

	}

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        string adPath = "LDAP://DC=jn,DC=com"; //Path to your LDAP directory server
        LdapAuthentication adAuth = new LdapAuthentication(adPath);
        try
        {
            //if (true == adAuth.IsAuthenticated(txtDomain.Text, txtUsername.Text, txtPassword.Text))
            {
                // string groups = adAuth.GetGroups();
                string groups = "";

                //Create the ticket, and add the groups.
                bool isCookiePersistent = chkPersist.Checked;
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1,
                          txtUsername.Text, DateTime.Now, DateTime.Now.AddMinutes(60), isCookiePersistent, groups);

                //Encrypt the ticket.
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                //Create a cookie, and then add the encrypted ticket to the cookie as data.
                authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                if (true == isCookiePersistent)
                {
                    authCookie.Expires = authTicket.Expiration;
                }

                //Add the cookie to the outgoing cookies collection.
                Response.Cookies.Add(authCookie);


                //Esb��ȨУ��
                EsbAuthen(txtUsername.Text);

                //Server.Transfer("Default.aspx");
                //You can redirect now.
                //Response.Redirect(FormsAuthentication.GetRedirectUrl(txtUsername.Text, false));
                Response.Redirect("Default.aspx", false);
            }
            /*else
            {
                errorLabel.Text = "��¼ʧ�ܣ������û��������룡";
            }*/
        }
        catch (System.Exception ex)
        {
            errorLabel.Text = "��¼ʧ�ܣ������û��������룡";
        }
    }

    /// <summary>
    /// Esb��ȨУ��
    /// </summary>
    /// <param name="userName"></param>
    private void EsbAuthen(string userName)
    {
        AuthenUser authenUser = AuthenUser.GetAuthenUserByLoginName(userName);

        if (null == authenUser){
            string strValue = "����δ��Ȩ���û���";
            string strAll = "<SCRIPT lanquage='JScript'>window.alert('" + strValue + "');window.location.href='Logout.aspx'<" + "/SCRIPT>";
            Response.Write(strAll);
            Response.End();
        }
    }
}
