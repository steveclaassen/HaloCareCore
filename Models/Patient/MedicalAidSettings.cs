using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class MedicalAidSettings
    {
        [Key]
        [Required]
        public int settingID { get; set; }

        [Required]
        public Guid medicalAidId { get; set; }

        [DisplayName("HIV pre-approval")]
        public bool hivPreAppovalRequired { get; set; }

        [DisplayName("Days for pathology")]
        public int daysForPathology { get; set; }

        [DisplayName("Authorisation req")]
        public bool authRequired { get; set; }

        [DisplayName("G/Chronic communication")]
        public int daysForCommunicationGen { get; set; }

        [DisplayName("Days for communication HIV")]
        public int daysForCommunicationHiv { get; set; }

        [DisplayName("Overdue script SMS")]
        public int overdueScriptSms { get; set; }

        [DisplayName("Overdue pathology SMS")]
        public int overduePathologySms { get; set; }

        [DisplayName("Confidential")]
        public bool confidential { get; set; }
      
        [DisplayName("Email Address")]
        public string email { get; set; }

        [DisplayName("Email Subject")]
        public string schemeSubject { set; get; }
      
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}