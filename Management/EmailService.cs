using HaloCareCore.DAL;
using HaloCareCore.Repos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace HaloCareCore.Management
{
    public class EmailService
    {
        private IMemberRepository _member;
        private readonly IConfiguration Configuration;
        private OH17Context context;
        public EmailService()
        {
            _member = new MemberRepository(Configuration, context);
        }

        private bool SendEmail(string msg, string to, string subject, DateTime now)
        {
            try
            {
                new SmtpClient(Configuration.GetSection("EmailSettings")["SmtpServer"]).Send(new MailMessage
                {
                    From = new MailAddress(Configuration.GetSection("EmailSettings")["SupportEmailsFrom"].ToString()),
                    To = { to },
                    Subject = subject,
                    IsBodyHtml = false,
                    Body = msg,
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public string SendEmail(string recipientsAddress, string subject, string body, bool isHtml, string fileName, string actualFileName, int[] extraAttachments = null)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
            try
            {
                string[] receipients = recipientsAddress.Split(';', ',').ToArray();

                MailMessage mMessage = new MailMessage();

                MailAddress mailAFrom = new MailAddress(Configuration.GetSection("EmailSettings")["SupportEmailsFrom"].ToString(), subject);
                mMessage.From = mailAFrom;

                MailAddress mailAReply = new MailAddress(Configuration.GetSection("EmailSettings")["SupportEmailsFrom"].ToString());

                for (int x = 0; x < receipients.Length; x++)
                {
                    if (!String.IsNullOrEmpty(receipients[x].Trim()))
                    {
                        mMessage.To.Add(new MailAddress(receipients[x].Trim()));
                    }
                }

                mMessage.Subject = subject;

                System.Net.Mail.Attachment Attachment = null;

                if (File.Exists(fileName) == true)
                {
                    Attachment = new System.Net.Mail.Attachment(fileName);
                    Attachment.Name = actualFileName;
                    mMessage.Attachments.Add(Attachment);
                }

                foreach (var attachment_no in extraAttachments)
                {
                    var attachment = _member.GetAttachment(attachment_no);
                    if (File.Exists(path + attachment.Link) == true)
                    {
                        Attachment = new System.Net.Mail.Attachment(path + attachment.Link);
                        Attachment.Name = attachment.Link;
                        mMessage.Attachments.Add(Attachment);
                    }
                }

                mMessage.IsBodyHtml = isHtml;
                mMessage.Body = body;

                SmtpClient client = new SmtpClient(Configuration.GetSection("EmailSettings")["SmtpServer"]);
                client.Timeout = 600000;

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mMessage);

                if (Attachment != null)
                    Attachment.Dispose();

                return "Sent";
            }
            catch (Exception ex)
            {
                string filePath = @"D:\Data\Logs\HC\Error.txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                return "Email Failure";
            }
        }
    }
}