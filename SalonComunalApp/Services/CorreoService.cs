using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SalonComunalApp.Interfaces;
using SalonComunalApp.Models;

namespace SalonComunalApp.Services
{
    public class CorreoService : ICorreoService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CorreoService> _logger;

        public CorreoService(IConfiguration config, ILogger<CorreoService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            try
            {
                var smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _config["EmailSettings:Usuario"] ?? "";
                var smtpPass = _config["EmailSettings:Password"] ?? "";
                var remitente = _config["EmailSettings:Remitente"] ?? "noreply@saloncomunal.com";

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress("Salón Comunal", remitente));
                mensaje.To.Add(MailboxAddress.Parse(destinatario));
                mensaje.Subject = asunto;
                mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(smtpUser, smtpPass);
                await smtp.SendAsync(mensaje);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Correo enviado a {Destinatario}: {Asunto}", destinatario, asunto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Destinatario}", destinatario);
            }
        }

        private string PlantillaBase(string titulo, string contenido) => $@"
            <html><body style='font-family:Arial,sans-serif;color:#333;'>
            <div style='max-width:600px;margin:auto;border:1px solid #ddd;border-radius:8px;overflow:hidden;'>
                <div style='background:#2c6e49;color:white;padding:20px;text-align:center;'>
                    <h2>🏛️ Salón Comunal</h2><p style='margin:0;'>{titulo}</p>
                </div>
                <div style='padding:24px;'>{contenido}</div>
                <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#999;'>
                    © 2026 Salón Comunal
                </div>
            </div></body></html>";

        public async Task EnviarConfirmacionCompraAsync(string destinatario, List<CarritoItem> items, decimal total, DateTime fechaCompra)
        {
            var filas = string.Join("", items.Select(i =>
                $"<tr><td style='padding:8px;border:1px solid #ddd;'>{i.Nombre}</td>" +
                $"<td style='padding:8px;border:1px solid #ddd;text-align:center;'>{i.Cantidad}</td>" +
                $"<td style='padding:8px;border:1px solid #ddd;text-align:right;'>₡{i.Precio:N2}</td>" +
                $"<td style='padding:8px;border:1px solid #ddd;text-align:right;'>₡{i.Subtotal:N2}</td></tr>"));

            var contenido = $@"<p>Gracias por tu compra realizada el <strong>{fechaCompra:dd/MM/yyyy HH:mm}</strong>.</p>
                <table style='width:100%;border-collapse:collapse;'>
                <thead><tr style='background:#f0f0f0;'>
                    <th style='padding:8px;border:1px solid #ddd;'>Producto</th>
                    <th style='padding:8px;border:1px solid #ddd;'>Cant.</th>
                    <th style='padding:8px;border:1px solid #ddd;'>Precio</th>
                    <th style='padding:8px;border:1px solid #ddd;'>Subtotal</th>
                </tr></thead><tbody>{filas}</tbody>
                <tfoot><tr style='background:#e8f5e9;font-weight:bold;'>
                    <td colspan='3' style='padding:10px;border:1px solid #ddd;text-align:right;'>TOTAL</td>
                    <td style='padding:10px;border:1px solid #ddd;text-align:right;'>₡{total:N2}</td>
                </tr></tfoot></table>";

            await EnviarAsync(destinatario, "✅ Confirmación de compra - Salón Comunal",
                PlantillaBase("Confirmación de Compra", contenido));
        }

        public async Task EnviarConfirmacionReservaAsync(string destinatario, Reserva reserva, decimal adelanto)
        {
            var fechaLimite = reserva.FechaEvento.AddDays(-7);
            var contenido = $@"
                <p>Su reserva ha sido registrada exitosamente.</p>
                <table style='width:100%;border-collapse:collapse;'>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>N.º Reserva</strong></td><td style='padding:8px;border:1px solid #ddd;'>#{reserva.Id}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Fecha del evento</strong></td><td style='padding:8px;border:1px solid #ddd;'>{reserva.FechaEvento:dd/MM/yyyy}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Total del servicio</strong></td><td style='padding:8px;border:1px solid #ddd;'>₡{reserva.Total:N2}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Adelanto pagado (25%)</strong></td><td style='padding:8px;border:1px solid #ddd;'>₡{adelanto:N2}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Monto restante (75%)</strong></td><td style='padding:8px;border:1px solid #ddd;'>₡{reserva.MontoRestante:N2}</td></tr>
                    <tr style='background:#fff3cd;'><td style='padding:8px;border:1px solid #ddd;'><strong>⚠️ Fecha límite de pago</strong></td><td style='padding:8px;border:1px solid #ddd;'><strong>{fechaLimite:dd/MM/yyyy}</strong></td></tr>
                </table>
                <p style='color:#c0392b;margin-top:16px;'><strong>Importante:</strong> Si no cancela el monto restante antes del {fechaLimite:dd/MM/yyyy}, perderá el adelanto y se liberará el espacio.</p>";

            await EnviarAsync(destinatario, "📅 Reserva confirmada - Salón Comunal",
                PlantillaBase("Reserva Confirmada", contenido));
        }

        public async Task EnviarConfirmacionPagoTotalAsync(string destinatario, Reserva reserva)
        {
            var contenido = $@"
                <p>El pago total de su reserva ha sido procesado. ¡Su evento está confirmado!</p>
                <table style='width:100%;border-collapse:collapse;'>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>N.º Reserva</strong></td><td style='padding:8px;border:1px solid #ddd;'>#{reserva.Id}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Fecha del evento</strong></td><td style='padding:8px;border:1px solid #ddd;'>{reserva.FechaEvento:dd/MM/yyyy}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Total pagado</strong></td><td style='padding:8px;border:1px solid #ddd;'>₡{reserva.Total:N2}</td></tr>
                    <tr style='background:#e8f5e9;'><td style='padding:8px;border:1px solid #ddd;'><strong>Estado</strong></td><td style='padding:8px;border:1px solid #ddd;'><strong>✅ Pagado en su totalidad</strong></td></tr>
                </table>";

            await EnviarAsync(destinatario, "✅ Pago total confirmado - Salón Comunal",
                PlantillaBase("Pago Total Confirmado", contenido));
        }

        public async Task EnviarCorreoCancelacionAsync(string destinatario, Reserva reserva)
        {
            bool pierdeAdelanto = reserva.Estado == "CanceladoSinReembolso";
            var contenido = $@"
                <p>Su reserva ha sido cancelada.</p>
                <table style='width:100%;border-collapse:collapse;'>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>N.º Reserva</strong></td><td style='padding:8px;border:1px solid #ddd;'>#{reserva.Id}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Fecha del evento</strong></td><td style='padding:8px;border:1px solid #ddd;'>{reserva.FechaEvento:dd/MM/yyyy}</td></tr>
                    <tr><td style='padding:8px;border:1px solid #ddd;'><strong>Estado</strong></td><td style='padding:8px;border:1px solid #ddd;'>{(pierdeAdelanto ? "❌ Cancelado sin reembolso" : "Cancelado")}</td></tr>
                </table>
                {(pierdeAdelanto ? "<p style='color:#c0392b;margin-top:16px;'><strong>El adelanto del 25% no será reembolsado</strong> debido a que la cancelación se realizó después del plazo establecido.</p>" : "<p>El espacio ha sido liberado.</p>")}";

            await EnviarAsync(destinatario, "❌ Reserva cancelada - Salón Comunal",
                PlantillaBase("Reserva Cancelada", contenido));
        }
    }
}
