using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class DoctorTypes
    {
        [Key]
        [Required]
        [DisplayName("Doctor Type")]
        public string doctorType { get; set; }

        [Required]
        [DisplayName("Type Description")]
        public string typeDescription { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}