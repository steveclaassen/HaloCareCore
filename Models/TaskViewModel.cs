using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class TaskViewModel
    {
        public int id { get; set; }

        [DisplayName("Assignment item ID")]
        public int assignmentitemID { get; set; }

        [DisplayName("Requirement ID")]
        public int RequirementID { get; set; }

        [DisplayName("Requirement")]
        public string requirement { get; set; }

        [DisplayName("Closed by")]
        public string closedby { get; set; }

        [DisplayName("Template ID")]
        public string templateID { get; set; }

        [DisplayName("Closed")]
        public bool closed { get; set; }

        [DisplayName("Task type")]
        public string taskType { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Task sequence")]
        public int taskSequence { get; set; }

        [DisplayName("Item ID")]
        public string itemID { get; set; } //HCare-1192
    }
}