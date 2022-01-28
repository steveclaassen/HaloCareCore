using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class ProfileHistory
    {
        public Guid Id { get; set; }
        public Guid ProfileStatusId { get; set; }
        public Guid MedicalAidId { get; set; }
        public Guid OptionId { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool SchemeValidity { get; set; }
        public string ChangeReason { get; set; }
        public string Origin { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}