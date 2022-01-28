using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class UserRole
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid userId { get; set; }
        [Required]
        public Guid roleId { get; set; }
    }
}