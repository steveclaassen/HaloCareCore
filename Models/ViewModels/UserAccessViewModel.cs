using HaloCareCore.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserAccessViewModel
    {
        public Guid userId { get; set; }
        public List<Guid> rights { get; set; }
        public List<UserProgramAccess> accessList { get; set; }
    }
}