using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace HaloCareCore.Helpers
{
    public class EmailHelper
    {
        private IConfiguration Configuration;
        public EmailHelper(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        public bool SendEmail(string msg, string link, string to, string subject, DateTime now)
        {
            try
            {
                new SmtpClient(Configuration.GetSection("EmailSettings")["SmtpServer"]).Send(new MailMessage
                {
                    From = new MailAddress(Configuration.GetSection("EmailSettings")["SupportEmailsFrom"].ToString()),
                    To = { to },
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = msg + link,

                    

                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}