using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
        }

        public void SendEmail(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Administration of Website", _emailConfig.From));

            // Vérifier que message.To n'est pas null
            if (message.To == null || message.To.Count == 0)
                throw new ArgumentException("La liste des destinataires est vide.", nameof(message.To));

            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(TextFormat.Text) { Text = message.Content };

            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    Console.WriteLine("Connexion au serveur SMTP...");

                    // Utilisation correcte du port 587 avec StartTls
                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);

                    Console.WriteLine("Authentification...");
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.Username, _emailConfig.Password);

                    Console.WriteLine("Envoi de l'email...");
                    client.Send(mailMessage);

                    Console.WriteLine("Email envoyé avec succès !");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'envoi de l'email : {ex.Message}");
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
