using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ProfileLock
    {
        [Key]
        [Required]
        public Guid UserId { set; get; }
        public Guid MemberUserId { set; get; }
        public DateTime CreatedDate { set; get; }
       
    }
}