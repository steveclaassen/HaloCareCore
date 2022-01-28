using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class MemberProfileVM
    {
        public MemberBasicViewModel MemberBasicView { get; set; }
        public List<MemberBasicViewModel> MemberBasicViews { get; set; }

        public PatientProgramHistory PatientProgramHistory { get; set; }
        public List<PatientProgramHistory> PatientProgramHistories { get; set; }

    }
}
