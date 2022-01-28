using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class DiabeticDairy
    {
        [Key]
        [Required]
        public int dairyId { get; set; }

        [DisplayName("Returned date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? returnedDate { get; set; }
        [Required]
        [DisplayName("Diary sent date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime sentDate { get; set; }
        [DisplayName("Notes")]
        public string notes { get; set; }
        public string ImportSubject { set; get; }
        [DisplayName("Educational notes")]
        public bool SingedEducationalNote { set; get; }
        public bool TelehonicalEducationalNote { set; get; }
        [DisplayName("Created by")]
        public string createdBy { set; get; }
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

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }
        public bool assignementSent { set; get; }
        public Guid programId { set; get; }
    }
}