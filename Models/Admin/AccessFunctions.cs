using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaloCareCore.Models.Admin
{
    public class AccessFunctions
    {
        [Key]
        public int Id { set; get; }
        public string function { set; get; }

        public bool canRead { set; get; }
   
        public bool canEdit { set; get; }

        public bool canCreate { set; get; }

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
        public Guid RoleId { set; get; }
        [NotMapped]
        public List<bool> canReads { set; get; }
        [NotMapped]
        public List<bool> canEdits { set; get; }
        [NotMapped]
        public List<bool> canCreates { set; get; }
    }
}