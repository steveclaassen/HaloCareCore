using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Patient
{
    public class HC_Member_Claims
    {
        [Key]
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Sequence { get; set; }
        public string Root { get; set; }
        public string FileName { get; set; }
        public string MedicalAidName { get; set; }
        public string MedicalAidCode { get; set; }
        public string CarrierCode { get; set; }
        public string Records { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool Archived { get; set; }
    }
}
