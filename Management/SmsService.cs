using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace HaloCareCore.Management
{
    public class SmsService
    {
        /* how an sms is sent
         * string response = PostMessage(text8, text);
		   string tnxres = response.Replace("ID:", "");
		   tnxcode = tnxres.Replace(" ", "");
		   repository.UpdateMessageQueueHistoryAdo(message_No, "Sent", tnxcode, tnxres);
         */
        private readonly IConfiguration Configuration;
        public string PostMessage(string To, string Message)
        {
            To = To.Replace(" ", "");
            string text = To.Substring(2, 2);
            string result;
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("user-agent", "Mozill/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                webClient.QueryString.Add("user", "optipharm");
                webClient.QueryString.Add("password", "optipharm1");
                webClient.QueryString.Add("api_id", "811695");
                webClient.QueryString.Add("to", To);
                webClient.QueryString.Add("text", Message.ToString().Trim());
                webClient.QueryString.Add("mo", "1");
                int length = Message.Length;
                if (length > 160)
                {
                    decimal num = Math.Ceiling(decimal.Divide(length, 160m));
                    decimal d = num * 10m;
                    decimal num2;
                    if (length % 160 < d)
                    {
                        num2 = ++num;
                    }
                    else
                    {
                        num2 = num;
                    }
                    webClient.QueryString.Add("concat", num2.ToString());
                }
                webClient.QueryString.Add("from", "10002");
                webClient.QueryString.Add("callback", "3");
                Stream stream = webClient.OpenRead(Configuration.GetSection("EmailSettings")["smsAddress"].ToString());
                StreamReader streamReader = new StreamReader(stream);
                string text2 = streamReader.ReadToEnd();
                stream.Close();
                streamReader.Close();
                result = text2;
            }
            catch (Exception ex)
            {
                result = ex.StackTrace;
            }
            return result;
        }

        public string PostMessageViaMediscor(string schemeCode, string schemeOption, string membershipNumber, string dependantNumber, string cellNumber, string smsMessage)
        {
            cellNumber = cellNumber.Replace(" ", "");
            string text = cellNumber.Substring(2, 2);
            string result;
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("user-agent", "Mozill/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                webClient.QueryString.Add("schemeCode", schemeCode);
                webClient.QueryString.Add("schemeOption", schemeOption);
                webClient.QueryString.Add("membershipNumber", membershipNumber);
                webClient.QueryString.Add("cellNumber", cellNumber);
                webClient.QueryString.Add("smsMessage", smsMessage.ToString().Trim());
                int length = smsMessage.Length;
                if (length > 160)
                {
                    decimal num = Math.Ceiling(decimal.Divide(length, 160m));
                    decimal d = num * 10m;
                    decimal num2;
                    if (length % 160 < d)
                    {
                        num2 = ++num;
                    }
                    else
                    {
                        num2 = num;
                    }
                    webClient.QueryString.Add("concat", num2.ToString());
                }
                webClient.QueryString.Add("from", "10002");
                webClient.QueryString.Add("callback", "3");
                Stream stream = webClient.OpenRead(Configuration.GetSection("EmailSettings")["smsAddress"].ToString());
                StreamReader streamReader = new StreamReader(stream);
                string text2 = streamReader.ReadToEnd();
                stream.Close();
                streamReader.Close();
                result = text2;
            }
            catch (Exception ex)
            {
                result = ex.StackTrace;
            }
            return result;
        }

        string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

        void POST(string url, string jsonContent)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    length = response.ContentLength;
                }
            }
            catch (WebException ex)
            {
                var except = ex.Message;
            }
        }
    }
}