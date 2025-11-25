using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Runtime.Serialization.Json;
using Essensys.Common.SMS;
using System.Runtime.Serialization;

namespace Essensys.Common
{
    /// <summary>
    /// Service d'envoi de SMS
    /// </summary>
    public static class SMSSender
    {
        public static string SendMail(string email, string message)
        {
            LogManager.LogTrace("Send mail to " + email, null);
            EMailSender.SendSimpleEMail(ConfigurationManager.AppSettings["mailuser"], email, message, message, message);
            return "ok";
        }
        public static string Send(string localphonenumber, string countrycode, string message)
        {
            // Envoi à Octopush
            string phone = "+" + countrycode + localphonenumber.Replace(" ", "").Substring(1, 9);
            string query = String.Format("user_login={0}", ConfigurationManager.AppSettings["OCTOPUSH_LOGIN"]);
            query += String.Format("&api_key={0}", ConfigurationManager.AppSettings["OCTOPUSH_APIKEY"]);
            query += String.Format("&sms_recipients={0}", phone);
            query += String.Format("&sms_text={0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " - " + message);
            query += String.Format("&sms_sender={0}", "ESSENSYS");
            query += String.Format("&sms_type={0}", "FR");
            query += String.Format("&transactional={0}", "1");
            query += String.Format("&request_mode={0}", ConfigurationManager.AppSettings["OCTOPUSH_MODE"]);
            LogManager.LogTrace("QUERY=" + query, null);
            Uri uri = new Uri("http://www.octopush-dm.com/api/sms?" + query);

            MemoryStream ms = new MemoryStream();

            // Submit Request
            using (WebClient w = new WebClient())
            {
                try
                {
                    w.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    byte[] response = w.UploadData(uri, ms.GetBuffer());
                    string res = Encoding.UTF8.GetString(response);
                    Byte[] bytes = Encoding.UTF8.GetBytes(res);
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    DataContractSerializer dataContractSerializer =
                        new DataContractSerializer(typeof(OctopushResult));
                    OctopushResult sms =
                   dataContractSerializer.ReadObject(memoryStream) as OctopushResult;
                    memoryStream.Close();

                    if (sms.ErrorCode == "000")
                    {
                        return "ok";
                    }
                    else
                    {
                        LogManager.LogError("Erreur dans la méthode SMS " + sms.ErrorCode, null);
                        return "ko";
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)wex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                LogManager.LogError("Erreur dans la méthode SMS : " + error, wex);
                            }
                        }
                    }
                    return "ko500";
                }
            }
        }
    }
}
