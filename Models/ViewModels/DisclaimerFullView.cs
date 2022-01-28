using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class DisclaimerFullView
    {
        public Guid DependantID { get; set; }
        public int Id { get; set; }

        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }

        [DisplayName("Question number")]
        public string QuestionNumber { get; set; }
        
        [DisplayName("English")]
        public string EnglishQuestion { get; set; }
        
        [DisplayName("English")]
        public string AfrikaansQuestion { get; set; }
        
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

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}
