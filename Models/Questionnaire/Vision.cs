using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class Vision
    {
        [DisplayName("Vision ID")]
        public int VisionID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }


        [DisplayName("Vision")]
        public string vision { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool visionCheck { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool eyeTreatmentCheck { get; set; }

        [DisplayName("Treatments")]
        public string eyeTreatment { get; set; }

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

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        //listBox concatination
        public string[] vision_Concat
        {
            get
            {
                if (vision != null)
                {
                    return vision.Split(',');
                }
                else
                {
                    return vision_Concat;
                }
            }
            set
            {
                vision = string.Join(",", value);
            }
        }

        //listBox concatination
        public string[] eyeTreat_Concat
        {
            get
            {
                if (eyeTreatment != null)
                {
                    return eyeTreatment.Split(',');
                }
                else
                {
                    return eyeTreat_Concat;
                }
            }
            set
            {
                eyeTreatment = string.Join(",", value);
            }
        }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }



    }
}
