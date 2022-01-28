using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ChronicMedication
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("NAPPI")]
        public string nappiStart { get; set; }

        [DisplayName("Description")]
        public string description { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Created date")]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [Required]
        public bool Active { get; set; }
    }

}