using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaloCareCore.Models.Questionnaire
{
    public class QuestionnaireOther
    {
        [Key]
        public int QuestionnaireOtherID { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }

        [DisplayName("What is your occupation")]
        public string occupation { get; set; }

        [DisplayName("Yes/No")]
        public bool occupationCheck { get; set; }

        [DisplayName("Shift worker comment")]
        public string shiftWorker { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool shiftWorkCheck { get; set; }

        [DisplayName("Lypohypertrophy")]
        public string lypohypertrophy { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool lypohypertrophyCheck { get; set; }

        [DisplayName("Lypohypertrophy diagnosed")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? lypohypertrophyDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool recDrugs { get; set; }

        [DisplayName("Recreational drugs last use")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? recDrugsLastUsed { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool depression { get; set; }

        [DisplayName("Comment")]
        public string depressionComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool? TBScreen { get; set; }

        [DisplayName("TB Screening date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? TBScreenDate { get; set; }

        [DisplayName("Outcome")]
        public string TBScreenResult { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool tbDiagnosed { get; set; }

        [DisplayName("TB treatement start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? tbTreatmentStartDate { get; set; }

        [DisplayName("TB treatement end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? tbTreatmentEndDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool Pregnant { get; set; }

        [DisplayName("Expected delivery")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EDD { get; set; }

        [DisplayName("Treating doctor")]
        public string TreatingDoctor { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool isDoctorAware { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool frailCareCheck { get; set; }

        [DisplayName("Frail care")]
        public string frailCare { get; set; }

        [DisplayName("Comment")]
        public string frailCareComment { get; set; }
        [NotMapped]
        public string[] frailCare_Concat
        {
            get
            {
                if (frailCare != null)
                {
                    return frailCare.Split(',');
                }
                else
                {
                    return frailCare_Concat;
                }
            }
            set
            {
                frailCare = string.Join(",", value);
            }
        }

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

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool hasKidneyInfection { get; set; }

        public string bloodTestFrequency { get; set; }

        [DisplayName("Last blood test")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? bloodTestEffectiveDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}
