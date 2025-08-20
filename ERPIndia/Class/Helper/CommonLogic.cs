using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// Common logic class
    /// </summary>
    public class CommonLogic
    {
        #region Variable Declaration

        //private static bool isMailDefaultCredentials = false;
        private static string[] defaultPageSize = new string[7] { "10", "20", "30", "50", "100", "200", "500" };

        #endregion
       
        #region Public Methods

        #region Encrypt/Decrypt Methods

        /// <summary>
        /// Encrypts the specified encrypt text.
        /// </summary>
        /// <param name="encryptText">The encrypt text.</param>
        /// <returns>Returns encrypted string.</returns>
        public static string Encrypt(string encryptText)
        {
            return new AES().AESEncryptCtr(encryptText, GetConfigValue(StringConstants.AESPassword), Convert.ToInt32(GetConfigValue(StringConstants.AESBits)));
        }

        /// <summary>
        /// Decrypts the specified decrypt text.
        /// </summary>
        /// <param name="decryptText">The decrypt text.</param>
        /// <returns>Returns decrypted string.</returns>
        public static string Decrypt(string decryptText)
        {
            return new AES().AESDecryptCtr(decryptText, GetConfigValue(StringConstants.AESPassword), Convert.ToInt32(GetConfigValue(StringConstants.AESBits)));
        }

        #endregion

        #region Paging Methods

        /// <summary>
        /// Gets the page drop down list.
        /// </summary>
        /// <param name="selectedValue">The selected value.</param>
        /// <param name="adminDefaultPageSize">Size of the admin default page.</param>
        /// <returns>returns paging drop down item list</returns>
        public static List<SelectListItem> GetPageDropDownList(string selectedValue, string adminDefaultPageSize)
        {
            List<SelectListItem> pageDropDownList = new List<SelectListItem>();

            foreach (string pageSize in defaultPageSize)
            {
                if (pageSize.Equals(adminDefaultPageSize))
                {
                    pageDropDownList.Add(new SelectListItem() { Text = string.Format(StringConstants.DefaultPageSize, pageSize), Value = pageSize });
                }
                else
                {
                    pageDropDownList.Add(new SelectListItem() { Text = pageSize, Value = pageSize });
                }
            }

            SelectListItem item = pageDropDownList.FirstOrDefault(p => p.Value == selectedValue);

            if (item != null)
            {
                item.Selected = true;
            }
            else if (!string.IsNullOrEmpty(adminDefaultPageSize) && pageDropDownList.FirstOrDefault(p => p.Value == adminDefaultPageSize) != null)
            {
                pageDropDownList.FirstOrDefault(p => p.Value == adminDefaultPageSize).Selected = true;
            }

            return pageDropDownList;
        }

        #endregion

        #region QueryString Methods

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns query string.</returns>
        public static string GetQueryString(string paramName)
        {
            if (HttpContext.Current.Request.QueryString[paramName] != null)
            {
                return HttpContext.Current.Request.QueryString[paramName].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the query string long value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns query string long value.</returns>
        public static long GetQueryStringLongValue(string paramName)
        {
            return SqlHelper.ParseNativeLong(GetQueryString(paramName));
        }

        /// <summary>
        /// Gets the query string boolean value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns query string boolean value.</returns>
        public static bool GetQueryStringBoolValue(string paramName)
        {
            string tempString = GetQueryString(paramName).ToUpperInvariant();
            return tempString.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || tempString.Equals("1", StringComparison.InvariantCultureIgnoreCase) || tempString.Equals("YES", StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region FormData Methods

        /// <summary>
        /// Gets the form data object.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns form data object.</returns>
        public static object GetFormDataObject(string paramName)
        {
            if (HttpContext.Current.Request.Form[paramName] != null)
            {
                return HttpContext.Current.Request.Form[paramName];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the form data string.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns form data string.</returns>
        public static string GetFormDataString(string paramName)
        {
            if (HttpContext.Current.Request.Form[paramName] != null)
            {
                return HttpContext.Current.Request.Form[paramName].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region WebConfig Methods

        /// <summary>
        /// Gets the config value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns config string value.</returns>
        public static string GetConfigValue(string paramName)
        {
            try
            {
                return ConfigurationManager.AppSettings[paramName].ToString();
            }
            catch
            {
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the config boolean value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns config boolean value.</returns>
        public static bool GetConfigBoolValue(string paramName)
        {
            string tmp = GetConfigValue(paramName);
            return tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase) || tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the config integer value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns config integer value.</returns>
        public static int GetConfigIntValue(string paramName)
        {
            return SqlHelper.ParseNativeInt(GetConfigValue(paramName));
        }

        #endregion

        #region Session Methods

        /// <summary>
        /// Gets the logged user id.
        /// </summary>
        /// <returns>Returns logged user id.</returns>
        public static int GetLoggedUserId()
        {
            return SqlHelper.ParseNativeInt(CommonLogic.GetSessionValue(StringConstants.UserId));
        }
        /// <summary>
        /// Gets the session value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns session string value.</returns>
        public static string GetSessionValue(string paramName)
        {
            // Check if HttpContext.Current is null
            if (HttpContext.Current == null)
            {
                return string.Empty; // Or handle appropriately
            }

            // Check if Session is null
            if (HttpContext.Current.Session == null)
            {
                return string.Empty; // Or handle appropriately
            }

            // Now safely check the session parameter
            if (HttpContext.Current.Session[paramName] != null)
            {
                return Utils.ToStringOrEmpty(HttpContext.Current.Session[paramName]);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the session object.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns session object.</returns>
        public static object GetSessionObject(string paramName)
        {
            return HttpContext.Current.Session[paramName];
        }

        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        public static void SetSessionValue(string paramName, object paramValue)
        {
            HttpContext.Current.Session[paramName] = paramValue;
        }

        /// <summary>
        /// Sessions the sign out.
        /// </summary>
        public static void SessionSignOut()
        {
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.RemoveAll();

            SetSessionValue(StringConstants.UserId, null);
            SetSessionValue(StringConstants.UserName, null);
            SetSessionValue(StringConstants.FullName, null);
            SetSessionValue(StringConstants.RoleId, null);
            SetSessionValue(StringConstants.RoleName, null);
            SetSessionValue(StringConstants.MenuList, null);
            SetSessionValue(StringConstants.FinancialYear, null);
        }

        public static bool HasRoles(string roles)
        {
            if (!string.IsNullOrEmpty(roles))
            {
                string roleID = string.Concat(",", CommonLogic.GetSessionValue(StringConstants.RoleId), ",");
                return string.Concat(",", roles, ",").Contains(roleID);
            }
            return true;
        }

        #endregion

        #region Cookie Methods

        /// <summary>
        /// Sets the cookie value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="expires">The expires.</param>
        public static void SetCookieValue(string paramName, object paramValue, DateTime expires)
        {
            try
            {
                HttpCookie httpCookie = new HttpCookie(paramName, paramValue.ToString());
                httpCookie.Expires = expires;

                if (HttpContext.Current.Request.Cookies[paramName] != null)
                {
                    System.Web.HttpContext.Current.Request.Cookies.Set(httpCookie);
                }
                else
                {
                    System.Web.HttpContext.Current.Response.Cookies.Add(httpCookie);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns cookie string value.</returns>
        public static string GetCookieValue(string paramName)
        {
            if (HttpContext.Current.Request.Cookies[paramName] != null)
            {
                return HttpContext.Current.Request.Cookies[paramName].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Clears the cookie.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        public static void ClearCookie(string paramName)
        {
            HttpCookie httpCookie = HttpContext.Current.Request.Cookies[paramName];

            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
                httpCookie.Expires = DateTime.Now.AddDays(-2);
                System.Web.HttpContext.Current.Response.Cookies.Add(httpCookie);
            }
        }

        #endregion

        #region Mail Methods

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="fromMail">From mail.</param>
        /// <param name="toMail">To mail.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <returns>Returns true if send mail otherwise false.</returns>
        public static bool SendMail(string fromMail, string toMail, string subject, string body)
        {
            return SendMail(fromMail, toMail, subject, body, false, string.Empty, string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="fromMail">From mail.</param>
        /// <param name="toMail">To mail.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="isBodyHTML">if set to <c>true</c> [is body HTML].</param>
        /// <returns>Returns true if send mail otherwise false.</returns>
        public static bool SendMail(string fromMail, string toMail, string subject, string body, bool isBodyHTML)
        {
            return SendMail(fromMail, toMail, subject, body, isBodyHTML, string.Empty, string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="fromMail">From mail.</param>
        /// <param name="toMail">To mail.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="isBodyHTML">if set to <c>true</c> [is body HTML].</param>
        /// <param name="filePaths">The file paths.</param>
        /// <returns>Returns true if send mail otherwise false.</returns>
        public static bool SendMail(string fromMail, string toMail, string subject, string body, bool isBodyHTML, string[] filePaths)
        {
            return SendMail(fromMail, toMail, subject, body, isBodyHTML, string.Empty, string.Empty, string.Empty, filePaths);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="fromMail">From mail.</param>
        /// <param name="toMail">To mail.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <returns>Returns true if send mail otherwise false.</returns>
        public static bool SendMail(string fromMail, string toMail, string subject, string body, string cc, string bcc, string replyTo)
        {
            return SendMail(fromMail, toMail, subject, body, false, cc, bcc, replyTo, null);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="fromMail">From mail.</param>
        /// <param name="toMail">To mail.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="isBodyHTML">if set to <c>true</c> [is body HTML].</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <returns>Returns true if send mail otherwise false.</returns>
        public static bool SendMail(string fromMail, string toMail, string subject, string body, bool isBodyHTML, string cc, string bcc, string replyTo)
        {
            return SendMail(fromMail, toMail, subject, body, isBodyHTML, cc, bcc, replyTo, null);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="fromMail">From mail.</param>
        /// <param name="toMail">To mail.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="isBodyHTML">if set to <c>true</c> [is body HTML].</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="filePaths">The file paths.</param>
        /// <returns>Returns true if send mail otherwise false.</returns>
        public static bool SendMail(string fromMail, string toMail, string subject, string body, bool isBodyHTML, string cc, string bcc, string replyTo, string[] filePaths)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                SmtpClient smtpClient = new SmtpClient(GetApplicationValue("SMTPHost"), SqlHelper.ParseNativeInt(GetApplicationValue("SMTPPort")));

                mailMessage.From = new MailAddress(fromMail);
                mailMessage.To.Add(toMail);
                mailMessage.Body = body;
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = isBodyHTML;
                mailMessage.Priority = MailPriority.High;

                if (!string.IsNullOrEmpty(cc))
                {
                    mailMessage.CC.Add(cc);
                }

                if (!string.IsNullOrEmpty(bcc))
                {
                    foreach (string str in bcc.Split(new char[] { ',', ';' }))
                    {
                        if (str.Trim().Length > 0)
                        {
                            mailMessage.Bcc.Add(new MailAddress(str.Trim()));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(replyTo))
                {
                    mailMessage.ReplyToList.Add(replyTo);
                }

                //if (!isMailDefaultCredentials)
                if (!string.IsNullOrEmpty(GetApplicationValue("SMTPUserName")) && !string.IsNullOrEmpty(GetApplicationValue("SMTPPassword")))
                {
                    //smtpClient.Credentials = new System.Net.NetworkCredential(StringConstants.SMTPUserName, StringConstants.SMTPPassword);
                    ////smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(GetApplicationValue("SMTPUserName"), GetApplicationValue("SMTPPassword"));

                    if (GetApplicationValue("SMTPHost").ToLower() == "smtp.gmail.com")
                    {
                        smtpClient.EnableSsl = true;
                    }

                }
                else
                {
                    smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                if (filePaths != null && filePaths.Length > 0)
                {
                    foreach (string path in filePaths)
                    {
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            FileInfo fileInfo = new FileInfo(path);
                            Attachment attachment = new Attachment(path);
                            attachment.Name = string.Concat(subject.Replace(" ", "_"), fileInfo.Extension);

                            mailMessage.Attachments.Add(attachment);
                        }
                    }
                }

                try
                {
                    smtpClient.Send(mailMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    GetExceptionMessage(ex);
                }

                return false;
            }
        }

        #endregion

        #region Image Methods

        /// <summary>
        /// Gets the active inactive image.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="status">The status.</param>
        /// <returns>Returns active inactive image string.</returns>
        public static string GetActiveInactiveImage(string id, string status)
        {
            if (status == "active")
            {
                return "<a href='javascript:void(0)' style='text-decoration:none' id='img" + status + id + "' title='Click to make inactive' name='imgInactive' class='SetStatusClick'  onclick='return SetActiveInactive(this," + id + ");' ><i class='icon-ok icon-center-check'></i></a>";
            }
            else if (status == "inactive")
            {
                return "<a href='javascript:void(0)' style='text-decoration:none' id='img" + status + id + "' title='Click to make active' name='imgActive' class='SetStatusClick'  onclick='return SetActiveInactive(this," + id + ");' ><i class='icon-ban-circle icon-center-edit'></i></a>";
            }
            else
            {
                return "<a href='javascript:void(0)' style='text-decoration:none' id='img" + status + id + "' title='Click here to delete' name='imgDelete' class='SetStatusClick'  onclick='return SetActiveInactive(this," + id + ");' ><i class='icon-trash icon-center-trash'></i></a>";
            }
        }

        /// <summary>
        /// Resizes the image.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destPath">The destination path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="isResizeWithResolution">if set to <c>true</c> [is resize with resolution].</param>
        public static void ResizeImage(string sourcePath, string destPath, int width, int height, bool isResizeWithResolution)
        {
            using (Bitmap bmp = CreateThumbnail(sourcePath, width, height, isResizeWithResolution))
            {
                if (bmp != null)
                {
                    try
                    {
                        bmp.Save(destPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the thumbnail.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="isResizeWithResolution">if set to <c>true</c> [is resize with resolution].</param>
        /// <returns>Returns thumbnail image.</returns>
        public static Bitmap CreateThumbnail(string filename, int width, int height, bool isResizeWithResolution)
        {
            System.Drawing.Bitmap bmpOut = null;

            using (Bitmap bmpIn = new Bitmap(filename))
            {
                try
                {
                    ImageFormat imageFormat = bmpIn.RawFormat;

                    decimal ratio = default(decimal);
                    int newWidth = 0;
                    int newHeight = 0;

                    if (isResizeWithResolution)
                    {
                        if (bmpIn.Width > bmpIn.Height)
                        {
                            ratio = (decimal)width / bmpIn.Width;
                            newWidth = width;
                            newHeight = (int)(bmpIn.Height * ratio);
                        }
                        else
                        {
                            ratio = (decimal)height / bmpIn.Height;
                            newHeight = height;
                            newWidth = (int)(bmpIn.Width * ratio);
                        }

                        if (newHeight > height || newWidth > width)
                        {
                            if (bmpIn.Width > bmpIn.Height)
                            {
                                ratio = (decimal)height / bmpIn.Height;
                                newHeight = height;
                                newWidth = (int)(bmpIn.Width * ratio);
                            }
                            else
                            {
                                ratio = (decimal)width / bmpIn.Width;
                                newWidth = width;
                                newHeight = (int)(bmpIn.Height * ratio);
                            }
                        }
                    }
                    else
                    {
                        newWidth = width;
                        newHeight = height;
                    }

                    bmpOut = new Bitmap(newWidth, newHeight);
                    Graphics gps = Graphics.FromImage(bmpOut);
                    gps.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    gps.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                    gps.DrawImage(bmpIn, 0, 0, newWidth, newHeight);
                }
                catch (Exception ex)
                {
                    GetExceptionMessage(ex);
                    bmpOut = null;
                }
            }

            return bmpOut;
        }

        #endregion

        #region RewriteURL Methods

        /// <summary>
        /// Gets the formatted URL.
        /// </summary>
        /// <param name="strUrl">The STR URL.</param>
        /// <returns>Returns formatted URL.</returns>
        public static string GetFormattedURL(string strUrl)
        {
            if (!string.IsNullOrEmpty(strUrl))
            {
                System.Web.UI.Control resobj = new System.Web.UI.Control();
                return resobj.ResolveUrl("~/" + strUrl.Replace('\\', Path.DirectorySeparatorChar));
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats the string.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <returns>Returns formatted string.</returns>
        public static string FormatString(string stringValue)
        {
            if (!string.IsNullOrEmpty(stringValue))
            {
                string strTitle = stringValue.ToString()
                                              .Trim()
                                              .Trim('-')
                                              .ToLower()
                                              .Replace("c#", "C-Sharp")
                                              .Replace("vb.net", "VB-Net")
                                              .Replace("asp.net", "Asp-Net")
                                              .Replace(".", "-")
                                              .Replace("&", "and")
                                              .Replace(" ", "_");

                char[] chars = "$%#@!*?;:~`+=()[]{}|\\'<>,/^\".".ToCharArray();

                for (int i = 0; i <= chars.Length - 1; i++)
                {
                    string strChar = chars.GetValue(i).ToString();

                    if (strTitle.Contains(strChar))
                    {
                        strTitle = strTitle.Replace(strChar, string.Empty);
                    }
                }

                return strTitle.Replace(" ", "-")
                               .Replace("--", "-")
                               .Replace("---", "-")
                               .Replace("----", "-")
                               .Replace("-----", "-")
                               .Replace("----", "-")
                               .Replace("---", "-")
                               .Replace("--", "-")
                               .Trim()
                               .Trim('-');
            }
            else
            {
                return "-";
            }
        }

        #endregion

        #region URL Related Methods

        /// <summary>
        /// Gets the encoded URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns encoded URL.</returns>
        public static string GetEncodedURL(string url)
        {
            return url.Replace('&', '|').Replace('=', '$');
        }

        /// <summary>
        /// Gets the decoded URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Returns decoded URL.</returns>
        public static string GetDecodedURL(string url)
        {
            return url.Replace('|', '&').Replace('$', '=');
        }

        /// <summary>
        /// Gets the page URL with query string.
        /// </summary>
        /// <returns>Returns  page URL with query string.</returns>
        public static string GetPageUrlWithQueryString()
        {
            return GetPageUrlWithQueryString(string.Empty);
        }

        /// <summary>
        /// Gets the page URL with query string.
        /// </summary>
        /// <param name="additionalNameValuePair">The additional name value pair.</param>
        /// <returns>Returns page URL with query string.</returns>
        public static string GetPageUrlWithQueryString(string additionalNameValuePair)
        {
            return GetPageUrlWithQueryString(additionalNameValuePair, true);
        }

        /// <summary>
        /// Gets the page URL with query string.
        /// </summary>
        /// <param name="additionalNameValuePair">The additional name value pair.</param>
        /// <param name="encodeValues">if set to <c>true</c> [encode values].</param>
        /// <returns>Returns page URL with query string.</returns>
        public static string GetPageUrlWithQueryString(string additionalNameValuePair, bool encodeValues)
        {
            StringBuilder url = new StringBuilder();

            url.Append(HttpContext.Current.Items["RequestedPage"] != null ? HttpContext.Current.Items["RequestedPage"].ToString() : GetPageName(false));
            string originalNameValuePair = HttpContext.Current.Items["RequestedQueryString"] != null ? HttpContext.Current.Items["RequestedQueryString"].ToString() : HttpContext.Current.Request.Url.Query;

            if (originalNameValuePair.StartsWith("?"))
            {
                originalNameValuePair = originalNameValuePair.Remove(0, 1);
            }

            string[] originalQueryStrings = originalNameValuePair.Split('&');
            string[] additionalQueryStrings = additionalNameValuePair.Split('&');
            string[] allQueryStrings = originalQueryStrings.Union(additionalQueryStrings).ToArray();

            if (allQueryStrings.Length > 0)
            {
                url.Append("?");

                foreach (string nameValuePair in allQueryStrings)
                {
                    string[] queryStringValues = nameValuePair.Split('=');

                    if (queryStringValues.Length == 2)
                    {
                        url.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(queryStringValues[0]), encodeValues ? HttpUtility.UrlEncode(queryStringValues[1]) : queryStringValues[1]);
                    }
                }
            }

            return url.ToString().TrimEnd('&');
        }

        /// <summary>
        /// Gets the name of the page.
        /// </summary>
        /// <param name="isIncludePath">if set to <c>true</c> [is include path].</param>
        /// <returns>Returns name of the page.</returns>
        public static string GetPageName(bool isIncludePath)
        {
            string str = CommonLogic.GetServerVariable("SCRIPT_NAME");

            if (!isIncludePath)
            {
                int index = str.LastIndexOf("/");

                if (index != -1)
                {
                    str = str.Substring(index + 1);
                }
            }

            return str;
        }

        /// <summary>
        /// Gets the page name with query string.
        /// </summary>
        /// <returns>Returns page name with query string.</returns>
        public static string GetPageNameWithQueryString()
        {
            string pageName = GetPageName(false);
            string queryParam = GetServerVariable("QUERY_STRING");

            if (!string.IsNullOrEmpty(queryParam))
            {
                pageName = string.Concat(pageName, "?", queryParam);
            }

            return pageName;
        }

        /// <summary>
        /// Gets the page raw URL.
        /// </summary>
        /// <returns>Returns page raw URL.</returns>
        public static string GetPageRawUrl()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        /// <summary>
        /// Gets the page referrer URL.
        /// </summary>
        /// <returns>Returns page referrer URL.</returns>
        public static string GetPageReferrerUrl()
        {
            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                return HttpContext.Current.Request.UrlReferrer.ToString();
            }

            return string.Empty;
        }

        #endregion

        #region Server Variable Methods

        /// <summary>
        /// Gets the server variable.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns server variable.</returns>
        public static string GetServerVariable(string paramName)
        {
            if (HttpContext.Current.Request.ServerVariables[paramName] != null)
            {
                return HttpContext.Current.Request.ServerVariables[paramName].ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the client IP address.
        /// </summary>
        /// <returns>Returns client IP address.</returns>
        public static string GetClientIPAddress()
        {
            string address = GetServerVariable("HTTP_X_FORWARDED_FOR").Trim();

            if (string.IsNullOrEmpty(address) || address.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                address = GetServerVariable("HTTP_X_CLUSTER_CLIENT_IP").Trim();
            }

            if (string.IsNullOrEmpty(address) || address.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                address = GetServerVariable("HTTP_CLIENT_IP").Trim();
            }

            if (string.IsNullOrEmpty(address) || address.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                address = GetServerVariable("REMOTE_ADDR").Trim();
            }

            return address.Split(',')[0].Trim();
        }

        #endregion

        #region Application Methods

        /// <summary>
        /// Gets the application value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Returns application value.</returns>
        public static string GetApplicationValue(string paramName)
        {
            if (System.Web.HttpContext.Current.Application[paramName] != null)
            {
                return System.Web.HttpContext.Current.Application[paramName].ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the application value.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        public static void SetApplicationValue(string paramName, object paramValue)
        {
            HttpContext.Current.Application[paramName] = paramValue;
        }

        #endregion

        #region Exception Methods

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Returns exception message.</returns>
        public static string GetExceptionMessage(Exception exception)
        {
            string result = string.Empty;

            WriteFile("AppError", string.Concat("Message : ", exception.Message, Environment.NewLine, "StackTrace:", exception.StackTrace));

            if (exception is System.Data.SqlClient.SqlException)
            {
                result = GetSQLException(exception);
                result = string.Concat(StringConstants.DBErrorMsg, "<br/>", result);
            }
            else
            {
                result = StringConstants.CodeUnknownError;
            }

            // result = exception.Message;
            return result;
        }

        /// <summary>
        /// Gets the SQL exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Returns SQL exception string.</returns>
        private static string GetSQLException(Exception exception)
        {
            System.Data.SqlClient.SqlException sqlException = exception as System.Data.SqlClient.SqlException;

            switch (sqlException.Number)
            {
                //// Invalid Database
                case 4060:
                    return StringConstants.DatabaseFailedToConnect;

                // Login Failed
                case 18456:
                    return StringConstants.DatabaseFailedToLogin;

                // ForeignKey Violation
                case 547:
                    return StringConstants.DatabaseForeignKeyViolation;

                // Unique Index/Constraint Violation
                case 2627:
                case 2601:
                    return StringConstants.DatabaseUniqueConstraintViolation;

                case 102:
                    return StringConstants.DatabaseIncorrectSyntax;

                default:
                    return StringConstants.DatabaseUnknownError;
            }
        }

        public static void WriteFile(string fileName, string content)
        {
            try
            {
                string path = Path.Combine(System.Web.HttpContext.Current.Request.PhysicalApplicationPath, "ErrorLog\\");
                //System.Web.HttpContext.Current.Request.MapPath("ErrorLog\\");


                if (!string.IsNullOrEmpty(content))
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    StreamWriter responseText = new StreamWriter(string.Concat(path, fileName, "_", DateTime.Now.ToString("yyyyMMdd"), ".txt"), true);

                    responseText.WriteLine(DateTime.Now.ToString());
                    responseText.WriteLine(content);
                    responseText.WriteLine("-------------------");

                    responseText.Dispose();
                }
            }
            catch
            {
                //  throw ex;
            }
        }

        #endregion

        #endregion
    }
}