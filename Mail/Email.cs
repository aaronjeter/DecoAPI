using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net.Mail;

namespace DecoAPI.Mail
{
    public class Email
    {
        public string Address { get; set; }
        public string Contents { get; set; }

        public Email(string address, string contents)
        {
            Address = address;
            Contents = contents;
        }

        public Email()
        { }

        public void Send(string address, string contents)
        {
            Address = address;
            Contents = contents;
            Send();            
        }

        public void Send()
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add(Address);
                mailMessage.From = new MailAddress("DoNotReply@decostar.com");
                mailMessage.Subject = "CAR Notification";
                mailMessage.Body = Contents;
                SmtpClient smtpClient = new SmtpClient("deccarr02ds");

                smtpClient.Send(mailMessage);
            }
            catch (Exception)
            { }
        }
    }
}