using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class CompactAssignmentsListView
    {
        public List<CompactOpenAssignments> openAssignments { get; set; }
        public List<CompactOpenAssignments> inProgressAssignments { get; set; }
        public List<CompactOpenAssignments> closedAssignments { get; set; }

        public List<EnquiryFullViewModel> enquiryList { get; set; }

        public int opencount { get; set; }
        public int closedcount { get; set; }

    }
}