using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class OtherMedicalHistory
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool respiratoryTract { get; set; }

        [DisplayName("Comment")]
        public string respiratoryTractComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? respiratoryTractEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool pneumonia { get; set; }

        [DisplayName("Comment")]
        public string pneumoniaComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? pneumoniaEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool meningitis { get; set; }

        [DisplayName("Comment")]
        public string meningitisComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? meningitisEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool lymphadenopathy { get; set; }

        [DisplayName("Comment")]
        public string lymphadenopathyComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? lymphadenopathyEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool diarrhoea { get; set; }

        [DisplayName("Comment")]
        public string diarrhoeaComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? diarrhoeaEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool bladderInfection { get; set; }

        [DisplayName("Comment")]
        public string bladderInfectionComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? bladderInfectionEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool weightLoss { get; set; }

        [DisplayName("Comment")]
        public string weightLossComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? weightLossEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool cancer { get; set; }

        [DisplayName("Comment")]
        public string cancerComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? cancerEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool immunization { get; set; }

        [DisplayName("Comment")]
        public string immunizationComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? immunizationEffectiveDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool cervicalCancer { get; set; }

        [DisplayName("Comment")]
        public string cervicalCancerComment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? cervicalCancerEffectiveDate { get; set; }

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

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}

