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
using Connect.Libraries.UserManagement;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using Microsoft.VisualBasic.CompilerServices;
using Telerik.Web.UI;

namespace Connect.Modules.UserManagement.AccountRegistration
{
    public partial class Templates : ConnectUsersModuleBase
    {
        public Templates()
        {
            this.Init += Page_Init;
            this.Load += Page_Load;
        }

        private void Page_Init(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        private void Page_Load(object sender, EventArgs e)
        {
            LocalizeForm();
            if (!Page.IsPostBack)
            {
                BindThemes();
                BindLocales();
                if (Settings.Contains("ModuleTheme"))
                {
                    try
                    {
                        SelectTheme(Conversions.ToString(Settings["ModuleTheme"]));
                    }
                    catch
                    {
                    }
                }

                BindSelectedTheme();
                VerifyPasswordSettings();
            }
        }

        protected void drpThemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindSelectedTheme();
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            bool blnSucess = false;
            SaveTemplates(ref blnSucess);
            if (blnSucess)
            {
                UpdateSettings();
            }
        }

        protected void cmdUpdateExit_Click(object sender, EventArgs e)
        {
            bool blnSucess = false;
            SaveTemplates(ref blnSucess);
            if (blnSucess)
            {
                UpdateSettings();
            }

            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL());
        }

