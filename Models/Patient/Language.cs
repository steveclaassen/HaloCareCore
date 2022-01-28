using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class Language
    {
        [Key]
        [Required]
        [DisplayName("Language")]
        public string languageName { get; set; }

        public string languageCode { get; set; }

        public string iSeriesLanguageCode { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}