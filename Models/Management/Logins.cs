using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class Logins
    {
        [Key]
        public Guid UserId { get; set; }
        public string SessionId { get; set; }
        public bool LoggedIn { get; set; }
    }
}