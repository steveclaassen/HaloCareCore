using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class ProgramStatuses
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Program code")]
        public string programCode { get; set; }

        [DisplayName("Status code")]
        public string statusCode { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}