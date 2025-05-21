using System.Net;
using System.Net.Mail;

namespace backend.Utilities
{
    public class EmailManager
    {
        private readonly SmtpClient _client;
        private const string Host = "smtp.gmail.com";
        private const int Port = 587;
        private const string User = "dibujofacil646@gmail.com";
        private const string AppPassword = "lrmx igwb eokm temj"; 
        private const bool EnableSsl = true;

        public EmailManager()
        {
            _client = new SmtpClient(Host, Port)
            {
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(User, AppPassword)
            };
        }

        public void SendEmail(string to, string subject, string body, bool isHtml = false)
        {
            var mail = new MailMessage(User, to, subject, body)
            {
                IsBodyHtml = isHtml
            };

            _client.Send(mail);
            mail.Dispose();
        }

        public void SendPasswordResetEmail(string toEmail, string resetToken)
        {
            var resetLink = $"http://localhost:5173/reset-password?token={resetToken}";
            var subject = "Restablecer contraseña - DibujoFácil";
            var body = $@"
                <h1 style='color: #2D3748; font-family: Arial;'>Restablecimiento de contraseña</h1>
                <p style='font-size: 16px;'>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                <p style='font-size: 16px;'>Haz clic en el siguiente enlace para continuar:</p>
                <a href='{resetLink}' style='
                    background-color: #4299E1;
                    color: white;
                    padding: 12px 24px;
                    border-radius: 4px;
                    text-decoration: none;
                    display: inline-block;
                    margin: 20px 0;
                '>Restablecer contraseña</a>
                <p style='font-size: 14px; color: #718096;'>
                    Si no solicitaste este cambio, puedes ignorar este mensaje.<br>
                    El enlace expirará en 1 hora.
                </p>
            ";

            var mail = new MailMessage(
                from: User,
                to: toEmail,
                subject: subject,
                body: body
            )
            {
                IsBodyHtml = true
            };

            _client.Send(mail);
            mail.Dispose();
        }


        public void SendVerificationEmail(string toEmail, string token)
        {
            var verificationLink = $"http://localhost:5173/verify-email?token={token}";
            var body = $@"
        <h1>Verifica tu cuenta</h1>
        <p>Haz clic en el enlace para completar tu registro:</p>
        <a href='{verificationLink}'>Verificar cuenta</a>
        <p><small>Este enlace expira en 24 horas</small></p>
    ";

            SendEmail(toEmail, "Verifica tu cuenta - DibujoFácil", body, true);
        }
    }
}