using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ZipValidate
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            var tranCode = txtTranCode.Text;
            var extensions = new string[] { ".txt", ".pdf", ".jpg", ".xls" };
            var location = Server.MapPath($"~/UploadedFiles/{tranCode}");

            checkListFiles.Items.Clear();

            if (fileUpload1.HasFile)
            {
                string fileUploadMsg = string.Empty;
                foreach (HttpPostedFile htfiles in fileUpload1.PostedFiles)
                {

                    var getFileName = Path.GetFileName(htfiles.FileName);
                    var fullPath = $"{location}/{getFileName}";

                    EnsurePathExists(location);
                    htfiles.SaveAs(fullPath);
                    //htfiles.SaveAs(Server.MapPath("~/UploadedFiles/" + getFileName));

                    var isRemove = false;

                    var fileExtension = Path.GetExtension(getFileName);
                    
                    if (fileExtension == ".zip")
                    {
                        using (ZipArchive archive = ZipFile.OpenRead($"{location}/{getFileName}"))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (extensions.Any(ext => entry.FullName.EndsWith(ext)))
                                {
                                    //entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
                                    //lblMessage.Text = "Archive extracted successfully and containes following files";
                                }
                                else
                                {
                                    isRemove = true;
                                    fileUploadMsg += string.IsNullOrEmpty(fileUploadMsg) ? $"{Path.GetFileName(fullPath)}," : $" {Path.GetFileName(fullPath)}";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (extensions.Any(a => a == fileExtension))
                        {

                        }
                        else
                        {
                            isRemove = true;
                            fileUploadMsg += string.IsNullOrEmpty(fileUploadMsg) ? $"{Path.GetFileName(fullPath)}," : $" {Path.GetFileName(fullPath)}";
                        }
                    }

                    lblMessage.Text = $"File [{fileUploadMsg}] not allowed.";

                    if (isRemove)
                    {
                        // remove file
                        TryToDelete(fullPath);
                    }
                }
            }

            if (Directory.Exists(location))
            {
                var files = GetFilesDictionary(location);

                if (files.Count == 0) checkListFiles.Items.Clear();

                checkListFiles.DataSource = files;
                checkListFiles.DataTextField = "Key";
                checkListFiles.DataValueField = "Value";
                checkListFiles.DataBind();
            }

        }

        public static void EnsurePathExists(string path)
        {
            // ... Set to folder path we must ensure exists.
            try
            {
                // ... If the directory doesn't exist, create it.
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception)
            {
                // Fail silently.
            }
        }

        /// <summary>
        /// Wrap the Delete method with an exception handler.
        /// </summary>
        static bool TryToDelete(string f)
        {
            try
            {
                // A.
                // Try to delete the file.
                File.Delete(f);
                return true;
            }
            catch (IOException)
            {
                // B.
                // We could not delete the file.
                return false;
            }
        }

        public static string[] GetFiles(string path)
        {
            var result = Directory.GetFiles($@"{path}");

            return result;
        }

        public static string[] GetFilesName(string path)
        {
            var result = new List<string>();
            var files = GetFiles(path);
            foreach(var file in files)
            {
                result.Add(Path.GetFileName(file));
            }

            return result.ToArray();
        }

        public static Dictionary<string, string> GetFilesDictionary(string path)
        {
            var result = new Dictionary<string, string>();

            var files = GetFiles(path);

            foreach(var file in files)
            {
                result.Add(Path.GetFileName(file), Path.GetFullPath(file));
            }

            return result;
        }

        private static bool SendEmail(string frSendEmail, string toSendEmail, string usrEmail, string pwdEmail, string hostEmail, int? portEmail, string subjectEmail, string bodyEmail, string[] attachFiles, int loopSendEmailErr, string[] logData)
        {
            bool result = false;
            MailMessage mail = null;
            SmtpClient SmtpServer = null;

            try
            {
                if (!string.IsNullOrEmpty(frSendEmail) && !string.IsNullOrEmpty(toSendEmail) &&
                    !string.IsNullOrEmpty(hostEmail))
                {
                    mail = new MailMessage();
                    SmtpServer = new SmtpClient(hostEmail);
                    mail.From = new MailAddress(frSendEmail);
                    Array.ForEach(toSendEmail.Split(';'),
                        c =>
                        {
                            mail.To.Add(c);
                        }
                    );
                    mail.Subject = subjectEmail;
                    mail.IsBodyHtml = true;
                    mail.Body = bodyEmail;

                    if (attachFiles != null && attachFiles.Length > 0)
                    {
                        Array.ForEach(
                            attachFiles,
                            filePath =>
                            {
                                mail.Attachments.Add(new System.Net.Mail.Attachment(filePath));
                            }
                        );
                    }
                    if (portEmail.HasValue)
                        SmtpServer.Port = portEmail.Value;
                    if (!string.IsNullOrEmpty(usrEmail) && !string.IsNullOrEmpty(pwdEmail))
                    {
                        SmtpServer.UseDefaultCredentials = false;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(usrEmail, pwdEmail);
                        SmtpServer.EnableSsl = true;
                        SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    }

                    for (int i = 1; i <= loopSendEmailErr; i++)
                    {
                        try
                        {
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            SmtpServer.Send(mail);
                            result = true;

                            stopwatch.Stop();
                            var msg = $"Time elapsed: {ConvertMillisecondsToSeconds(stopwatch.ElapsedMilliseconds)} seconds";
                        }
                        catch (Exception ex)
                        {
                            result = false;
                        }

                        if (result == false && i != loopSendEmailErr)
                            System.Threading.Thread.Sleep(60000); // waiting resend email.
                        else if (result)
                        {
                            i = (loopSendEmailErr + 1); // out loop send completed.
                        }
                        else if (i == loopSendEmailErr)
                            i++; // out loop send error
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                if (SmtpServer != null)
                {
                    SmtpServer.Dispose();
                    SmtpServer = null;
                }
                if (mail != null)
                {
                    mail.Dispose();
                    mail.Dispose();
                    mail = null;
                }
            }

            return result;
        }

        private static double ConvertMillisecondsToSeconds(double milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds).TotalSeconds;
        }

        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            var attachFiles = new List<string>();

            var config = new
            {
                FromSendEmail = "mongkolm@hatari.net",
                ToSendEmail = "b5209194@gmail.com;mongkolm@hatari.net",
                UserEmail = "mongkolm@hatari.net",
                PasswordEmail = "",
                HostEmail = "smtp.gmail.com",
                PortEmail = 587,
                SubjectEmail = "Test",
                BodyEmail = "Test",
                LoopSendEmailError = 1
            };


            foreach (ListItem li in checkListFiles.Items)
            {
                if (li.Selected)
                {
                    attachFiles.Add(li.Value);
                }
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var send = SendEmail(config.FromSendEmail, config.ToSendEmail, config.UserEmail, config.PasswordEmail, config.HostEmail,
                config.PortEmail, config.SubjectEmail, config.BodyEmail, attachFiles.ToArray(), config.LoopSendEmailError, null);

            stopwatch.Stop();
            lblSendEmailMsg.Text = $"Time elapsed: {ConvertMillisecondsToSeconds(stopwatch.ElapsedMilliseconds)} seconds";


            if (send)
            {
                lblSendEmailMsg.Text += ", Send Email Success";
            }
            else
            {
                lblSendEmailMsg.Text += ", Send Email Not Success";
            }
        }

        protected void btnGenTranCode_Click(object sender, EventArgs e)
        {
            txtTranCode.Text = RandomString(20, true);
        }

        // Generate a random string with a given size  
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }
}