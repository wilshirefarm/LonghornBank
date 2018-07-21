using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net; 

namespace Team16LonghornBank.Utilities
{
    public class EmailMessaging
    {
        public static void SendEmail(String toEmailAddress, String emailSubject, String emailBody)
        {
            //Create an email client to send the emails
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("team16longhornbank@gmail.com", "Wilsha!!"),
                EnableSsl = true
            };

            //Add everything that you need to the body of the message
            String finalMessage = emailBody + "\n\n DISCLAIMER: If you are not the intendednt recipient, please delete this message. ";

            //Create an email address object for the sender addreess
            MailAddress senderEmail = new MailAddress("team16longhornbank@gmail.com", "Team 16");

            MailMessage mm = new MailMessage();
            mm.Subject = "Team 16 - " + emailSubject;
            mm.Sender = senderEmail;
            mm.From = senderEmail;
            mm.To.Add(new MailAddress(toEmailAddress));
            mm.Body = finalMessage;
           // mm.IsBodyHtml = true;
            client.Send(mm);
        }
    }
}