namespace Inventory.Application.Services
{
    public interface IWhatsAppService
    {
        Task SendMessageAsync(string phoneNumber, string message);
    }
}
