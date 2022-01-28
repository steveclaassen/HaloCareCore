using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class MedicalAidPlans
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public Guid medicalAidId { get; set; }

        [Required]
        [DisplayName("Plan code")]
        [StringLength(15, ErrorMessage = "Code restricted to 15 characters")] //HCare-1177
        public string planCode { get; set; }

        [Required]
        [DisplayName("Plan name")]
        [StringLength(50, ErrorMessage = "Name restricted to 50 characters")] 
        public string Name { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

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
        public DateTime? modifieddate { get; set; }

        [DisplayName("Claim code")]
        public string claimCode { get; set; }

        [DisplayName("Service plan code")]
        public string servicePlanCode { get; set; }
    }
}