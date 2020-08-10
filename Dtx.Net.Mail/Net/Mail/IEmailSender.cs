using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Dtx.Net.Mail
{
    public interface IEmailSender
    {
        string ConvertTextForEmailBody(string text);
        Task<Dtx.Result> Send(string subject, string body);
        Task<Dtx.Result> Send(MailAddress recipient, string subject, string body, MailPriority priority);
        Task<Dtx.Result> Send(MailAddress sender, MailAddressCollection recipients, string subject, string body, MailPriority priority, List<string> attachmentPathNames, DeliveryNotificationOptions deliveryNotification);
    }
}
