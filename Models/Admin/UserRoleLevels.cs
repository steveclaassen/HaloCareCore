using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Admin
{
    public class UserRoleLevels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid roleID { get; set; }
        public bool admin { get; set; }

    }
}