using System.Collections.Generic;

namespace Dtx.Net.Mail
{
    public class SmtpMailOptions
    {
        public const string SmtpMail = "SmtpMail";
        public SmtpMailOptions()
        {
            // Set default values
            SmtpClientEnableSsl = false;
            SmtpClientPortNumber = 25;
            SmtpClientTimeout = 100000;
            UseDefaultCredentials = false;
            MailHeaders = new Dictionary<string, string>() { };
        }
        public bool SmtpClientEnableSsl { get; set; }

        public int SmtpClientTimeout { get; set; }

        public int SmtpClientPortNumber { get; set; }

        public string SmtpClientHostAddress { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpPassword { get; set; }

        public string SenderEmailAddress { get; set; }

        public string SenderDisplayName { get; set; }

        public string EmailSubjectTemplate { get; set; }

        public string BccAddresses { get; set; }

        public string SupportEmailAddress { get; set; }

        public string SupportDisplayName { get; set; }

        public Dictionary<string, string> MailHeaders { get; set; }
    }
}
