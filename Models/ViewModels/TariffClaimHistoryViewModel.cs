using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class TariffClaimHistoryViewModel
    {
        public TariffClaimHistory TariffClaimHistory { set; get; }
        public List<TariffClaimHistory> tariffClaimHistories { set; get; }
    }
}