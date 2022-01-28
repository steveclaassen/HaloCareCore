using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class PatientRiskRatingHistory
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [Required]
        public Guid dependantID { get; set; }

        [DisplayName("Risk rating")]
        public string RiskId { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
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

        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Reason")]
        public string reason { get; set; }

        [DisplayName("Sent To CL")]
        public int sentToCl { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        public List<RiskRatingTypes> RiskRatings { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        public int patientStagingHistoryId  { set;get;}

    }
}