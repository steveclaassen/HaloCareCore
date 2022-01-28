using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Script
{
    public class ScriptReferences
    {
        [Key]
        public int id { get; set; }

        public Guid dependantId { get; set; }

        [DisplayName("Reference #")]
        public int reference { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Attachment")]
        public string attachment { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public bool active { get; set; }
    }
}