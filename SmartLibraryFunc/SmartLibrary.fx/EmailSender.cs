using MailKit;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SmartLibrary.fx.Models;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SmartLibrary.fx
{
    public class EmailSender
    {
        private const string From = "saitejagoud123@gmail.com";
        private const string SmtpServer = "smtp.gmail.com";
        private const int Port = 465;
        private const string Username = "saitejagoud123@gmail.com";
        private const string Password = "cmatkeorgmooksya";

        [FunctionName("EmailSender")]
        public void Run([QueueTrigger("smartlibraryemailqueue", Connection = "azure_storage_connection_string")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var newMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<NewMessage>(myQueueItem);
            var message = new Message(newMessage.To, newMessage.Subject, newMessage.Content, new string[] { }, new string[] { }, newMessage.IsHtml);
            SendEmailInstantly(message);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public bool SendEmailInstantly(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            return Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Cc.AddRange(message.Cc);
            emailMessage.Bcc.AddRange(message.Bcc);

            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(message.IsHtml ? TextFormat.Html : TextFormat.Plain) { Text = message.Content };

            return emailMessage;
        }

        private bool Send(MimeMessage mailMessage)
        {
            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    smtpClient.Connect(SmtpServer, Port, true);
                    smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    smtpClient.Authenticate(Username, Password);
                    smtpClient.MessageSent += OnMessageSent;
                    smtpClient.Send(mailMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    smtpClient.Disconnect(true);
                }
            }
        }
        private void OnMessageSent(object sender, MessageSentEventArgs e)
        {
            Console.WriteLine("The message was sent!");
        }
    }
}
