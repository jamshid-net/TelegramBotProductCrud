using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotCrud.TelegramService
{
    public class MainButtons
    {
        public  ReplyKeyboardMarkup Buttons()
        {

            KeyboardButton[][] keys = new KeyboardButton[][]
              {
                                                        new []
                                                        {
                                                            new KeyboardButton("Start"),
                                                            new KeyboardButton("GetById"),
                                                            new KeyboardButton("GetAll")
                                                        },
                                                          new KeyboardButton[]
                                                        {
                                                            new KeyboardButton("Add"),
                                                            new KeyboardButton("Remove"),
                                                            new KeyboardButton("Update")
                                                        }
              };
            ReplyKeyboardMarkup markup = new(keys)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true,
            }; 

            return markup;


        }

    }
}
