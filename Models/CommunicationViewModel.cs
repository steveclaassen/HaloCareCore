using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class CommunicationViewModel
    {
        public SmsMessageTemplates smsMessageTemplate { get; set; }
        public List<SmsMessageTemplates> smsMessageTemplates { get; set; }

        public EmailTemplates emailTemplate { get; set; }
        public List<EmailTemplates> emailTemplates { get; set; }

        public List<EducationalNotes> EducationalNotes { get; set; }

        public PopUpTemplate PopUpTemplate { get; set; } //hcare-1374
        public List<PopUpTemplate> PopUpTemplates { get; set; } //hcare-1374
        public PathologyAutomatedTemplate automatedTemplate { set; get; }
        public List<PathologyAutomatedTemplate> pathologyAutomatedTemplates { set; get; } 

        public List<AttachmentTemplate> Attachments { get; set; } //hcare-1380
        
        public List<EmailLayout> EmailLayouts { get; set; } //hcare-1384
    }
}
