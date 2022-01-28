using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class Podiatry
    {
        [DisplayName("Podiatry ID")]
        public int PodiatryID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }


        [DisplayName("Amputation comment")]
        public string amputationComment { get; set; }

        [DisplayName("Amputation reason")]
        public string amputationReason { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool amputationCheck { get; set; }
        
        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryLopsCheck { get; set; }

        [DisplayName("LOPS comment")]
        public string podiatryLopsComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryDeformityCheck { get; set; }

        [DisplayName("Deformation comment")]
        public string podiatryDeformityComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryPerArterialDiseaseCheck { get; set; }

        [DisplayName("Affected leg")]
        public string podiatryPerArterialDiseaseAffectedLeg { get; set; }

        [DisplayName("Peripheral Arterial Disease")]
        public string podiatryPerArterialDiseaseComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryWoundCheck { get; set; }

        [DisplayName("Affected leg")]
        public string podiatryWoundAffectedLeg { get; set; }

        [DisplayName("Podiatry Wound Comment")]
        public string podiatryWoundComment { get; set; }

        [DisplayName("Wound infection")]
        public string WoundInfection { get; set; }

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

        public string[] amputaionComment_Concat
        {
            get
            {
                if (amputationComment != null)
                {
                    return amputationComment.Split(',');
                }
                else
                {
                    return amputaionComment_Concat;
                }
            }
            set
            {
                amputationComment = string.Join(",", value);
            }
        }
        
        public string[] amputaionReason_Concat
        {
            get
            {
                if (amputationReason != null)
                {
                    return amputationReason.Split(',');
                }
                else
                {
                    return amputaionReason_Concat;
                }
            }
            set
            {
                amputationReason = string.Join(",", value);
            }
        }
        
        public string[] podiatryLops_Concat
        {
            get
            {
                if (podiatryLopsComment != null)
                {
                    return podiatryLopsComment.Split(',');
                }
                else
                {
                    return podiatryLops_Concat;
                }
            }
            set
            {
                podiatryLopsComment = string.Join(",", value);
            }
        }
        
        public string[] podiatryDeformity_Concat
        {
            get
            {
                if (podiatryDeformityComment != null)
                {
                    return podiatryDeformityComment.Split(',');
                }
                else
                {
                    return podiatryDeformity_Concat;
                }
            }
            set
            {
                podiatryDeformityComment = string.Join(",", value);
            }
        }
        
        public string[] PerArterialDisease_Concat
        {
            get
            {
                if (podiatryPerArterialDiseaseComment != null)
                {
                    return podiatryPerArterialDiseaseComment.Split(',');
                }
                else
                {
                    return PerArterialDisease_Concat;
                }
            }
            set
            {
                podiatryPerArterialDiseaseComment = string.Join(",", value);
            }
        }
        
        public string[] podiatryWound_Concat
        {
            get
            {
                if (podiatryWoundComment != null)
                {
                    return podiatryWoundComment.Split(',');
                }
                else
                {
                    return podiatryWound_Concat;
                }
            }
            set
            {
                podiatryWoundComment = string.Join(",", value);
            }
        }


        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }


    }
}
