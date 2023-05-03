using Telegram.Bot;

namespace BotCrud.TelegramService.TelegramService
{
    internal class TelegramService
    {
        private readonly TelegramBotClient Bot;
        public TelegramService(string token)
        {
            Bot = new(token);
        }

        public void Start()
        {
           Bot.ReceiveAsync<TelegramHandler>();
            Console.ReadKey();
        }
    }
}
