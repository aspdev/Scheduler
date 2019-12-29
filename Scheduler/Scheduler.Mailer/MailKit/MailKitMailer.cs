using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Scheduler.Mailer.Interfaces;
using System.Threading.Tasks;

namespace Scheduler.Mailer.MailKit
{
    public class MailKitMailer : ISchedulerMailer
    {
        private readonly string host;
        private readonly int port;
        private readonly bool useSsl;
        private readonly string mailBoxAddress;
        
        public MailKitMailer(string host, int port, bool useSsl, string mailBoxAddress)
        {
            this.host = host;
            this.port = port;
            this.useSsl = useSsl;
            this.mailBoxAddress = mailBoxAddress;
               
        }
        
        public async void SendMail(string fromName, string emailTo, string subject, string message, string mailBoxPassword)
        {
                var messageToSend = new MimeMessage();
                messageToSend.From.Add(new MailboxAddress(fromName, mailBoxAddress));
                messageToSend.To.Add(new MailboxAddress(emailTo));
                messageToSend.Subject = subject;
                messageToSend.Body = new TextPart(TextFormat.Html) { Text = message };
               
                using(var smtpClient = new SmtpClient())
                {
                   smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                   await smtpClient.ConnectAsync(host, port, useSsl).ConfigureAwait(false);
                   await smtpClient.AuthenticateAsync(mailBoxAddress, mailBoxPassword).ConfigureAwait(false);
                   await smtpClient.SendAsync(messageToSend).ConfigureAwait(false);
                   await smtpClient.DisconnectAsync(true).ConfigureAwait(false);
                }
        }
    }
    
}
