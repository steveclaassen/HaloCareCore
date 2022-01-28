using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class EmailTemplateViewModel
    {
        public Guid id { get; set; }
        public Guid accountId { get; set; }
        public Guid programId { get; set; }
        public Guid medicalAidId { get; set; }
        public Guid planId { get; set; }
        public bool Added { get; set; }
        public int templateId { get; set; }

        [DisplayName("Subject")]
        public string subject { get; set; }

        [DisplayName("Body")]
        public string body { get; set; }


        [DisplayName("Title")]
        public string title { get; set; }
    }
}