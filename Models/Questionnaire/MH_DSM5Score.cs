using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class MH_DSM5Score
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }
        
        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }
        
        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Score")]
        public int Score { get; set; }

        [DisplayName("Reason")]
        public string Reason { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EffectiveDate { get; set; }

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

        [DisplayName("Active")]
        public bool Active { get; set; }

        public int? RiskID { get; set; }

        public List<MH_DSM5ScoreHistory> MH_DSM5ScoreHistories { get; set; }
    }
}
