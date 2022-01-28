using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class PatientQuestionnaireResponse
    {
        [Key]
        [Required]
        public int PatientQuestionnaireResponseID { get; set; }

        public Guid DependantId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public int TaskId { get; set; }

        [DisplayName("Confirmed")]
        public bool Confirmed{ get; set; }

        [DisplayName("Answer")]
        public bool Answer{ get; set; }

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

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}