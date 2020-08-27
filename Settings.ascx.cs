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
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using Microsoft.VisualBasic.CompilerServices;

namespace Connect.Modules.UserManagement.AccountRegistration
{
    public partial class Settings : DotNetNuke.Entities.Modules.ModuleSettingsBase
    {
        public override void LoadSettings()
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    BindPages();
                    BindRoles();
                    if (Settings.Contains("ExternalInterface"))
                        txtInterface.Text = Settings["ExternalInterface"].ToString();
                    if (Settings.Contains("ShowUserName"))
                        drpUsernameMode.SelectedValue = Settings["ShowUserName"].ToString();
                    if (Settings.Contains("ShowDisplayName"))
                        drpDisplaynameMode.SelectedValue = Settings["ShowDisplayName"].ToString();
                    if (Settings.Contains("RedirectAfterSubmit"))
                        drpRedirectAfterSubmit.SelectedValue = Settings["RedirectAfterSubmit"].ToString();
                    if (Settings.Contains("RedirectAfterLogin"))
                        drpRedirectAfterLogin.SelectedValue = Settings["RedirectAfterLogin"].ToString();
                    if (Settings.Contains("UsermanagementTab"))
                        drpUserManagementTab.SelectedValue = Settings["UsermanagementTab"].ToString();
                    if (Settings.Contains("AddToRoleOnSubmit"))
                        drpAddToRole.SelectedValue = Settings["AddToRoleOnSubmit"].ToString();
                    if (Settings.Contains("NotifyRole"))
                        drpNotifyRole.Items.FindByText(Settings["NotifyRole"].ToString()).Selected = true;
                    if (Settings.Contains("NotifyUser"))
                        chkNotifyUser.Checked = Conversions.ToBoolean(Settings["NotifyUser"]);
                    if (Settings.Contains("AddToRoleStatus"))
                        drpRoleStatus.SelectedValue = Conversions.ToString(Settings["AddToRoleStatus"]);
                    if (Settings.Contains("ReCaptchaKey"))
                        txtPrivateCaptchaKey.Text = Conversions.ToString(Settings["ReCaptchaKey"]);
                    if (Settings.Contains("CompareFirstNameLastName"))
                        chkCompareFirstNameLastName.Checked = Conversions.ToBoolean(Settings["CompareFirstNameLastName"]);
                    if (Settings.Contains("ValidateEmailThroughRegex"))
                        chkValidateEmailThroughRegex.Checked = Conversions.ToBoolean(Settings["ValidateEmailThroughRegex"]);
                    if (Settings.Contains("EmailRegex"))
                    {
                        txtEmailRegex.Text = Conversions.ToString(Settings["EmailRegex"]);
                    }
                    else
                    {
                        try
                        {
                            txtEmailRegex.Text = Conversions.ToString(UserController.GetUserSettings(PortalId)["Security_EmailValidation"]);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception exc)           // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
                var objModules = new DotNetNuke.Entities.Modules.ModuleController();
                objModules.UpdateTabModuleSetting(TabModuleId, "ReCaptchaKey", txtPrivateCaptchaKey.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "ExternalInterface", txtInterface.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "ShowUserName", drpUsernameMode.SelectedValue);
                objModules.UpdateTabModuleSetting(TabModuleId, "ShowDisplayName", drpDisplaynameMode.SelectedValue);
                objModules.UpdateTabModuleSetting(TabModuleId, "RedirectAfterSubmit", drpRedirectAfterSubmit.SelectedValue);
                objModules.UpdateTabModuleSetting(TabModuleId, "RedirectAfterLogin", drpRedirectAfterLogin.SelectedValue);
                objModules.UpdateTabModuleSetting(TabModuleId, "UsermanagementTab", drpUserManagementTab.SelectedValue);
                objModules.UpdateTabModuleSetting(TabModuleId, "AddToRoleOnSubmit", drpAddToRole.SelectedValue);
                // we need the rolename for sending mails to users, therefor store here the rolename rather than the id!
                objModules.UpdateTabModuleSetting(TabModuleId, "NotifyRole", drpNotifyRole.SelectedItem.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "NotifyUser", chkNotifyUser.Checked.ToString());
                objModules.UpdateTabModuleSetting(TabModuleId, "AddToRoleStatus", drpRoleStatus.SelectedValue);
                objModules.UpdateTabModuleSetting(TabModuleId, "CompareFirstNameLastName", chkCompareFirstNameLastName.Checked.ToString());
                objModules.UpdateTabModuleSetting(TabModuleId, "ValidateEmailThroughRegex", chkValidateEmailThroughRegex.Checked.ToString());
                objModules.UpdateTabModuleSetting(TabModuleId, "EmailRegex", txtEmailRegex.Text);
            }
            catch (Exception exc)           // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindPages()
        {
            var tabs = TabController.GetPortalTabs(PortalId, Null.NullInteger, true, true, false, false);
            drpRedirectAfterSubmit.DataSource = tabs;
            drpRedirectAfterSubmit.DataBind();
            drpRedirectAfterLogin.DataSource = tabs;
            drpRedirectAfterLogin.DataBind();
            drpUserManagementTab.DataSource = tabs;
            drpUserManagementTab.DataBind();
        }

        private void BindRoles()
        {
            var rc = new DotNetNuke.Security.Roles.RoleController();
            var roles = rc.GetPortalRoles(PortalId);
            drpAddToRole.DataSource = roles;
            drpAddToRole.DataBind();
            drpAddToRole.Items.Insert(0, new ListItem("---", "-1"));
            drpNotifyRole.DataSource = roles;
            drpNotifyRole.DataBind();
            drpNotifyRole.Items.Insert(0, new ListItem("---", "-1"));
        }
    }
}