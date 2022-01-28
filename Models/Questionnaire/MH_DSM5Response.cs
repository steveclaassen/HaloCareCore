using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class MH_DSM5Response
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }
        
        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }
        
        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Substance abuse")]
        public bool SubstanceAbuse { get; set; }

        [DisplayName("Depression")]
        public bool Depression { get; set; }
        [DisplayName("Score")]
        public int DepressionSC { get; set; }

        [DisplayName("Interest")]
        public bool Interest { get; set; }
        [DisplayName("Score")]
        public int InterestSC { get; set; }

        [DisplayName("Weight-loss")]
        public bool WeightLoss { get; set; }
        [DisplayName("Score")]
        public int WeightLossSC { get; set; }

        [DisplayName("Psychomotor")]
        public bool Psychomotor { get; set; }
        [DisplayName("Score")]
        public int PsychomotorSC { get; set; }

        [DisplayName("Tiredness")]
        public bool Tiredness { get; set; }
        [DisplayName("Score")]
        public int TirednessSC { get; set; }

        [DisplayName("Self-worth")]
        public bool SelfWorth { get; set; }
        [DisplayName("Score")]
        public int SelfWorthSC { get; set; }

        [DisplayName("Concentration")]
        public bool Concentration { get; set; }
        [DisplayName("Score")]
        public int ConcentrationSC { get; set; }

        [DisplayName("Suicidal")]
        public bool Suicidal { get; set; }
        [DisplayName("Score")]
        public int SuicidalSC { get; set; }

        public string Outcome { get; set; }
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
