using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserOptionsViewModel
    {
        public Guid Id { get; set; }
        public Guid medicalAidID { get; set; }
        public Guid programId { get; set; }

        [DisplayName("Plan")]
        public string MmedicalAidName { get; set; }

        [DisplayName("Program")]
        public string programName { get; set; }

        [DisplayName("Include")]
        public bool include { get; set; }

    }
}