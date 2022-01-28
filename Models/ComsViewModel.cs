using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ComsViewModel
    {
        public Queries queries { get; set; }
        public SmsMessages smsMessages { get; set; }
        public Emails emailMessages { get; set; }
        public Contact contacts { get; set; }
        public Address address { get; set; }
        public QueryTypes queryType { get; set; }
        public Dependant dependant { get; set; }
        public Language language { get; set; }

        public MemberImports MemberInfo { get; set; }
        public List<MemberImports> MemberInfos { get; set; }

        public List<Notes> notes { get; set; }
        public List<Queries> queriess { get; set; }
        public List<Assignments> assignments { get; set; }
        public List<QueryTypes> queryTypes { get; set; }

        public List<EnquiryFullViewModel> enquiryList { get; set; }
        public EnquiryComments enquiryComment { get; set; }

        public SmsMessageTemplates smsMessageTemplate { get; set; }
        public List<SmsMessageTemplates> smsMessageTemplates { get; set; }

        public EmailTemplates emailTemplate { get; set; }
        public List<EmailTemplates> emailTemplates { get; set; }

        public MemberInformation memberInformation { get; set; }
        public List<MemberInformation> memberInformations { get; set; }

        public List<ContactInformationVM> contactInformationVMs { get; set; }

        public Attachments attachment { get; set; }
        public List<Attachments> attachments { get; set; }

        public List<PatientDisclaimer> patientDisclaimers { get; set; }

        public AuthorizationLetters authLetter { get; set; }
        public List<AuthorizationLetters> authLetters { get; set; }

        //HCare-1061
        public DiabeticDairy diabeticDairy { get; set; }
        public List<DiabeticDairy> diabeticDairies { get; set; }

        //HCare-864
        public DisclaimerResponse DisclaimerResponse { get; set; }
        public List<DisclaimerResponse> DisclaimerResponses { get; set; }
        public List<DisclaimerFullView> DisclaimerFullView { get; set; }

        public List<CommunicationLogVM> CommunicationLog { get; set; }
        public string programCode { set; get; } //HCare-1252

        public List<NextOFKin> NextOfKin { get; set; } //hcare-1361


        public List<Emails> EmailHistory { get; set; } //hcare-1380
        public List<AttachmentTemplate> AttachmentTemplates { get; set; } //hcare-1380
        public List<EmailAttachmentHistory> EmailAttachmentHistories { get; set; } //hcare-1380

        public List<SmsMessages> SMSHistory { get; set; } //hcare-1378
        public List<EmailLayout> EmailHeader { get; set; } //hcare-1384
        public List<EmailLayout> EmailFooter { get; set; } //hcare-1384




        //HCare-996
        public bool CanCreate { set; get; }
        public bool CanEdit { set; get; }
        public bool CanRead { set; get; }
    }

}