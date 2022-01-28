using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class NewBorn
    {
        [Key]
        public int Id { get; set; }

        public Guid DependantID { get; set; }

        public Guid? QuestionnaireID { get; set; }

        public bool hasConcern { get; set; }
        [DisplayName("Concern")]
        public string Concern { get; set; }

        public bool isBreastfeeding { get; set; }
        [DisplayName("Breast feeding")]
        public string Breastfeeding { get; set; }

        public bool isOnMedication { get; set; }
        [DisplayName("Medication")]
        public string Medication { get; set; }

        public bool hivTest { get; set; }
        [DisplayName("HIV test")]
        public string HIVTestComment { get; set; }

        public bool hivResults { get; set; }
        [DisplayName("HIV result")]
        public string HIVResultsComment { get; set; }

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

        [DisplayName("Comment")]
        public string GeneralComments { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Follow up")]
        public bool FollowUp { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }



    }
}
