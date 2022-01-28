using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class WorkflowViewModel
    {
        public int Id { get; set; }
        
        public Guid? roleId { get; set; }
        
        public string assignmentTypeId { get; set; }
        
        [DisplayName("Description")]
        public string description { get; set; }
        
        [DisplayName("Selected")]
        public bool selected { get; set; }
        public bool active { get; set; } //HCare-1010

        //HCare-995 start
        [DisplayName("Diabetes")]
        public bool diabetes { get; set; }
        [DisplayName("HIV")]
        public bool hiv { get; set; }
        [DisplayName("Hypertension")]
        public bool hypertension { get; set; }
        [DisplayName("Oncology")]
        public bool oncology { get; set; }
        [DisplayName("Mental health")]
        public bool mental { get; set; }
        //end
    }
}