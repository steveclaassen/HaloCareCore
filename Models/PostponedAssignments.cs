using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class PostponedAssignments
    {
        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string dependantCode { get; set; }

        [DisplayName("Patient name")]
        public string patientName { get; set; }

        [DisplayName("Scheme")]
        public string medicalScheme { get; set; }

        [DisplayName("Patient status")]
        public string patientStatus { get; set; }

        [DisplayName("Assignment ID")]
        public int assignmentID { get; set; }

        [DisplayName("Days overdue")]
        public string daysOverdue { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? assignmentEffective { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}