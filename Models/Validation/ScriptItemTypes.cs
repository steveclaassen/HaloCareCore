using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ScriptItemTypes
    {
        [Key]
        [Required]
        [DisplayName("Item type")]
        public string itemType { get; set; }

        [Required]
        [DisplayName("Item")]
        public string itemTypeName { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}