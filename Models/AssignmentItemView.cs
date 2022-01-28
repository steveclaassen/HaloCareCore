using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AssignmentItemView
    {
        [DisplayName("Assignment ID")]
        public string assignmentID { get; set; }

        [DisplayName("Assignment item ID")]
        public string assignmentItemId { get; set; }

        [DisplayName("Item name")]
        public string itemDocName { get; set; }

        [DisplayName("Item captured ID")]
        public int itemCapturedId { get; set; }

        [DisplayName("Item type")]
        public string itemTypeName { get; set; }

        [DisplayName("Assignment item type")]
        public string assignmentItemType { get; set; }

        [DisplayName("Requirement")]
        public string requirement { get; set;}

        [DisplayName("Follow up")]
        public bool followup { get; set; }

        [DisplayName("Followed up")]
        public bool folloedwup { get; set; }

        [DisplayName("Capture")]
        public bool capture { get; set; }

        [DisplayName("Captured")]
        public bool captured { get; set; }

        [DisplayName("Attach")]
        public bool attach { get; set; }

        [DisplayName("Attached")]
        public bool attached { get; set; }

        [DisplayName("Log ID")]
        public int logid { get; set; }
    }
}