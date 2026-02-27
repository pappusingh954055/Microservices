namespace Inventory.Application.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public async Task SendMessageAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                Console.WriteLine("[WhatsAppService] Phone number missing. Skipping WhatsApp.");
                return;
            }

            try
            {
                // Placeholder for real WhatsApp API integration (e.g., Twilio, Interakt, Meta Cloud API)
                // For now, we simulate the sending process.
                Console.WriteLine($"[WhatsAppService] Sending WhatsApp to {phoneNumber}: {message}");
                
                // Simulate network delay
                await Task.Delay(500);

                Console.WriteLine($"[WhatsAppService] WhatsApp sent successfully to {phoneNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsAppService] Failed to send WhatsApp to {phoneNumber}: {ex.Message}");
            }
        }
    }
}
