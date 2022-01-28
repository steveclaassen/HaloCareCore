using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class CoMormidConditions
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public int coMorbidId { get; set; }

        [Required]
        public Guid dependantID { get; set; }

        public Guid? questionnaireID { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [Required]
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

        [DisplayName("CoMorbid treatment")]
        public string CoMorbidTreatement { get; set; }

        [DisplayName("Treatment end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? treatementEndDate { get; set; }

        [DisplayName("Diagnosis date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? diagnosisDate { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        //hcare-1293
        public string icd10code { get; set; }
        public bool hasComormid { set; get; }



    }
}