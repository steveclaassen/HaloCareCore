using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class PatientClinicalRulesHistory
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public Guid dependentId { get; set; }

        [Required]
        public string ruleBroken { get; set; }

        [Required]
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
        public bool active { get; set; }

        [DisplayName("Pathology")]
        public int pathologyID { get; set; }

        [DisplayName("Assignment")]
        public string assignmentItem { get; set; }
        //Hcare-1116
        public string pathFieldName { get; set; }
        public string pathFieldValue { get; set; }

    }
}