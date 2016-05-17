using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net.Mail;

namespace DecoAPI.Email
{
    public class Email
    {
        public string Address { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }

        public Email()
        {

        }

        public Email(string address, string message, string subject)
        {
            Address = address;
            Message = message;
            Subject = subject;
        }

        /// <summary>
        /// Method to send email messages, given address and message contents
        /// </summary>
        public void SendMail()
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add(Address);
                mailMessage.Bcc.Add("aaron.jeter@magna.com");
                mailMessage.From = new MailAddress("DoNotReply@decostar.com");
                mailMessage.Subject = Subject;
                mailMessage.Body = Message;
                SmtpClient smtpClient = new SmtpClient("smtp.tor.magna.intranet");

                //Console.WriteLine("Mail sent to: " + address + "\n--> " + contents + "\n");

                smtpClient.Send(mailMessage);
                //LogEmail(address, contents);
            }
            catch (Exception)
            { }
        }
    }
}