using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public  class CommunicationLogVM
    {
        public Guid dependentID { set; get; }
        public  string Scheme { get; set; }
        public  string MemberNumber { get; set; }
        public string ProfileNumber { get; set; }
        public string DepandantCode { set; get; }

        public string TypeOfCommunication { get; set; }
        public string Detail { get; set; }
        public DateTime DateSent { get; set; }
        public string description { set; get; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime dateFrom { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime dateTo { get; set; }
        public string CommunicationSentTo { set; get; }

        public List<CommunicationLogVM> communicationLogVMs { set; get; }
       

  

    }
}