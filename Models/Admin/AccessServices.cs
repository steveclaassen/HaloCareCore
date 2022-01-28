using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Admin
{
    public class AccessServices
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string name { get; set; }

        [Required]
        [DisplayName("Description")]
        public string description { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Created date")]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}