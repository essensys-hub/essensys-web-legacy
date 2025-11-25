using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Essensys.Common
{
    /// <summary>
    /// Classe d'envoi des emails
    /// </summary>
    public static class EMailSender
    {
        /// <summary>
        /// Préparation d'un email
        /// </summary>
        /// <param name="to">Destinataire</param>
        /// <param name="subject">Sujet</param>
        /// <param name="message">Message</param>
        /// <param name="htmlmessage">Message HTML</param>
        /// <returns>Message Mail</returns>
        private static MailMessage PrepareEMail(string to, string subject, string message, string htmlmessage)
        {
            // Paramétrage du message
            MailAddress replyto = new MailAddress(ConfigurationManager.AppSettings["mailreplyto"]);
            MailMessage Message = new MailMessage();

            Message.From = new MailAddress(ConfigurationManager.AppSettings["mailuser"], ConfigurationManager.AppSettings["mailuser"]);
            Message.Subject = subject;
            foreach (string mto in to.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                Message.To.Add(new MailAddress(mto.Trim(), mto.Trim()));
            }
            Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            Message.Sender = new MailAddress(ConfigurationManager.AppSettings["mailreplyto"]);

            // Corps du message
            AlternateView plainView = AlternateView.CreateAlternateViewFromString(message, null, "text/plain");
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(GenerateHTMLTemplate(htmlmessage), null, "text/html");
            Message.AlternateViews.Add(plainView);
            Message.AlternateViews.Add(htmlView);

            return Message;
        }


        public static string GenerateHTMLTemplate(string htmlmessage)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/Content/EMail.html";
            if (!File.Exists(path))
                path = @"C:\EBAZTEN\TFS\EBZCLIENTS\TRONC\Essensys.Web\Essensys.Web.UI/Content/EMail.html";
            if (File.Exists(path))
            {
                return File.ReadAllText(path, Encoding.UTF8)
                    .Replace("[[HTML]]", htmlmessage)
                    .Replace("[[WEBSERVER]]", ConfigurationManager.AppSettings["webserver"]);
            }
            else
                throw new Exception("Le template de mail EMail.html n'est pas présent dans le répertoire Content");
        }

        /// <summary>
        /// Envoi d'un email simple
        /// </summary>
        /// <param name="from">Mail de provenance</param>
        /// <param name="to">Mail de destination</param>
        /// <param name="subject">Sujet</param>
        /// <param name="message">Message</param>
        /// <param name="htmlmessage">MessageHTML</param>
        public static void SendSimpleEMail(string from, string to, string subject, string message, string htmlmessage)
        {
            try
            {
                // Déclaration du client simple SMTP
                //SmtpClient Client = new SmtpClient(ConfigurationManager.AppSettings["mailserver"], Convert.ToInt32(ConfigurationManager.AppSettings["mailport"]));
                //Client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailuser"], ConfigurationManager.AppSettings["mailpassword"]);

                MailMessage Message = PrepareEMail(to, subject, message, htmlmessage);
                Message.From = new MailAddress(from, from);
                Message.Bcc.Add(new MailAddress(from, from));

                MailAddressCollection rpt = new MailAddressCollection();
                Message.ReplyToList.Add(new MailAddress(from, from));
                Message.Sender = new MailAddress(from, from);

                // Envoi du message avec Amazon RAW EMAIL
                var config = new AmazonSimpleEmailServiceConfig();
                AmazonSimpleEmailServiceClient service = new AmazonSimpleEmailServiceClient(ConfigurationManager.AppSettings["Amazon.Key"], ConfigurationManager.AppSettings["Amazon.SecretKey"]);

                SendRawEmailRequest request = new SendRawEmailRequest();
                request.RawMessage = new RawMessage();
                request.RawMessage.Data = BuildRawMailHelper.ConvertMailMessageToMemoryStream(Message);
                var response = service.SendRawEmail(request);
                //Client.Send(Message);
                LogManager.LogTrace("EMail correctement envoyé à " + to, null);
            }
            catch (Exception ex)
            {
                LogManager.LogError("Impossible d'envoyer l'email : " + ex.Message, ex);
                throw new Exception("Impossible d'envoyer l'email", ex);
            }
        }
    }


    public class BuildRawMailHelper
    {
        private const BindingFlags nonPublicInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly ConstructorInfo _mailWriterContructor;
        private static readonly MethodInfo _sendMethod;
        private static readonly MethodInfo _closeMethod;

        static BuildRawMailHelper()
        {
            Assembly systemAssembly = typeof(SmtpClient).Assembly;
            Type mailWriterType = systemAssembly
                .GetType("System.Net.Mail.MailWriter");

            _mailWriterContructor = mailWriterType
                .GetConstructor(nonPublicInstance, null,
                    new[] { typeof(Stream) }, null);

            _sendMethod = typeof(MailMessage).GetMethod("Send",
                nonPublicInstance);

            _closeMethod = mailWriterType.GetMethod("Close",
                nonPublicInstance);
        }

        public static MemoryStream ConvertMailMessageToMemoryStream(
            MailMessage message)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                object mailWriter = _mailWriterContructor.Invoke(
                    new object[] { memoryStream });

                _sendMethod.Invoke(message, nonPublicInstance, null,
                                    new[] { mailWriter, true, true }, null);

                _closeMethod.Invoke(mailWriter, nonPublicInstance,
                    null, new object[] { }, null);

                return memoryStream;
            }
        }
    }
}
