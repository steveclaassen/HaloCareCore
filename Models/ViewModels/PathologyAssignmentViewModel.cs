using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class PathologyAssignmentViewModel
    {
        public List<AssignmentItemTypes> assignmentItemTypes { get; set; }
        public List<Programs> programs { get; set; }
        public List<PathologyFields> PathologyFields { set; get; }
        public PathologyAssignments PathologyAssignments { get; set; }



        public int PathologyAssignmentId { get; set; }
   
        [DisplayName("Program")]
        public string ProgramCode { get; set; }


        [DisplayName("Pathology")]
        public string PathologyField { get; set; }

        [DisplayName("Assignment item type")]
        public string AssignmentItemType { get; set; }
       
        [DisplayName("Due date (Days)")]
        public int PathologyDueDate { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}