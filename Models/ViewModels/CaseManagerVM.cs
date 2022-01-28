using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class CaseManagerVM
    {
        public UserMemberHistory UserMemberHistory { get; set; }
        public List<UserMemberHistory> UserMemberHistories { get; set; }
    }
}
