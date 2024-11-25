using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Core.Common
{
    public class EmailHelper
    {
        public void SendMail(SendEmail sendEmail)
        {
            // SMTP Server configuration
            string smtpHost = "smtp.office365.com";  // SMTP server address (e.g., smtp.gmail.com)
            int smtpPort = 465;  // SMTP port (587 for TLS, 465 for SSL)
            string smtpUser = "info@shttermanagementdev.btprojects.in";  // Your email address
            string smtpPass = "Smtpmail@1234%^";  // Your email password

            // Email message details
            string fromAddress = sendEmail.FromEmail ?? "";  // Sender's email address
            string toAddress = "info@shttermanagementdev.btprojects.in";  // Recipient's email address
            string subject = sendEmail.Subject ?? "";  // Email subject
            string body = sendEmail.Body ?? "";  // Email body content

            try
            {
                // Create the mail message object
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(fromAddress);
                mailMessage.To.Add(toAddress);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;  // Set to true if you want to send HTML content

                // Create the SMTP client object
                SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
                smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtpClient.EnableSsl = true;  // Enable SSL if required by the SMTP server

                // Send the email
                smtpClient.Send(mailMessage);

                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
