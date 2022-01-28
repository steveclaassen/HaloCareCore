using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class UserRoleVM
    {
        public List<Users> Users { get; set; }
        public List<Roles> Roles { get; set; }
    }
}
