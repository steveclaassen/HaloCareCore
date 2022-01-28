using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;

namespace HaloCareCore.Models
{
    public class AssignmentsListView
    {
        public List<OpenAssignments> openAssignments { get; set; }
        public List<OpenAssignments> inProgressAssignments { get; set; }
        public List<OpenAssignments> closedAssignments { get; set; }
        public List<Queries> queries { get; set; }

        public Member member { get; set; }
        public Dependant dependant { get; set; }

        public int pageRowCount { get; set; }
        public int opencount { get; set; }
        public int inprogresscount { get; set; }
        public int closedcount { get; set; }


    }
}