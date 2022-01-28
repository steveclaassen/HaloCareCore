using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Script
{
    public class Scripts
    {
        [Key]
        [Required]
        [DisplayName("Script ID")]
        public int scriptID { get; set; }

        [Required]
        [DisplayName("Script type")]
        public string scriptType { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [Required]
        [DisplayName("Repeats")]
        public int repeats { get; set; }

        [DisplayName("Doctor ID")]
        public Guid? doctorID { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

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
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]

        [DisplayName("Modified date")]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("First line")]
        public bool firstLine { get; set; }

        [DisplayName("Second line")]
        public bool secondLine { get; set; }

        [DisplayName("Salvage therapy")]
        public bool salvageTherapy { get; set; }

        [DisplayName("Prophylaxis")]
        public bool prophylaxis { get; set; }

        [DisplayName("Resistance test")]
        public bool resistanceTest { get; set; }

        [DisplayName("Practice #")]
        public string practiceNo { get; set; }

        [DisplayName("Status")]
        public string Status { get; set; }

    }
}