        protected void cmdDeleteSelected_Click(object sender, EventArgs e)
        {
            try
            {
                DeleteTheme();
            }
            catch (Exception ex)
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblDeleteThemeError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL());
        }

        protected void drpLocales_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindSelectedTheme();
        }

        protected void cmdCopySelected_Click(object sender, EventArgs e)
        {
            pnlTemplateName.Visible = true;
        }

        private void VerifyPasswordSettings()
        {
            if (DotNetNuke.Security.Membership.MembershipProvider.Instance().PasswordRetrievalEnabled == false)
            {
                string strNote = Localization.GetString("lblPasswordRetrievalDisabled", LocalResourceFile);
                if (DotNetNuke.Security.Membership.MembershipProvider.Instance().RequiresQuestionAndAnswer)
                {
                    strNote += Localization.GetString("lblRequiresQuestionAndAnswer", LocalResourceFile);
                }

                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, strNote, ModuleMessage.ModuleMessageType.BlueInfo);
            }
        }

        private void LocalizeForm()
        {
            cmdCancel.Text = Localization.GetString("cmdCancel", LocalResourceFile);
            cmdUpdateExit.Text = Localization.GetString("cmdUpdateExit", LocalResourceFile);
            cmdUpdate.Text = Localization.GetString("cmdUpdate", LocalResourceFile);
            cmdCopySelected.Text = Localization.GetString("cmdCopySelected", LocalResourceFile);
            cmdDeleteSelected.Text = Localization.GetString("cmdDeleteSelected", LocalResourceFile);
        }

        private void BindThemes()
        {
            drpThemes.Items.Clear();
            string basepath = Server.MapPath(TemplateSourceDirectory + "/templates/");
            foreach (string folder in System.IO.Directory.GetDirectories(basepath))
            {
                string foldername = folder.Substring(folder.LastIndexOf(@"\") + 1);
                drpThemes.Items.Add(new ListItem(foldername, folder));
            }
        }

        private void SelectTheme(string ThemeName)
        {
            drpThemes.Items.FindByText(ThemeName).Selected = true;
        }

        private void BindSelectedTheme()
        {
            cmdDeleteSelected.Visible = drpThemes.SelectedIndex != 0;
            if (Settings.Contains("ModuleTheme"))
            {
                try
                {
                    if ((Conversions.ToString(Settings["ModuleTheme"]) ?? "") == (drpThemes.SelectedItem.Text ?? ""))
                    {
                        chkUseTheme.Checked = true;
                        DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDeleteSelected, Localization.GetSafeJSString(Localization.GetString("lblThemeInUse", LocalResourceFile)));
                    }
                    else
                    {
                        chkUseTheme.Checked = false;
                        DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDeleteSelected, Localization.GetSafeJSString(Localization.GetString("lblConfirmDelete", LocalResourceFile)));
                    }
                }
                catch
                {
                }
            }
            else
            {
                DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDeleteSelected, Localization.GetSafeJSString(Localization.GetString("lblConfirmDelete", LocalResourceFile)));
                chkUseTheme.Checked = false;
            }

            string path = drpThemes.SelectedValue;
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToAdmin))
                {
                    txtEmailAdminTemplate.Text = GetTemplate(drpThemes.SelectedItem.Value, Libraries.UserManagement.Constants.TemplateName_EmailToAdmin, drpLocales.SelectedValue, true);
                }

                if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToUser_Private))
                {
                    txtEmailUserPrivate.Text = GetTemplate(drpThemes.SelectedItem.Value, Libraries.UserManagement.Constants.TemplateName_EmailToUser_Private, drpLocales.SelectedValue, true);
                }

                if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToUser_Verified))
                {
                    txtEmailUserVerified.Text = GetTemplate(drpThemes.SelectedItem.Value, Libraries.UserManagement.Constants.TemplateName_EmailToUser_Verified, drpLocales.SelectedValue, true);
                }

                if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToUser))
                {
                    txtEmailUser.Text = GetTemplate(drpThemes.SelectedItem.Value, Libraries.UserManagement.Constants.TemplateName_EmailToUser, drpLocales.SelectedValue, true);
                }

                if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_Form))
                {
                    txtFormTemplate.Text = GetTemplate(drpThemes.SelectedItem.Value, Libraries.UserManagement.Constants.TemplateName_Form, drpLocales.SelectedValue, true);
                }
            }
        }

        private void SaveTemplate(string SelectedTheme, string TemplateName, string Locale)
        {
            string path = SelectedTheme + @"\" + TemplateName.Replace(Libraries.UserManagement.Constants.TemplateName_Extension, "." + Locale + Libraries.UserManagement.Constants.TemplateName_Extension);
            if ((PortalSettings.DefaultLanguage.ToLower() ?? "") == (Locale.ToLower() ?? "") | string.IsNullOrEmpty(Locale))
            {
                path = SelectedTheme + @"\" + TemplateName;
            }

            var sw = new System.IO.StreamWriter(path, false);
            if ((TemplateName ?? "") == Libraries.UserManagement.Constants.TemplateName_EmailToAdmin)
            {
                sw.Write(txtEmailAdminTemplate.Text);
            }

            if ((TemplateName ?? "") == Libraries.UserManagement.Constants.TemplateName_EmailToUser_Private)
            {
                sw.Write(txtEmailUserPrivate.Text);
            }

            if ((TemplateName ?? "") == Libraries.UserManagement.Constants.TemplateName_EmailToUser_Verified)
            {
                sw.Write(txtEmailUserVerified.Text);
            }

            if ((TemplateName ?? "") == Libraries.UserManagement.Constants.TemplateName_EmailToUser)
            {
                sw.Write(txtEmailUser.Text);
            }

            if ((TemplateName ?? "") == Libraries.UserManagement.Constants.TemplateName_Form)
            {
                sw.Write(txtFormTemplate.Text);
            }

            sw.Close();
            sw.Dispose();
        }

        private void SaveTemplates(ref bool blnSucess)
        {
            string basepath = drpThemes.SelectedValue;
            if (pnlTemplateName.Visible)
            {
                if (string.IsNullOrEmpty(txtTemplateName.Text))
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblMustEnterTemplateName", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    blnSucess = false;
                    return;
                }

                if (string.IsNullOrEmpty(txtEmailAdminTemplate.Text) | string.IsNullOrEmpty(txtEmailUser.Text) | string.IsNullOrEmpty(txtEmailUserPrivate.Text) | string.IsNullOrEmpty(txtEmailUserVerified.Text) | string.IsNullOrEmpty(txtFormTemplate.Text))
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblMustEnterTemplate", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    blnSucess = false;
                    return;
                }

                string newpath = Server.MapPath(TemplateSourceDirectory + "/templates/") + txtTemplateName.Text;
                try
                {
                    System.IO.Directory.CreateDirectory(newpath);
                }
                catch
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblInvalidFolderName", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    blnSucess = false;
                    return;
                }

                try
                {
                    foreach (string file in System.IO.Directory.GetFiles(basepath))
                    {
                        string destinationpath = newpath + @"\" + file.Substring(file.LastIndexOf(@"\") + 1);
                        System.IO.File.Copy(file, destinationpath);
                    }

                    basepath = newpath;
                }
                catch (Exception ex)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblCouldNotCopyTheme", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    blnSucess = false;
                    return;
                }

                pnlTemplateName.Visible = false;
                BindThemes();
                SelectTheme(txtTemplateName.Text);
                cmdDeleteSelected.Visible = true;
            }

            try
            {
                foreach (string file in System.IO.Directory.GetFiles(basepath))
                {
                    if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToAdmin))
                    {
                        SaveTemplate(drpThemes.SelectedValue, Libraries.UserManagement.Constants.TemplateName_EmailToAdmin, drpLocales.SelectedValue);
                    }

                    if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToUser_Private))
                    {
                        SaveTemplate(drpThemes.SelectedValue, Libraries.UserManagement.Constants.TemplateName_EmailToUser_Private, drpLocales.SelectedValue);
                    }

                    if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToUser_Verified))
                    {
                        SaveTemplate(drpThemes.SelectedValue, Libraries.UserManagement.Constants.TemplateName_EmailToUser_Verified, drpLocales.SelectedValue);
                    }

                    if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_EmailToUser))
                    {
                        SaveTemplate(drpThemes.SelectedValue, Libraries.UserManagement.Constants.TemplateName_EmailToUser, drpLocales.SelectedValue);
                    }

                    if (file.EndsWith(Libraries.UserManagement.Constants.TemplateName_Form))
                    {
                        SaveTemplate(drpThemes.SelectedValue, Libraries.UserManagement.Constants.TemplateName_Form, drpLocales.SelectedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("lblCouldNotWriteTheme", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                blnSucess = false;
                return;
            }

            blnSucess = true;
        }

        private void UpdateSettings()
        {
            var ctrl = new ModuleController();
            ctrl.UpdateTabModuleSetting(TabModuleId, "ModuleTheme", drpThemes.SelectedItem.Text);
        }

        private void DeleteTheme()
        {
            string basepath = drpThemes.SelectedValue;
            foreach (string file in System.IO.Directory.GetFiles(basepath))
                System.IO.File.Delete(file);
            System.IO.Directory.Delete(basepath);
            BindThemes();
            UpdateSettings();
            BindSelectedTheme();
        }

        private void BindLocales()
        {
            var dicLocales = DotNetNuke.ComponentModel.ComponentBase<ILocaleController, LocaleController>.Instance.GetLocales(PortalId);
            if (dicLocales.Count > 1)
            {
                pnlLocales.Visible = true;
            }

            foreach (Locale objLocale in dicLocales.Values)
            {
                var item = new ListItem();
                item.Text = objLocale.Text;
                item.Value = objLocale.Code;
                drpLocales.Items.Add(item);
            }

            try
            {
                drpLocales.Items[0].Selected = true;
            }
            catch
            {
            }
        }
    }
}