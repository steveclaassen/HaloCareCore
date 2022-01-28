using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class MH_SchizophreniaResponse
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }

        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }

        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Depression")]
        public string Depression { get; set; }
        
        [DisplayName("Score")]
        public int DepressionSC { get; set; }

        [DisplayName("Hopelessness")]
        public string Hopelessness { get; set; }
        
        [DisplayName("Score")]
        public int HopelessnessSC { get; set; }

        [DisplayName("Self depreciation")]
        public string SelfDepreciation { get; set; }
        
        [DisplayName("Score")]
        public int SelfDepreciationSC { get; set; }

        [DisplayName("Guilty ideas")]
        public string GuiltyIdeas { get; set; }
        
        [DisplayName("Score")]
        public int GuiltyIdeasSC { get; set; }

        [DisplayName("Pathological guilt")]
        public string PathologicalGuilt { get; set; }

        [DisplayName("Score")]
        public int PathologicalGuiltSC { get; set; }

        [DisplayName("Morning depression")]
        public string MorningDepression { get; set; }

        [DisplayName("Score")]
        public int MorningDepressionSC { get; set; }

        [DisplayName("Early wakening")]
        public string EarlyWakening { get; set; }
        
        [DisplayName("Score")]
        public int EarlyWakeningSC { get; set; }

        [DisplayName("Suicidal")]
        public string Suicidal { get; set; }

        [DisplayName("Score")]
        public int SuicidalSC { get; set; }

        [DisplayName("Suicidal comment")]
        public string SuicidalCMT { get; set; }

        [DisplayName("Observed depression")]
        public string ObservedDepression { get; set; }

        [DisplayName("Score")]
        public int ObservedDepressionSC { get; set; }

        [DisplayName("Outcome")]
        public string Outcome { get; set; }
        
        [DisplayName("Total score")]
        public int TotalScore { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Follow up")]
        public bool FollowUp { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public int? RiskID { get; set; } //HCare-1151


    }
}
