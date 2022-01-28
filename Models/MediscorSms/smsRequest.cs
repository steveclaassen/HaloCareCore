using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.MediscorSms
{
    public class smsRequest
    {
        [JsonProperty("schemeCode")]
        public string SchemeCode { get; set; }

        [JsonProperty("schemeOption")]
        public string SchemeOption { get; set; }

        [JsonProperty("membershipNumber")]
        public string MembershipNumber { get; set; }

        [JsonProperty("dependantNumber")]
        public string DependantNumber { get; set; }

        [JsonProperty("message")]
        public Message message { get; set; }
    }
}