using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class Hypoglycaemia
    {
        public int HypoglycaemiaID { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }


        [DisplayName("Glucose < 4.1mol")]
        public string glucoseReading { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool hypoglycaemiaSymptomsCheck { get; set; }

        [DisplayName("Yes/No")]
        public bool hypoglycaemiaCheck { get; set; }

        [DisplayName("Sleep patterns")]
        public string hypoglycaemiaComment { get; set; }

        [DisplayName("Symptoms")]
        public string hypoglycaemiaSymptomsComment { get; set; }

        [DisplayName("Hypoglycaemic assistance")]
        public string hypoglycaemiaAssistance { get; set; }

        [DisplayName("Emergency room visits")]
        public string emergencyRoomVisits { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool insulinUseCheck { get; set; }

        [DisplayName("Insulin Comment")]
        public string insulinUseComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool sulfonylureaUseCheck { get; set; }

        [DisplayName("Insulin Comment")]
        public string sulfonylureaUseComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool ckdStageCheck { get; set; }

        [DisplayName("End-Stage kidney disease")]
        public string ckdStageComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool patientAgeCheck { get; set; }

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

        //listBox concatination

        public string[] hypoglycaemiaSymptoms_Concat
        {
            get
            {
                if (hypoglycaemiaSymptomsComment != null)
                {
                    return hypoglycaemiaSymptomsComment.Split(',');
                }
                else
                {
                    return hypoglycaemiaSymptoms_Concat;
                }
            }
            set
            {
                hypoglycaemiaSymptomsComment = string.Join(",", value);
            }
        }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}
