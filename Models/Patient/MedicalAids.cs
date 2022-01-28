using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class MedicalAid
    {
        [Key]
        public Guid MedicalAidID { get; set; }

        [Required]
        [DisplayName("Medical scheme")]
        [StringLength(50, ErrorMessage = "Name restricted to 50 characters")] //HCare-969
        public string Name { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "Code restricted to 15 characters")] //HCare-1177
        [DisplayName("Account")]
        public string medicalAidCode { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

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

        [DisplayName("Location")]
        public string logoLocation { get; set; }

        [DisplayName("ChroniLine Carrier")]
        public bool clCarrier { get; set; }

        public virtual MedicalAidSettings medicalAidSettings { get; set; }

        [DisplayName("Risk rules active")]
        public bool riskRules { get; set; }

        [DisplayName("Incl. extracts")]
        public bool reporting { get; set; }//HCare-937

        [DisplayName("Incl. Title|Name|Surname sync")]
        public bool titleSync { get; set; }//HCare-1040

        [DisplayName("Disclaimer req.")]
        public bool disclaimer { get; set; }//HCare-864

        [DisplayName("Medical aid account")]
        public string AccountCode { get; set; }

        [DisplayName("Referral")]
        public string Referral { get; set; }

        //==>> reporting

        [DisplayName("Production")]
        public bool Production { get; set; } //hcare-1292

        [DisplayName("Extracts")]
        public bool Extracts { get; set; } //hcare-1292

        [DisplayName("Imports")]
        public bool Imports { get; set; } //hcare-1292

        [DisplayName("Exports")]
        public bool Exports { get; set; } //hcare-1292

        [DisplayName("Advanced search")]
        public bool AdvancedSearch { get; set; } //hcare-1328



        [DisplayName("Claims")]
        public bool Claims { get; set; } //hcare-1362












        [DisplayName("Sms opt out")]
        public bool smsoptout { get; set; }//HCare-1327

        [DisplayName("SMS OPT-OUT Report email")]
        public string optOutEmail { get; set; }


        [DisplayName("Allow sync")]
        public bool sync { get; set; }//HCare-1321

        //[NotMapped]
        //public IEnumerable<string> Referral_Concat
        //{
        //    get
        //    {
        //        if (Referral != null)
        //        {
        //            return Referral.Split(',').ToList();
        //        }
        //        else
        //        {
        //            return Referral_Concat;
        //        }
        //    }
        //    set
        //    {
        //        Referral = string.Join(",", value);
        //    }
        //}
        //[DisplayName("Medical aid claim code ")]
        //public string MedicalAidClaimCode { get; set; } //HCare-1086
    }
}