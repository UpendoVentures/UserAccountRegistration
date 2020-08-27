// ***********************************************************************************
// Connect UsersLibrary
// 
// Copyright (C) 2013-2014 DNN-Connect Association, Philipp Becker
// http://dnn-connect.org
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
// 
// ***********************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Connect.Libraries.UserManagement;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using Telerik.Web.UI;

namespace Connect.Modules.UserManagement.AccountRegistration
{
    public partial class View : ConnectUsersModuleBase, IActionable
    {
        public View()
        {

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            this.Init += Page_Init;
            this.PreRender += Page_PreRender;
        }

        protected void Page_Init(object sender, EventArgs e)
        {

            // DotNetNuke.Framework.AJAX.RegisterScriptManager()

            var argplhControls = plhRegister;
            ProcessFormTemplate(ref argplhControls, GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_Form, CurrentLocale, false), null);
            plhRegister = argplhControls;
            Button btnUpdate = (Button)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_UpdateButton);
            if (btnUpdate is object)
            {
                btnUpdate.Click += btnUpdate_Click;
            }

            Button btnLogin = (Button)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_LoginButton);
            if (btnLogin is object)
            {
                btnLogin.Click += btnLogin_Click;
            }

            Button btnLostPassword = (Button)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_LostPasswordButton);
            if (btnLostPassword is object)
            {
                btnLostPassword.Click += btnLostPassword_Click;
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            bool blnHasCaptchaControl = false;
            bool blnIsValid = true;
            string strResultCode = "";
            var checkControl = FindControlRecursive(plhRegister, plhRegister.ID + "_ReCaptchaPanel");
            if (checkControl is object)
            {
                blnHasCaptchaControl = true;
                blnIsValid = false;
            }

            if (blnHasCaptchaControl)
            {
                ReCaptcha.Validate(ReCaptchaKey, ref blnIsValid, ref strResultCode);
            }

            if (blnIsValid)
            {
                Register();
            }
            else
            {
                pnlError.Visible = true;
                pnlSuccess.Visible = false;
                switch (strResultCode.ToLower() ?? "")
                {
                    case "invalid-site-private-key":
                        {
                            // reCaptcha set up not correct, register anyway.
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(new Exception(Localization.GetString(strResultCode.ToLower() + ".Error", LocalResourceFile)));
                            Register();
                            break;
                        }

                    case "invalid-request-cookie":
                        {
                            lblError.Text = Localization.GetString(strResultCode.ToLower() + ".Error", LocalResourceFile);
                            break;
                        }

                    case "incorrect-captcha-sol":
                        {
                            lblError.Text = Localization.GetString(strResultCode.ToLower() + ".Error", LocalResourceFile);
                            break;
                        }

                    case "captcha-timeout":
                        {
                            lblError.Text = Localization.GetString(strResultCode.ToLower() + ".Error", LocalResourceFile);
                            break;
                        }

                    case "recaptcha-not-reachable":
                        {
                            // reCaptcha server not reachable. Register anyway.
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(new Exception(Localization.GetString(strResultCode.ToLower() + ".Error", LocalResourceFile)));
                            Register();
                            break;
                        }

                    default:
                        {
                            lblError.Text = Localization.GetString("recaptcha-common-error.Error", LocalResourceFile);
                            break;
                        }
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            HandleLogin();
        }

        protected void btnLostPassword_Click(object sender, EventArgs e)
        {
            HandleLostPassword();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            Control argContainer = plhRegister;
            ManageRegionLabel(ref argContainer);
            plhRegister = (PlaceHolder)argContainer;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private void HandleLostPassword()
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "ctl=SendPassword"));
        }

        private void HandleLogin()
        {
            TextBox txtUsername = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_UsernameForLogin);
            TextBox txtPassword = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_PasswordForLogin);
            CheckBox chkRemember = (CheckBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_RememberForLogin);
            if (txtUsername is object && txtPassword is object)
            {
                var loginStatus = default(UserLoginStatus);
                var objUser = UserController.ValidateUser(PortalId, txtUsername.Text, txtPassword.Text, "", PortalSettings.PortalName, Request.UserHostAddress, ref loginStatus);
                switch (loginStatus)
                {
                    case UserLoginStatus.LOGIN_FAILURE:
                        {
                            lblError.Text = Localization.GetString("LOGIN_FAILURE", LocalResourceFile);
                            pnlError.Visible = true;
                            break;
                        }

                    case UserLoginStatus.LOGIN_INSECUREADMINPASSWORD:
                    case UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD:
                    case UserLoginStatus.LOGIN_SUPERUSER:
                    case UserLoginStatus.LOGIN_SUCCESS:
                        {
                            bool blnPersistent = false;
                            if (chkRemember is object)
                            {
                                blnPersistent = chkRemember.Checked;
                            }

                            UserController.UserLogin(PortalId, objUser, PortalSettings.PortalName, Request.UserHostAddress, blnPersistent);
                            if (Request.QueryString["ReturnURL"] is object)
                            {
                                Response.Redirect(Server.UrlDecode(Request.QueryString["ReturnURL"]), true);
                            }

                            if (RedirectAfterLogin != Null.NullInteger)
                            {
                                Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(RedirectAfterLogin));
                            }
                            else
                            {
                                Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId));
                            }

                            break;
                        }

                    case UserLoginStatus.LOGIN_USERLOCKEDOUT:
                        {
                            lblError.Text = Localization.GetString("LOGIN_USERLOCKEDOUT", LocalResourceFile);
                            pnlError.Visible = true;
                            break;
                        }

                    case UserLoginStatus.LOGIN_USERNOTAPPROVED:
                        {
                            lblError.Text = Localization.GetString("LOGIN_USERNOTAPPROVED", LocalResourceFile);
                            pnlError.Visible = true;
                            break;
                        }
                }
            }
        }

        private void Register()
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = false;
            var strMessages = new List<string>();
            bool blnUpdateUsername = false;
            bool blnUpdateFirstname = false;
            bool blnUpdateLastname = false;
            bool blnUpdateDisplayname = false;
            bool blnUpdatePassword = false;
            bool blnUpdateEmail = false;
            TextBox txtUsername = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Username);
            blnUpdateUsername = txtUsername is object;
            if (blnUpdateUsername)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Username, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingUsername", LocalResourceFile));
                    Control argobjControl = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Username, ref argobjControl);
                    plhRegister = (PlaceHolder)argobjControl;
                }
                else
                {
                    Control argobjControl1 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Username, ref argobjControl1, true);
                    plhRegister = (PlaceHolder)argobjControl1;
                }
            }

            TextBox txtEmail = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Email);
            blnUpdateEmail = txtEmail is object;
            if (blnUpdateEmail)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Email, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_InvalidEmail", LocalResourceFile));
                    Control argobjControl2 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Email, ref argobjControl2);
                    plhRegister = (PlaceHolder)argobjControl2;
                }
                else
                {
                    Control argobjControl3 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Email, ref argobjControl3, true);
                    plhRegister = (PlaceHolder)argobjControl3;
                }
            }

            TextBox txtPassword = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Password1);
            TextBox txtPassword2 = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Password2);
            blnUpdatePassword = txtPassword is object && txtPassword2 is object;
            if (blnUpdatePassword)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Password1, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingPassword1", LocalResourceFile));
                    Control argobjControl4 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Password1, ref argobjControl4);
                    plhRegister = (PlaceHolder)argobjControl4;
                }
                else
                {
                    Control argobjControl5 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Password1, ref argobjControl5, true);
                    plhRegister = (PlaceHolder)argobjControl5;
                }

                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Password2, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingPassword2", LocalResourceFile));
                    Control argobjControl6 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Password2, ref argobjControl6);
                    plhRegister = (PlaceHolder)argobjControl6;
                }
                else
                {
                    Control argobjControl7 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Password2, ref argobjControl7, true);
                    plhRegister = (PlaceHolder)argobjControl7;
                }
            }

            TextBox txtPasswordQuestion = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_PasswordQuestion);
            bool blnUpdatePasswordQuestion = txtPasswordQuestion is object;
            if (blnUpdatePasswordQuestion)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_PasswordQuestion, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingPasswordQuestion", LocalResourceFile));
                    Control argobjControl8 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_PasswordQuestion, ref argobjControl8);
                    plhRegister = (PlaceHolder)argobjControl8;
                }
                else
                {
                    Control argobjControl9 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_PasswordQuestion, ref argobjControl9, true);
                    plhRegister = (PlaceHolder)argobjControl9;
                }
            }

            TextBox txtPasswordAnswer = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_PasswordAnswer);
            bool blnUpdatePasswordAnswer = txtPasswordAnswer is object;
            if (blnUpdatePasswordAnswer)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_PasswordAnswer, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingPasswordAnswer", LocalResourceFile));
                    Control argobjControl10 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_PasswordAnswer, ref argobjControl10);
                    plhRegister = (PlaceHolder)argobjControl10;
                }
                else
                {
                    Control argobjControl11 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_PasswordAnswer, ref argobjControl11, true);
                    plhRegister = (PlaceHolder)argobjControl11;
                }
            }

            TextBox txtFirstName = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Firstname);
            blnUpdateFirstname = txtFirstName is object;
            if (blnUpdateFirstname)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Firstname, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingFirstname", LocalResourceFile));
                    Control argobjControl12 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Firstname, ref argobjControl12);
                    plhRegister = (PlaceHolder)argobjControl12;
                }
                else
                {
                    Control argobjControl13 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Firstname, ref argobjControl13, true);
                    plhRegister = (PlaceHolder)argobjControl13;
                }
            }

            TextBox txtLastName = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Lastname);
            blnUpdateLastname = txtLastName is object;
            if (blnUpdateLastname)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Lastname, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingFirstname", LocalResourceFile));
                    Control argobjControl14 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Lastname, ref argobjControl14);
                    plhRegister = (PlaceHolder)argobjControl14;
                }
                else
                {
                    Control argobjControl15 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Lastname, ref argobjControl15, true);
                    plhRegister = (PlaceHolder)argobjControl15;
                }
            }

            if (CompareFirstNameLastName && blnUpdateFirstname & blnUpdateLastname)
            {
                if ((txtLastName.Text.ToLower().Trim() ?? "") == (txtFirstName.Text.ToLower().Trim() ?? ""))
                {
                    strMessages.Add(Localization.GetString("Error_LastnameLikeFirstname", LocalResourceFile));
                    Control argobjControl16 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Firstname, ref argobjControl16);
                    plhRegister = (PlaceHolder)argobjControl16;
                }
            }

            TextBox txtDisplayName = (TextBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_Displayname);
            blnUpdateDisplayname = txtDisplayName is object;
            if (blnUpdateDisplayname)
            {
                if (!IsValidUserAttribute(Libraries.UserManagement.Constants.User_Displayname, plhRegister))
                {
                    strMessages.Add(Localization.GetString("Error_MissingDisplayName", LocalResourceFile));
                    Control argobjControl17 = plhRegister;
                    AddErrorIndicator(Libraries.UserManagement.Constants.User_Displayname, ref argobjControl17);
                    plhRegister = (PlaceHolder)argobjControl17;
                }
                else
                {
                    Control argobjControl18 = plhRegister;
                    RemoveErrorIndicator(Libraries.UserManagement.Constants.User_Displayname, ref argobjControl18, true);
                    plhRegister = (PlaceHolder)argobjControl18;
                }
            }

            bool blnProfileErrorAdded = false;
            foreach (string itemProp in GetPropertiesFromTempate(GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_Form, CurrentLocale, false)))
            {
                try
                {
                    var prop = ProfileController.GetPropertyDefinitionByName(PortalId, itemProp.Substring(2)); // itemprop comes in the form U:Propertyname or P:Propertyname
                    if (prop is object)
                    {
                        Control argobjControl21 = plhRegister;
                        if (!IsValidProperty(null, prop, ref argobjControl21))
                        {
                            if (blnProfileErrorAdded == false)
                            {
                                strMessages.Add(Localization.GetString("Error_MissingProfileField", LocalResourceFile));
                                blnProfileErrorAdded = true;
                            }

                            Control argobjControl19 = plhRegister;
                            AddErrorIndicator(prop.PropertyDefinitionId.ToString(), ref argobjControl19);
                            plhRegister = (PlaceHolder)argobjControl19;
                        }
                        else
                        {
                            Control argobjControl20 = plhRegister;
                            RemoveErrorIndicator(prop.PropertyDefinitionId.ToString(), ref argobjControl20, prop.Required);
                            plhRegister = (PlaceHolder)argobjControl20;
                        }

                        plhRegister = (PlaceHolder)argobjControl21;
                    }
                }
                catch
                {
                }
            }

            // Create the actual user object
            var oUser = new UserInfo();

            // approve membership if applicable
            if (PortalSettings.UserRegistration == (int)DotNetNuke.Common.Globals.PortalRegistrationType.PublicRegistration)
            {
                oUser.Membership.Approved = true;
            }
            else
            {
                oUser.Membership.Approved = false;
            }

            // set defaults
            oUser.AffiliateID = Null.NullInteger;
            oUser.PortalID = PortalSettings.PortalId;
            oUser.IsDeleted = false;
            oUser.IsSuperUser = false;
            oUser.LastIPAddress = Request.UserHostAddress;
            oUser.Profile = new UserProfile();
            oUser.Username = "";
            oUser.DisplayName = "";
            oUser.Email = "";
            oUser.Membership.Password = "";



            // set username depending on module setting
            switch (UsernameMode)
            {
                case UsernameUpdateMode.Email:
                    {
                        if (blnUpdateEmail)
                        {
                            oUser.Username = txtEmail.Text.Trim();
                        }

                        break;
                    }

                case UsernameUpdateMode.FirstLetterLastname:
                    {
                        if (blnUpdateLastname && blnUpdateFirstname)
                        {
                            oUser.Username = txtFirstName.Text.Trim().Substring(0, 1) + "." + txtLastName.Text;
                        }

                        break;
                    }

                case UsernameUpdateMode.FirstnameLastname:
                    {
                        if (blnUpdateLastname && blnUpdateFirstname)
                        {
                            oUser.Username = txtFirstName.Text + "." + txtLastName.Text;
                        }

                        break;
                    }

                case UsernameUpdateMode.Lastname:
                    {
                        if (blnUpdateLastname)
                        {
                            oUser.Username = txtLastName.Text;
                        }

                        break;
                    }

                case UsernameUpdateMode.UserSelect:
                    {
                        if (blnUpdateUsername)
                        {
                            oUser.Username = txtUsername.Text;
                        }

                        break;
                    }
            }

            // set displayname depending on module setting
            switch (DisplaynameMode)
            {
                case DisplaynameUpdateMode.Email:
                    {
                        if (blnUpdateEmail)
                        {
                            oUser.DisplayName = txtEmail.Text.Trim();
                        }

                        break;
                    }

                case DisplaynameUpdateMode.FirstLetterLastname:
                    {
                        if (blnUpdateLastname && blnUpdateFirstname)
                        {
                            oUser.DisplayName = txtFirstName.Text.Trim().Substring(0, 1) + ". " + txtLastName.Text;
                        }

                        break;
                    }

                case DisplaynameUpdateMode.FirstnameLastname:
                    {
                        if (blnUpdateLastname && blnUpdateFirstname)
                        {
                            oUser.DisplayName = txtFirstName.Text + " " + txtLastName.Text;
                        }
                        else
                        {
                        }

                        break;
                    }

                case DisplaynameUpdateMode.Lastname:
                    {
                        if (blnUpdateLastname)
                        {
                            oUser.DisplayName = txtLastName.Text;
                        }

                        break;
                    }

                case DisplaynameUpdateMode.UserSelect:
                    {
                        if (blnUpdateDisplayname)
                        {
                            oUser.DisplayName = txtDisplayName.Text;
                        }

                        break;
                    }
            }

            if (blnUpdateEmail)
            {
                oUser.Email = txtEmail.Text;
            }

            // try updating password
            if (blnUpdatePassword) // only true once both password fields are found in the template
            {
                if ((txtPassword.Text ?? "") == (txtPassword2.Text ?? ""))
                {
                    if (UserController.ValidatePassword(txtPassword.Text)) // let DNN validate password policy
                    {
                        oUser.Membership.Password = txtPassword.Text;
                    }
                    else // check failed, provide feedback about actual password policy
                    {
                        int MinLength = 0;
                        int MinNonAlphaNumeric = 0;
                        try
                        {
                            MinLength = DotNetNuke.Security.Membership.MembershipProvider.Instance().MinPasswordLength;
                        }
                        catch
                        {
                        }

                        try
                        {
                            MinNonAlphaNumeric = DotNetNuke.Security.Membership.MembershipProvider.Instance().MinNonAlphanumericCharacters;
                        }
                        catch
                        {
                        }

                        string strPolicy = string.Format(Localization.GetString("PasswordPolicy_MinLength", LocalResourceFile), MinLength.ToString());
                        if (MinNonAlphaNumeric > 0)
                        {
                            strPolicy += string.Format(Localization.GetString("PasswordPolicy_MinNonAlphaNumeric", LocalResourceFile), MinNonAlphaNumeric.ToString());
                        }

                        strMessages.Add(string.Format(Localization.GetString("InvalidPassword", LocalResourceFile), strPolicy));
                    }
                }
                else
                {
                    strMessages.Add(Localization.GetString("PasswordsDontMatch.Text", LocalResourceFile));
                }
            }
            else // no password fields in template, auto-generate password
            {
                oUser.Membership.Password = UserController.GeneratePassword(DotNetNuke.Security.Membership.MembershipProvider.Instance().MinPasswordLength);
            }

            if (blnUpdatePasswordQuestion && blnUpdatePasswordAnswer)
            {
                oUser.Membership.PasswordQuestion = txtPasswordQuestion.Text;
                oUser.Membership.PasswordAnswer = txtPasswordAnswer.Text;
            }

            if (string.IsNullOrEmpty(oUser.Username) | string.IsNullOrEmpty(oUser.Email) | string.IsNullOrEmpty(oUser.DisplayName) | string.IsNullOrEmpty(oUser.Membership.Password))
            {

                // template must be setup up wrong, some fields missing most likely
                strMessages.Add(string.Format(Localization.GetString("TemplateingError.Text", LocalResourceFile), PortalSettings.Email));
            }

            // set up profile object
            oUser.Profile = new UserProfile();
            oUser.Profile.InitialiseProfile(PortalSettings.PortalId, true);
            oUser.Profile.PreferredLocale = PortalSettings.DefaultLanguage;
            oUser.Profile.PreferredTimeZone = PortalSettings.TimeZone;

            // retrieve properties from template
            var propertiesCollection = new ProfilePropertyDefinitionCollection();
            Control argContainer = plhRegister;
            UpdateProfileProperties(ref argContainer, ref oUser, ref propertiesCollection, GetPropertiesFromTempate(GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_Form, CurrentLocale, false)));
            plhRegister = (PlaceHolder)argContainer;

            // -------------------------------------------------------------------------------------
            // Call the Validation interface as a last resort to stop registration
            // -------------------------------------------------------------------------------------
            bool externalValidationPass = true;
            if ((ExternalInterface ?? "") != (Null.NullString ?? ""))
            {
                object objInterface = null;
                if (ExternalInterface.Contains(","))
                {
                    string strAssembly = ExternalInterface.Split(char.Parse(","))[0].Trim();
                    string strClass = ExternalInterface.Split(char.Parse(","))[1].Trim();
                    objInterface = Activator.CreateInstance(strAssembly, strClass).Unwrap();
                }

                if (objInterface is object)
                {
                    bool localValidateRegistration()
                    {
                        var argServer = Server;
                        var argResponse = Response;
                        var argRequest = Request;
                        var ret =
                            ((Libraries.UserManagement.Interfaces.iAccountRegistration) objInterface)
                            .ValidateRegistration(ref argServer, ref argResponse, ref argRequest, oUser,
                                propertiesCollection, strMessages);
                        return ret;
                    }

                    externalValidationPass = localValidateRegistration();
                }
            }

            if (strMessages.Count > 0 || !externalValidationPass)
            {
                pnlError.Visible = true;
                lblError.Text = "<ul>";
                if (strMessages.Count == 0)
                {
                    lblError.Text += "<li>" + Localization.GetString("Error_Unknown", LocalResourceFile) + "</li>";
                }
                else
                {
                    foreach (string strMessage in strMessages)
                        lblError.Text += "<li>" + strMessage + "</li>";
                }

                lblError.Text += "</ul>";
                return;
            }

            // everything fine so far, let's create the account
            var createStatus = UserController.CreateUser(ref oUser);
            string strStatus = "";
            if (createStatus != UserCreateStatus.Success)
            {
                switch (createStatus)
                {
                    case UserCreateStatus.UsernameAlreadyExists:
                        {
                            switch (UsernameMode)
                            {
                                case UsernameUpdateMode.UserSelect:
                                    {
                                        strStatus = Localization.GetString("UsernameAlreadyExists", LocalResourceFile);
                                        break;
                                    }

                                case UsernameUpdateMode.Email:
                                    {
                                        strStatus = Localization.GetString("DuplicateEmail", LocalResourceFile);
                                        break;
                                    }

                                case UsernameUpdateMode.FirstnameLastname:
                                case UsernameUpdateMode.FirstLetterLastname:
                                case UsernameUpdateMode.Lastname:
                                    {
                                        strStatus = Localization.GetString("NameAlreadyExists", LocalResourceFile);
                                        break;
                                    }
                            }

                            break;
                        }

                    default:
                        {
                            strStatus = string.Format(Localization.GetString("CreateError", LocalResourceFile), createStatus.ToString());
                            break;
                        }
                }

                if (!string.IsNullOrEmpty(strStatus))
                {
                    strStatus = "<li>" + strStatus + "</li>";
                }
                else
                {
                    strStatus = "<li>" + createStatus.ToString() + "</li>";
                }

                pnlError.Visible = true;
                lblError.Text = "<ul>" + strStatus + "</ul>";
                return;
            }

            oUser = ProfileController.UpdateUserProfile(oUser, propertiesCollection);
            if (blnUpdateFirstname == true)
            {
                oUser.Profile.FirstName = txtFirstName.Text;
                oUser.FirstName = txtFirstName.Text;
            }
            else if (!string.IsNullOrEmpty(oUser.Profile.FirstName))
            {
                oUser.FirstName = oUser.Profile.FirstName;
            }

            if (blnUpdateLastname == true)
            {
                oUser.Profile.LastName = txtLastName.Text;
                oUser.LastName = txtLastName.Text;
            }
            else if (!string.IsNullOrEmpty(oUser.Profile.LastName))
            {
                oUser.LastName = oUser.Profile.LastName;
            }

            try
            {
                oUser.Profile.SetProfileProperty("Email", oUser.Email);
            }
            catch
            {
            }

            // update profile
            ProfileController.UpdateUserProfile(oUser);
            UserController.UpdateUser(PortalId, oUser);
            string strUserBody = "";
            string strAdminBody = "";
            if (NotifyUser)
            {
                if (PortalSettings.UserRegistration == (int)DotNetNuke.Common.Globals.PortalRegistrationType.PrivateRegistration)
                {
                    strUserBody = GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_EmailToUser_Private, CurrentLocale, false);
                }
                else if (PortalSettings.UserRegistration == (int)DotNetNuke.Common.Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    strUserBody = GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_EmailToUser_Verified, CurrentLocale, false);
                }
                else
                {
                    strUserBody = GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_EmailToUser, CurrentLocale, false);
                }
            }

            if (!string.IsNullOrEmpty(NotifyRole))
            {
                strAdminBody = GetTemplate(ModuleTheme, Libraries.UserManagement.Constants.TemplateName_EmailToAdmin, CurrentLocale, false);
            }

            if (!string.IsNullOrEmpty(strAdminBody))
            {
                ProcessAdminNotification(strAdminBody, oUser);
            }

            if (!string.IsNullOrEmpty(strUserBody))
            {
                ProcessUserNotification(strUserBody, oUser);
            }

            // add to role
            if (AddToRoleOnSubmit != Null.NullInteger)
            {
                try
                {
                    var rc = new RoleController();
                    if (AddToRoleStatus.ToLower() == "pending")
                    {
                        rc.AddUserRole(PortalId, oUser.UserID, AddToRoleOnSubmit, RoleStatus.Pending, false, DateTime.Now, Null.NullDate);
                    }
                    else
                    {
                        rc.AddUserRole(PortalId, oUser.UserID, AddToRoleOnSubmit, RoleStatus.Approved, false, DateTime.Now, Null.NullDate);
                    }
                }
                catch
                {
                }
            }

            bool blnAddMembership = false;
            CheckBox chkTest = (CheckBox)FindMembershipControlsRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_RoleMembership);
            if (chkTest is object)
            {
                // at least on role membership checkbox found. Now lookup roles that could match
                var rc = new RoleController();
                ArrayList roles;
                roles = rc.GetPortalRoles(PortalId);
                foreach (RoleInfo objRole in roles)
                {
                    bool blnPending = false;
                    CheckBox chkRole = (CheckBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_RoleMembership + objRole.RoleName.Replace(" ", ""));
                    if (chkRole is null)
                    {
                        chkRole = (CheckBox)FindControlRecursive(plhRegister, plhRegister.ID + "_" + Libraries.UserManagement.Constants.ControlId_RoleMembership + objRole.RoleName.Replace(" ", "") + "_Pending");
                        blnPending = true;
                    }

                    if (chkRole is object)
                    {
                        if (blnPending)
                        {
                            rc.AddUserRole(PortalId, oUser.UserID, objRole.RoleID, RoleStatus.Pending, false, DateTime.Now, Null.NullDate);
                        }
                        else
                        {
                            rc.AddUserRole(PortalId, oUser.UserID, objRole.RoleID, RoleStatus.Approved, false, DateTime.Now, Null.NullDate);
                        }
                    }
                }
            }

            if (PortalSettings.UserRegistration == (int)DotNetNuke.Common.Globals.PortalRegistrationType.PublicRegistration)
            {

                // logon user
                var logonStatus = UserLoginStatus.LOGIN_FAILURE;
                UserController.UserLogin(PortalId, oUser.Username, oUser.Membership.Password, "", PortalSettings.PortalName, Request.UserHostAddress, ref logonStatus, true);

                // see if all worked
                if (logonStatus != UserLoginStatus.LOGIN_SUCCESS)
                {
                    strStatus += logonStatus.ToString();
                    pnlError.Visible = true;
                    lblError.Text = strStatus;
                    return;
                }
            }

            string strResult = "";
            switch (PortalSettings.UserRegistration)
            {
                case (int)DotNetNuke.Common.Globals.PortalRegistrationType.PublicRegistration:
                    {
                        strResult = string.Format(Localization.GetString("RegisterSuccess_Public", LocalResourceFile), DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId));
                        break;
                    }

                case (int)DotNetNuke.Common.Globals.PortalRegistrationType.PrivateRegistration:
                    {
                        strResult = string.Format(Localization.GetString("RegisterSuccess_Private", LocalResourceFile), DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId));
                        break;
                    }

                case (int)DotNetNuke.Common.Globals.PortalRegistrationType.VerifiedRegistration:
                    {
                        strResult = string.Format(Localization.GetString("RegisterSuccess_Verified", LocalResourceFile), DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId));
                        break;
                    }
            }

            lblSucess.Text = "<ul><li>" + strResult + "</li></ul>";
            pnlSuccess.Visible = true;

            // run the final interface if applicable
            if ((ExternalInterface ?? "") != (Null.NullString ?? ""))
            {
                object objInterface = null;
                if (ExternalInterface.Contains(","))
                {
                    string strClass = ExternalInterface.Split(char.Parse(","))[0].Trim();
                    string strAssembly = ExternalInterface.Split(char.Parse(","))[1].Trim();
                    objInterface = Activator.CreateInstance(strAssembly, strClass).Unwrap();
                }

                if (objInterface is object)
                {
                    var argServer = Server;
                    var argResponse = Response;
                    var argRequest = Request;
                    ((Libraries.UserManagement.Interfaces.iAccountRegistration)objInterface).FinalizeAccountRegistration(ref argServer, ref argResponse, ref argRequest, oUser);
                }
            }

            // the following might not be processed if the interfaces manipulate the current response!
            if (Request.QueryString["ReturnURL"] is object)
            {
                Response.Redirect(Server.UrlDecode(Request.QueryString["ReturnURL"]), true);
            }

            if (RedirectAfterSubmit != Null.NullInteger)
            {
                Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(RedirectAfterSubmit));
            }
        }

        private void ProcessAdminNotification(string Body, UserInfo CurrentUser)
        {
            Body = Body.Replace("[PORTALURL]", PortalSettings.PortalAlias.HTTPAlias);
            Body = Body.Replace("[PORTALNAME]", PortalSettings.PortalName);
            Body = Body.Replace("[USERID]", CurrentUser.UserID.ToString());
            Body = Body.Replace("[DISPLAYNAME]", CurrentUser.DisplayName);
            if (DotNetNuke.Security.Membership.MembershipProvider.Instance().PasswordRetrievalEnabled)
            {
                Body = Body.Replace("[PASSWORD]", DotNetNuke.Security.Membership.MembershipProvider.Instance().GetPassword(User, ""));
            }

            Body = Body.Replace("[USERNAME]", CurrentUser.Username);
            Body = Body.Replace("[FIRSTNAME]", CurrentUser.FirstName);
            Body = Body.Replace("[LASTNAME]", CurrentUser.LastName);
            Body = Body.Replace("[EMAIL]", CurrentUser.Email);
            if (PortalSettings.UserRegistration == (int)DotNetNuke.Common.Globals.PortalRegistrationType.PrivateRegistration)
            {
                Body = Body.Replace("[ADMINACTION]", Localization.GetString("AuthorizeAccount.Action", LocalResourceFile));
                Body = Body.Replace("[REGISTRATIONMODE]", Localization.GetString("RegistrationMode_Private.Text", LocalResourceFile));
            }
            else if (PortalSettings.UserRegistration == (int)DotNetNuke.Common.Globals.PortalRegistrationType.VerifiedRegistration)
            {
                Body = Body.Replace("[ADMINACTION]", Localization.GetString("VerifyAccount.Action", LocalResourceFile));
                Body = Body.Replace("[REGISTRATIONMODE]", Localization.GetString("RegistrationMode_Verified.Text", LocalResourceFile));
            }
            else
            {
                Body = Body.Replace("[ADMINACTION]", Localization.GetString("NoAction.Action", LocalResourceFile));
                Body = Body.Replace("[REGISTRATIONMODE]", Localization.GetString("RegistrationMode_Public.Text", LocalResourceFile));
            }

            Body = Body.Replace("[USERURL]", DotNetNuke.Common.Globals.NavigateURL(UsermanagementTab, "", "uid=" + CurrentUser.UserID.ToString(), "RoleId=" + PortalSettings.RegisteredRoleId.ToString()));
            var ctrlRoles = new RoleController();
            var NotificationUsers = ctrlRoles.GetUsersByRoleName(PortalId, NotifyRole);
            foreach (UserInfo NotificationUser in NotificationUsers)
            {
                try
                {
                    Body = Body.Replace("[RECIPIENTUSERID]", NotificationUser.UserID.ToString());
                    Body = Body.Replace("[USERID]", NotificationUser.UserID.ToString());
                    DotNetNuke.Services.Mail.Mail.SendMail(PortalSettings.Email, NotificationUser.Email, "", string.Format(Localization.GetString("NotifySubject_UserRegistered.Text", LocalResourceFile), PortalSettings.PortalName), Body, "", "HTML", "", "", "", "");
                }
                catch
                {
                }
            }
        }

        private void ProcessUserNotification(string Body, UserInfo CurrentUser)
        {
            Body = Body.Replace("[PORTALURL]", PortalSettings.PortalAlias.HTTPAlias);
            Body = Body.Replace("[PORTALNAME]", PortalSettings.PortalName);
            Body = Body.Replace("[USERID]", CurrentUser.UserID.ToString());
            Body = Body.Replace("[DISPLAYNAME]", CurrentUser.DisplayName);
            if (DotNetNuke.Security.Membership.MembershipProvider.Instance().PasswordRetrievalEnabled)
            {
                Body = Body.Replace("[PASSWORD]", DotNetNuke.Security.Membership.MembershipProvider.Instance().GetPassword(User, ""));
            }

            Body = Body.Replace("[USERNAME]", CurrentUser.Username);
            Body = Body.Replace("[FIRSTNAME]", CurrentUser.FirstName);
            Body = Body.Replace("[LASTNAME]", CurrentUser.LastName);
            Body = Body.Replace("[EMAIL]", CurrentUser.Email);

            // verification code is now expected to be encrypted. Bummer.
            // Body = Body.Replace("[VERIFICATIONCODE]", PortalSettings.PortalId.ToString & "-" & CurrentUser.UserID.ToString)            
            Body = Body.Replace("[VERIFICATIONCODE]", Utilities.GetVerificationCode(CurrentUser));
            Body = Body.Replace("[RECIPIENTUSERID]", CurrentUser.UserID.ToString());
            Body = Body.Replace("[USERID]", CurrentUser.UserID.ToString());
            if (PortalSettings.UserTabId != Null.NullInteger)
            {
                Body = Body.Replace("[USERURL]", DotNetNuke.Common.Globals.NavigateURL(PortalSettings.UserTabId));
            }
            else
            {
                Body = Body.Replace("[USERURL]", DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId, "ctl=Profile"));
            }

            string returnurl = "";
            string loginurl = "";
            string verificationkey = PortalSettings.PortalId.ToString() + "-" + CurrentUser.UserID.ToString();
            if (PortalSettings.LoginTabId != Null.NullInteger)
            {
                loginurl = DotNetNuke.Common.Globals.NavigateURL(PortalSettings.LoginTabId, "", "VerificationCode=" + verificationkey);
            }
            else
            {
                loginurl = DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId, "", "ctl=Login", "VerificationCode=" + verificationkey);
            }

            Body = Body.Replace("[VERIFYURL]", loginurl);
            try
            {
                DotNetNuke.Services.Mail.Mail.SendMail(PortalSettings.Email, CurrentUser.Email, "", string.Format(Localization.GetString("NotifySubject_UserDetails.Text", LocalResourceFile), PortalSettings.PortalName), Body, "", "HTML", "", "", "", "");
            }
            catch
            {
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public DotNetNuke.Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new DotNetNuke.Entities.Modules.Actions.ModuleActionCollection();
                Actions.Add(GetNextActionID(), Localization.GetString("ManageTemplates.Action", LocalResourceFile), DotNetNuke.Entities.Modules.Actions.ModuleActionType.AddContent, "", "", EditUrl("ManageTemplates"), false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return Actions;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}