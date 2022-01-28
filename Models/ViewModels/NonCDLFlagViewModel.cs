using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class NonCDLFlagViewModel
    {
        public IEnumerable<Programs> programs { set; get; }
        public NonCLDFlags nonCLDFlags { set; get; }

    }
}