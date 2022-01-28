using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class StatusTypes
    {
        [Key]
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string Active { get; set; }
    }
}