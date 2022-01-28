using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Admin
{
    public class UserRoleRights
    {
        public int id { get; set; }
        public Guid roleId { get; set; }
        public int accessId { get; set; }
        public bool active { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
    }
}