using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Management
{
    public class PatientDiagnosis //hcre-1312
    {
        [Key]
        public int Id { get; set; }

        public Guid DependantID { get; set; }
        public Guid? QuestionnaireID { get; set; }


        [DisplayName("Code")]
        public string ProgramCode { get; set; }
        
        [DisplayName("Description")]
        public string ProgramDescription { get; set; }

        [DisplayName("ICD-10 code")]
        public string ICD10Code { get; set; }

        [DisplayName("Mapping code")]
        public string ConditionCode { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Follow up")]
        public bool FollowUp { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Medication")]
        public string Medication { get; set; } //hcare-863
    }
}
