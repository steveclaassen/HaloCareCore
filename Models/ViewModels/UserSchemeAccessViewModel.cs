using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserSchemeAccessViewModel
    {
        public Guid Id { get; set; }
        public Guid userId { get; set; }
        
        [DisplayName("Medical scheme ID")]
        public Guid medicalAidId { get; set; }
        
        [DisplayName("Medical scheme")]
        public string medicalAidName { get; set; }
        
        [DisplayName("Access")]
        public bool hasAccess { get; set; }
    }
}