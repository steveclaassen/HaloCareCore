using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class Roles
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [DisplayName("Name")]
        public string name { get; set; }

        [DisplayName("Admin")]
        public bool admin { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Settings")]
        public bool adminRights { get; set; }
    }
}