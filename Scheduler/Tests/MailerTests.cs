using Scheduler.Mailer.MailKit;
using Xunit;

namespace Tests
{
    public class MailerTests
    {
        [Fact]
        public void SendsEmail()
        {
            var mailer = new MailKitMailer("smtp.webio.pl", 465, true, "scheduler-notifications@smartscheduler.pl");
            var firstName = "Lukasz";
            var userName = "lukasz.dobrzynski@wp.pl";
            var temporaryPassword = "fJp9KDKhRIoR3HPaZEEnjJmWm5ul7RnukTfmEahyQI4=";
            var clientUrl = "https://smartscheduler.pl";
            
            
            string message = $"<b>Dear {firstName}</b></br><p>You're receiving this message because your Scheduler Account has been created.</p><p>Your <b>username</b>: {userName}</p><p>Your <b>first-time login password</b>: {temporaryPassword}</p><p>Follow the link below to change your password and log in to Scheduler application:</p><p><a href={clientUrl}>Login</a></p><p>Best</p><p>Scheduler Team</p>";

            mailer.SendMail("Scheduler-Notifications", "lukasz.dobrzynski@wp.pl", "Scheduler Account", message,
                "E_N_T_E_R_P_A_S_S_W_O_R_D");
            
        }
    }
}