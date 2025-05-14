using System.Threading.Tasks;

namespace Workshop.Service.Interface
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmail(string to, string body);
    }
}