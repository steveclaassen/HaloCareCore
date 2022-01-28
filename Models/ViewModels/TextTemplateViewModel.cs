using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class TextTemplateViewModel
    {
        public Guid id { get; set; }
        public Guid accountId { get; set; }
        public Guid programId { get; set; }
        public Guid medicalAidId { get; set; }
        public Guid planId { get; set; }
        public bool Added { get; set; }
        public int templateId { get; set; }

        [DisplayName("Title")]
        public string templateName { get; set; }

        [DisplayName("Text Message")]
        public string textMessage { get; set; }

    }
}