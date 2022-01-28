using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Script
{
    public class Authorisations
    {
        [Key]
        [Required]
        public int authID { get; set; }

        [Required]
        public Guid dependantID { get; set; }

        [Required]
        [DisplayName("Client #")]
        public int clientNo { get; set; }

        [Required]
        [DisplayName("Script ID")]
        public int scriptID { get; set; }

        [DisplayName("Request")]
        public string request { get; set; }

        [DisplayName("Response")]
        public string response { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Source")]
        public string source { get; set; }

        [DisplayName("Entry type")]
        public string entryType { get; set; }

        [DisplayName("Auth date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime authDate { get; set; }
    }
}