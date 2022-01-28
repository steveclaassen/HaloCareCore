using HaloCareCore.XmlModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace HaloCareCore.Management
{
    public class MemberValidationService
    {
        public static memberDetailsResponse POSTREQ(string url, string jsonContent)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";
            request.Headers.Add("apikey", "l7360ccd017d27435ea0f8c1d2fd88d5e1");

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        var ress = reader.ReadToEnd();
                        memberDetailsResponse jsonResponse = (memberDetailsResponse)JsonConvert.DeserializeObject(ress, typeof(memberDetailsResponse));

                        return jsonResponse;
                    }
                }
            }
            catch (WebException ex)
            {
                var except = ex.Message;
                return null;
            }
        }
    }
}