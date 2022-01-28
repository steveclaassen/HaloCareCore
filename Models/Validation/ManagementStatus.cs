using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class ManagementStatus
    {
        [Key]
        [Required]
        [DisplayName("Status code")]
        [StringLength(10, ErrorMessage = "Code restricted to 10 characters")] //HCare-969
        public string statusCode { get; set; }

        [Required]
        [DisplayName("Status")]
        [StringLength(50, ErrorMessage = "Name restricted to 50 characters")] //HCare-969
        public string statusName { get; set; }

        [DisplayName("Status type")]
        public string statusType { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }
               
        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

        public virtual ICollection<PatientManagementStatusHistory> PatientManagementStatusHistory { get; set; }

        [DisplayName("Program")]
        public string programCode { get; set; }

        // filtering the enrolment stages 
        public int enrolmentStage { get; set; }

    }
}