using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class AssignmentItemTypes
    {
        [Key]
        [Required]
        [DisplayName("Code")]
        public string assignmentItemType { get; set; }

        [Required]
        [DisplayName("Description")]
        public string itemDescription { get; set; }

        [DisplayName("Type")]
        public string sourceType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Capture")]
        public bool capture { get; set; }

        [DisplayName("Attach")]
        public bool attach { get; set; }

        [DisplayName("Requirements")]
        public string requirements { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        //Hcare-995 --start
        [DisplayName("Diabetes")]
        public bool? diabetes { get; set; }

        [DisplayName("HIV")]
        public bool? hiv { get; set; }

        [DisplayName("Oncology")]
        public bool? oncology { get; set; }

        [DisplayName("Mental Health")]
        public bool? mental { get; set; }

        [DisplayName("Hypertension")]
        public bool? hypertension { get; set; }
        [DisplayName("TB")]
        public bool? TB { get; set; }
        //--end
    }
}