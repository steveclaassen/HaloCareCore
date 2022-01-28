using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class Priorities
    {
        [Key]
        [Required]
        [DisplayName("Priority type")]
        public string prioritytype { get; set; }

        [Required]
        [DisplayName("Priority")]
        public string priorityName { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}