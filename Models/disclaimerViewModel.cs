using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class disclaimerViewModel
    {
        public PatientQuestionnaireResponse disclaimerResult { get; set; }
        public List<PatientQuestionnaireResponse> disclaimerResults { get; set; }

        public QuestionnaireTemplate questionnaireTemplate { get; set; }
        public List<QuestionnaireTemplate> questionnaireTemplates { get; set; }

        public Guid DependantId { get; set; }

        public int QuestionnaireTemplateID { get; set; }

        public int TemplateId { get; set; }

        public int TaskId { get; set; }

        public string TemplateName { get; set; }

        [DisplayName("Question No")]
        public int questionNo { get; set; }

        [DisplayName("Question")]
        public string Question { get; set; }

        [DisplayName("Answer")]
        public bool Answer { get; set; }

        [DisplayName("Confirmed")]
        public bool Confirmed { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("yesNo")]
        public bool yesNo { get; set; }



    }
}
