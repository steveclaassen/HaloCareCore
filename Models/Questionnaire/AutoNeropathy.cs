using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models
{
    public class AutoNeropathy
    {
        public int AutoNeropathyID { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }


        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool autoNeuroSympCheck { get; set; }

        [DisplayName("Symptoms")]
        public string autoNeuroSympComment { get; set; }

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

        //Needs to be geworked
        public string[] autoNeuroSymptom_Concat
        {
            get
            {
                if (autoNeuroSympComment != null)
                {
                    return autoNeuroSympComment.Split(',');
                }
                else
                {
                    return autoNeuroSymptom_Concat;
                }
            }
            set
            {
                autoNeuroSympComment = string.Join(",", value);
            }
        }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}
