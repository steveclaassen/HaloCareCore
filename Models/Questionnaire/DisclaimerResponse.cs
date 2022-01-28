using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class DisclaimerResponse
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }
        public int? AssignmentID { get; set; }
        public int? TaskID { get; set; }

        [DisplayName("Question")]
        public string Question { get; set; }

        public bool Yes { get; set; }
        public bool No { get; set; }
        
        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        public string Program { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}
