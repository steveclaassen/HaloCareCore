using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserProgramViewModal
    {
        public Guid Id { get; set; }
        public Guid userId { get; set; }
        [DisplayName("Program ID")]
        public Guid programId { get; set; }
        [DisplayName("Medical scheme ID")]
        public Guid medicalAidId { get; set; }
        [DisplayName("Program name")]
        public string programName { get; set; }
        [DisplayName("Has access")]
        public bool hasAccess { get; set; }
    }
}