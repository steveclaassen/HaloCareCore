using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Script
{
    public class ICD10Master
    {
        [Key]
        [Required]
        [DisplayName("ICD-10 code")]
        public string ICD10Code { get; set; }
        [DisplayName("ICD-10 description")]

        public string ICD10Description { get; set; }
        [DisplayName("Created date")]
        public DateTime? createdDate { set; get; }
        [DisplayName("Created by")]
        public string createdBy { set; get; }
        [DisplayName("Modified date")]
        public DateTime? modifiedDate { set; get; }
        [DisplayName("Modified by")]
        public string modifiedBy { set; get; }
        [DisplayName("Valid primary")]
        public string ValidPrimary { get; set; }
        [DisplayName("Valid clinical")]
        public string ValidClinical { get; set; }
        [DisplayName("Valid asterisk")]
        public string ValidAsterisk { get; set; }
        [DisplayName("Valid dagger")]
        public string ValidDagger { get; set; }
        [DisplayName("Valid sequelae")]
        public string ValidSequelae { get; set; }
        [DisplayName("Status")]
        public string Status { get; set; }
        [DisplayName("Age range")]
        public string AgeRange { get; set; }
        [DisplayName("Gender")]
        public string Gender { get; set; }
        [DisplayName("From date")]
        public DateTime? FromDate { get; set; }
        [DisplayName("End date")]
        public DateTime? EndDate { get; set; }
        [DisplayName("Chapter number")]

        public string ChapterNumber { get; set; }
        [DisplayName("Chapter description")]
        public string ChapterDescription { get; set; }
        [DisplayName("Group code")]
        public string GroupCode { get; set; }
        [DisplayName("Group description")]
        public string GroupDescription { get; set; }
        [DisplayName("Active")]
        public bool Active { get; set; }    }
}