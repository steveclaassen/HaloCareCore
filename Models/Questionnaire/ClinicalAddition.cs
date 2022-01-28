using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Questionnaire
{
    public class ClinicalAddition
    {
        [Key]
        [DisplayName("Clinical ID")]
        public int clinicalAdditionID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        [Required]
        [DisplayName("Hospitalisations")]
        public string hospitalisations { get; set; }

        [Required]
        [DisplayName("New condition diagnosed")]
        public string newConditionDiagnosed { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [Required]
        [DisplayName("Illness/immunisation")]
        public string immunisation { get; set; }

        public bool assignementSent { set; get; }
        public Guid programId { get; set; }

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

    }
}