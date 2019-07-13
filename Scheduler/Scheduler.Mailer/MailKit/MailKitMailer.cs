using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Scheduler.Mailer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
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


        public void SendMail(string FromName, string emailTo, string subject, string message, string mailBoxPassword)
        {
            var task = Task.Run(async () =>
            {
                var messageToSend = new MimeMessage();
                messageToSend.From.Add(new MailboxAddress(FromName, mailBoxAddress));
                messageToSend.To.Add(new MailboxAddress(emailTo));
                messageToSend.Subject = subject;
                messageToSend.Body = new TextPart(TextFormat.Html) { Text = message };

                using(var smtpClient = new SmtpClient())
                {
                   
                   await smtpClient.ConnectAsync(host, port, useSsl);
                   await smtpClient.AuthenticateAsync(mailBoxAddress, mailBoxPassword);
                   await smtpClient.SendAsync(messageToSend);
                   await smtpClient.DisconnectAsync(true);
                }
            });

            
        }
    }
}
