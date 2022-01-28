using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AssignmentsView
    {
        public List<AssignmentView> newAssignments { get; set; }
        public List<AssignmentView> activeAssignments { get; set; }
        public List<AssignmentView> closedAssignments { get; set; }
        public Assignments newAssignment { get; set; }

        public List<AssignmentItems> assignmentItems { get; set; }

        [DisplayName("Item ID")]
        public int itemID { get; set; }

        [DisplayName("Assignment item")]
        public string assignmentItemType { get; set; }

        [DisplayName("Task type")]
        public string tasktype { get; set; }

        [DisplayName("Task requirement")]
        public int taskRequirement { get; set; }


        //HCare-996
        public bool CanCreate { set; get; }
        public bool CanEdit { set; get; }
        public bool CanRead { set; get; }
    }
}