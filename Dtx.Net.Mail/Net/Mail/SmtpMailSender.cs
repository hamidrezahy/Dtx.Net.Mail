using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Dtx.Net.Mail
{
    public class SmtpMailSender : IEmailSender
    {
        private readonly SmtpMailOptions _options;
        public SmtpMailSender(IOptions<SmtpMailOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// تبديل متن به حالتی که برای ايميل مناسب گردد
        /// </summary>
        /// <param name="text">متن</param>
        public string ConvertTextForEmailBody(string text)
        {
            if (text == null)
            {
                return (string.Empty);
            }

            text =
                text
                .Replace(System.Convert.ToChar(13).ToString(), "<br />") // Return Key.
                .Replace(System.Convert.ToChar(10).ToString(), string.Empty) // Return Key.
                .Replace(System.Convert.ToChar(9).ToString(), "&nbsp;&nbsp;&nbsp;&nbsp;"); // TAB Key.

            return (text);
        }

        /// <summary>
		/// ارسال نامه الکترونيکی
		/// </summary>
		/// <param name="subject">موضوع</param>
		/// <param name="body">شرح</param>
		/// <param name="mailSettings">تنظیمات</param>
		public async Task<Dtx.Result> Send
            (
                string subject,
                string body
            )
        {
            return await Send
                (null,
                null,
                subject,
                body,
                MailPriority.High,
                null,
                DeliveryNotificationOptions.Never);
        }

        /// <summary>
        /// ارسال نامه الکترونيکی
        /// </summary>
        /// <param name="recipient">دريافت کننده</param>
        /// <param name="subject">موضوع</param>
        /// <param name="body">شرح</param>
        /// <param name="priority">اهميت</param>
        /// <param name="mailSettings"></param>
        /// <param name="mailSettings">تنظیمات</param>
        public async Task<Dtx.Result> Send
            (
                MailAddress recipient,
                string subject,
                string body,
                MailPriority priority
            )
        {
            // **************************************************
            MailAddressCollection
                oRecipients = new MailAddressCollection();

            oRecipients.Add(recipient);
            // **************************************************

            return await Send
                (null,
                oRecipients,
                subject,
                body,
                priority,
                null,
                DeliveryNotificationOptions.Never);
        }

        /// <summary>
        /// ارسال نامه الکترونيکی
        /// </summary>
        /// <param name="sender">فرستنده</param>
        /// <param name="recipients">گيرندگان</param>
        /// <param name="subject">موضوع</param>
        /// <param name="body">شرح</param>
        /// <param name="priority">اهميت</param>
        /// <param name="attachmentPathNames">پيوست ها</param>
        /// <param name="deliveryNotification">رسيد ارسال</param>
        /// <param name="mailSettings">تنظیمات</param>
        public async Task<Dtx.Result> Send
            (
                MailAddress sender,
                MailAddressCollection recipients,
                string subject,
                string body,
                MailPriority priority,
                System.Collections.Generic.List<string> attachmentPathNames,
                DeliveryNotificationOptions deliveryNotification
            )
        {
            Dtx.Result oResult = new Dtx.Result();

            // **************************************************
            MailAddress oSender = null;
            SmtpClient oSmtpClient = null;
            MailMessage oMailMessage = null;
            // **************************************************

            try
            {
                // **************************************************
                // *** Mail Message Configuration *******************
                // **************************************************
                oMailMessage = new MailMessage();

                // **************************************************
                oMailMessage.To.Clear();
                oMailMessage.CC.Clear();
                oMailMessage.Bcc.Clear();
                oMailMessage.Attachments.Clear();
                oMailMessage.ReplyToList.Clear();
                // **************************************************

                // **************************************************
                if (sender != null)
                {
                    oSender = sender;
                }
                else
                {
                    if (string.IsNullOrEmpty(_options.SenderDisplayName))
                    {
                        oSender =
                            new MailAddress
                                (address: _options.SenderEmailAddress,
                                displayName: _options.SenderEmailAddress,
                                displayNameEncoding: System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        oSender =
                            new MailAddress
                                (address: _options.SenderEmailAddress,
                                displayName: _options.SenderDisplayName,
                                displayNameEncoding: System.Text.Encoding.UTF8);
                    }
                }

                oMailMessage.From = oSender;
                oMailMessage.Sender = oSender;

                // Note: Below Code Obsoleted in .NET 4.0
                //oMailMessage.ReplyTo = oSender;

                oMailMessage.ReplyToList.Add(oSender);
                // **************************************************

                if (recipients == null)
                {
                    MailAddress oMailAddress = null;

                    if (string.IsNullOrEmpty(_options.SupportDisplayName))
                    {
                        oMailAddress =
                            new MailAddress
                                (address: _options.SupportEmailAddress,
                                displayName: _options.SupportEmailAddress,
                                displayNameEncoding: System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        oMailAddress =
                            new MailAddress
                                (address: _options.SupportEmailAddress,
                                displayName: _options.SupportDisplayName,
                                displayNameEncoding: System.Text.Encoding.UTF8);
                    }

                    oMailMessage.To.Add(oMailAddress);
                }
                else
                {
                    // Note: Wrong Usage!
                    // oMailMessage.To = recipients;

                    foreach (MailAddress oMailAddress in recipients)
                    {
                        oMailMessage.To.Add(oMailAddress);
                    }
                }

                if (string.IsNullOrEmpty(_options.BccAddresses) == false)
                {
                    _options.BccAddresses =
                        _options.BccAddresses
                        .Replace(";", ",")
                        .Replace("|", ",")
                        .Replace("،", ",");

                    string[] strBccAddresses =
                        _options.BccAddresses.Split(',');

                    foreach (string strBccAddress in strBccAddresses)
                    {
                        bool blnFound =
                            oMailMessage.To
                            .Where(current => string.Compare(current.Address, strBccAddress, true) == 0)
                            .Any();

                        if (blnFound == false)
                        {
                            blnFound =
                                oMailMessage.Bcc
                                .Where(current => string.Compare(current.Address, strBccAddress, true) == 0)
                                .Any();

                            if (blnFound == false)
                            {
                                MailAddress oMailAddress =
                                    new MailAddress(address: strBccAddress);

                                oMailMessage.Bcc.Add(item: oMailAddress);
                            }
                        }
                    }

                    // Note: [BccAddresses] must be separated with comma character (",")
                    //oMailMessage.Bcc.Add(mailSettings.BccAddresses);
                }

                // **************************************************
                oMailMessage.SubjectEncoding = System.Text.Encoding.UTF8;

                if (string.IsNullOrEmpty(_options.EmailSubjectTemplate))
                {
                    oMailMessage.Subject = subject;
                }
                else
                {
                    oMailMessage.Subject =
                        string.Format(_options.EmailSubjectTemplate, subject);
                }
                // **************************************************

                // **************************************************
                oMailMessage.Body = body;
                oMailMessage.IsBodyHtml = true;
                oMailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                // **************************************************

                // **************************************************
                oMailMessage.Priority = priority;
                oMailMessage.DeliveryNotificationOptions = deliveryNotification;
                // **************************************************

                if ((attachmentPathNames != null) && (attachmentPathNames.Count > 0))
                {
                    foreach (string strAttachmentPathName in attachmentPathNames)
                    {
                        if (System.IO.File.Exists(strAttachmentPathName))
                        {
                            Attachment oAttachment =
                                new Attachment(strAttachmentPathName);

                            oMailMessage.Attachments.Add(oAttachment);
                        }
                    }
                }


                // **************************************************
                foreach (KeyValuePair<string, string> item in _options.MailHeaders)
                {
                    oMailMessage.Headers.Add(item.Key, item.Value);
                }
                // **************************************************
                // *** /Mail Message Configuration ******************
                // **************************************************

                // **************************************************
                // *** Smtp Client Configuration ********************
                // **************************************************
                oSmtpClient = new SmtpClient();

                // **************************************************
                oSmtpClient.Port = _options.SmtpClientPortNumber;
                oSmtpClient.Timeout = _options.SmtpClientTimeout;
                oSmtpClient.EnableSsl = _options.SmtpClientEnableSsl;
                // **************************************************

                oSmtpClient.DeliveryMethod =
                    SmtpDeliveryMethod.Network;

                oSmtpClient.Host =
                    _options.SmtpClientHostAddress;

                // **************************************************
                // **************************************************

                // **************************************************
                oSmtpClient.UseDefaultCredentials = _options.UseDefaultCredentials;

                if (_options.UseDefaultCredentials == false)
                {
                    System.Net.NetworkCredential oNetworkCredential =
                        new System.Net.NetworkCredential
                            (userName: _options.SmtpUsername, password: _options.SmtpPassword);

                    oSmtpClient.Credentials = oNetworkCredential;
                }
                // **************************************************
                // *** /Smtp Client Configuration *******************
                // **************************************************

                await oSmtpClient.SendMailAsync(oMailMessage);
                oResult.Success = true;
            }
            catch (System.Exception ex)
            {
                oResult.Success = false;
                oResult.Exception = ex;
            }
            finally
            {
                if (oMailMessage != null)
                {
                    oMailMessage.Dispose();
                    oMailMessage = null;
                }

                if (oSmtpClient != null)
                {
                    oSmtpClient.Dispose();
                    oSmtpClient = null;
                }
            }

            return oResult;
        }
    }
}
