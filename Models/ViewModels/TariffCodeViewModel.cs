using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class TariffCodeViewModel
    {
        public TariffCode TariffCode {set;get;}
        public IEnumerable<Programs> Programs { get; set; }
        public IEnumerable<AssignmentItemTypes> AssignmentItemTypes { get; set; }

        public IEnumerable<PathologyFields> PathologyFields { get; set; }


    }
}