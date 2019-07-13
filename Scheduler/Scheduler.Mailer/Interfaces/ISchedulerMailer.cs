using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Mailer.Interfaces
{
    public interface ISchedulerMailer
    {
        void SendMail(string FromName, string emailTo, string subject, string message, string mailBoxPassword);
    }
}
