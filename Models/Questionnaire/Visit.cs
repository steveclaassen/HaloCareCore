using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class Visit
    {
        [Key]
        public int visitID { set; get; }


        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        [DisplayName("Follow-up visits with Doctor")]
        public string FollowUpDoctor { get; set; }

        [DisplayName("Follow-up visit with Dis-Chem/clinic sister")]
        public string FollowUpClinic { get; set; }

        [DisplayName("Follow-up date with HaloCare")]
        public string FollowUpHaloCare { get; set; }

        [DisplayName("Other service provider benefits")]
        public string OtherService { get; set; }
        
        public bool assignementSent { set; get; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }
        public Guid programId { get; set; }

        [DisplayName("Follow-up visits with Psychologist")]
        public string FollowUpPsychologist { get; set; }

        [DisplayName("Follow-up visits with Psychiatrist")]
        public string FollowUpPsychiatrist { get; set; }


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
    }

}
