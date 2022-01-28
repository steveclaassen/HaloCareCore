using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class MedicalAidClaimCode
    {
        [Key]
        public int id { get; set; }

        [DisplayName("Account")]
        [StringLength(15, ErrorMessage = "Account restricted to 15 characters")] //HCare-1177
        public string medicalAidCode { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [DisplayName("Entry type")]
        public string entryType { get; set; }

        [DisplayName("Carrier")]
        [Required] //1216
        [StringLength(10, ErrorMessage = "Carrier restricted to 10 characters")] //HCare-1177
        public string claimCode { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Modified by")]                    //HCare-1086
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]                  //HCare-1086
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}