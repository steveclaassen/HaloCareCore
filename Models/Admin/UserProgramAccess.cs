using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Admin
{
    public class UserProgramAccess
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public Guid userId { get; set; }
        public Guid programId { get; set; }
        public Guid medicalAidId { get; set; }
        [Required]
        public string createdBy { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }
        public string modifiedBy { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }
        [Required]
        public bool Active { get; set; }
    }
}