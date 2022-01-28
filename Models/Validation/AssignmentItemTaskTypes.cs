using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class AssignmentItemTaskTypes
    {
        [Key]
        [Required]
        [DisplayName("Task ID")]
        public string taskId { get; set; }

        [Required]
        [DisplayName("Task type")]
        public string taskDescription { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}