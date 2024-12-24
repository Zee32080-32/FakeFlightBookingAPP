using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Threading.Tasks;
using static System.Windows.Forms.Design.AxImporter;


//interacts with SendGrid API
namespace FakeFlightBookingAPI.Services
{
    public class EmailService
    {
        private readonly SendGridOptions _sendGridOptions;

        public EmailService(IOptions<SendGridOptions> sendGridOptions)
        {
            _sendGridOptions = sendGridOptions.Value;
        }

        // asynchronous, allowing it to perform non-blocking operations
        public async Task SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            var client = new SendGridClient(_sendGridOptions.ApiKey);
            var from = new EmailAddress(_sendGridOptions.FromEmail);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, messageBody, messageBody);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception($"Email sending failed with status code: {response.StatusCode}");
            }

        }
    }
}
