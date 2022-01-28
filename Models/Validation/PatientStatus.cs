using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class PatientStatus
    {
        [Key]
        public string Code { get; set; }
        public string Name { get; set; }
        public bool active { get; set; }
    }
}