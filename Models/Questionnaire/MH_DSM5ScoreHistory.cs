using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Questionnaire
{
    public class MH_DSM5ScoreHistory
    {
        [Key]
        public int Id { get; set; }

        public Guid DependantID { get; set; }

        [DisplayName("Previous Score")]
        public int OldScore { get; set; }
        
        [DisplayName("New Score")]
        public int NewScore { get; set; }

        [DisplayName("Old Reason")]
        public string OldReason { get; set; }

        [DisplayName("New Reason")]
        public string NewReason { get; set; }

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

        [DisplayName("Active")]
        public bool Active { get; set; }
        
        public int? RiskID { get; set; }
    }
}