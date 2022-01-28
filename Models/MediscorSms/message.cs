using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.MediscorSms
{
    public class Message
    {
        [JsonProperty("cellNumber")]
        public string CellNumber { get; set; }

        [JsonProperty("smsMessage")]
        public string SmsMessage { get; set; }
    }
}