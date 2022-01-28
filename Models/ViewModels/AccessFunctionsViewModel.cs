using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AccessFunctionsViewModel
    {
        public bool canRead { set; get; }

        public bool canEdit { set; get; }

        public bool canCreate { set; get; }
    }
}