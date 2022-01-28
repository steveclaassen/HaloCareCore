using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class Practices
    {
        [Key]
        [Required]
        [DisplayName("Practice #")]
        public string practiceNo { get; set; }

        [DisplayName("Practice name")]
        public string practiceName { get; set; }

        [DisplayName("Council #")]
        public string councilNo { get; set; }

        [DisplayName("Service provided")]
        public string service { get; set; }

        [DisplayName("Discipline")]
        public string decipline { get; set; }

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

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

    }
}