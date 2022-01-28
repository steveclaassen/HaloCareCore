using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ScriptTypes
    {
        [Key]
        [Required]
        [DisplayName("Type")]
        public string type { get; set; }

        [Required]
        [DisplayName("Type name")]
        public string typeName { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}