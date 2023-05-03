using BotCrud;
using BotCrud.TelegramService;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramHandler : IUpdateHandler
{
    private ITelegramBotClient _client;
    static MainButtons mainButtons = new MainButtons();
    AppDbContext _db = new AppDbContext();
    List<User> _users = new();
    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine("HandlePollingErrorAsync:" + exception.Message);
        await botClient.SendTextMessageAsync(591208356, "Date:" + DateTime.Now + " Exception: " + exception.Message);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _client, Update update, CancellationToken cancellationToken)
    {

        if (update.Message == null) return;

        Parallel.Invoke(async () =>
        {


            await Console.Out.WriteLineAsync(update.Message.Chat.FirstName);


            string cmd = update?.Message?.Text ?? "";
            if (!cmd.StartsWith("/")) { cmd = "/" + cmd.ToLower(); }
            _users.Add(new User() { ChatId = update.Message.Chat.Id });
            var user = _users.FirstOrDefault(x => x.ChatId == update.Message.Chat.Id);

            if (user?.step == -1)
            {
                user.Command = cmd;
                if (cmd.Equals("/add")) user.step = 0;
                if (cmd.Equals("/update")) user.step = 0;
                if (cmd.Equals("/remove")) user.step = 0;
            }
            else if (user != null) cmd = user.Command;

            switch (cmd)
            {
                case "/start": StartHandler(_client, update); break;
                case "/getall": GetAll(_client, update); break;
                case "/getbyid": GetById(_client, update); break;
                case "/add": Add(_client, update, cancellationToken); break;
                case "/remove": Remove(_client, update); break;
                case "/update": UpdateProduct(_client, update, cancellationToken); break;

                default:
                    StartHandler(_client, update);
                    ; break;
            }
        });
    }
    private void StartHandler(ITelegramBotClient _client, Update update)
    {
      
      _client.SendTextMessageAsync
            (update.Message.Chat.Id, 
            "Hi " + update.Message.Chat.FirstName,
            ParseMode.Markdown, replyMarkup: mainButtons.Buttons());
    }
    private async void UpdateProduct(ITelegramBotClient client, Update? update, CancellationToken cancellationToken)
    {
        long id = update.Message.Chat.Id;
        var user = _users.FirstOrDefault(x => x.ChatId == id);
        if (user != null)
        {
            Message message = new Message();
            switch (user.step)
            {
                case 0:
                    {
                        await client.SendTextMessageAsync(id, "enter id: ");
                        user.step++;
                        break;



                    }
                case 1:
                    {
                        user.Product.Id = Convert.ToInt32(update.Message.Text);
                        await client.SendTextMessageAsync(id, "Name:");
                        user.step++;
                        break;
                    }

                case 2:
                    {
                        if (!string.IsNullOrWhiteSpace(update.Message.Text))
                        {
                            user.Product.Name = update.Message.Text;
                            await client.SendTextMessageAsync(id, "Price:");
                            user.step++;

                        }
                        else
                        {
                            user.step--;
                            await client.SendTextMessageAsync(id, "Invalid Name! \nPlease reinput value");
                        }
                        break;
                    }
                case 3:
                    {
                        if (decimal.TryParse(update.Message.Text, out decimal price))
                        {
                            user.Product.Price = price;
                            await client.SendTextMessageAsync(id, "Photo:");
                            user.step++;
                        }
                        else
                        {
                            await client.SendTextMessageAsync(id, "Invalid Price! \nPlease reinput value");
                        }
                        break;
                    }
                case 4:
                    {

                        Telegram.Bot.Types.File file = new Telegram.Bot.Types.File();
                        string path = @"D:\TestUpload\";
                        bool flag = false;
                        if (update.Message.Document != null)
                        {
                            file = await client.GetFileAsync(update.Message.Document.FileId);
                            flag = true;
                        }
                        else if (update.Message.Photo != null)
                        {
                            file = await client.GetFileAsync(update.Message.Photo.Last().FileId);
                            flag = true;
                        }
                        else
                        {
                            await client.SendTextMessageAsync(id, "Error No photo !");
                            await client.SendTextMessageAsync(id, "Enterdays");
                            user.step++;

                        }
                        if (flag)
                        {
                            path += Path.GetFileName(file.FilePath);
                            using var stream = System.IO.File.OpenWrite(path);
                            var res = await client.GetInfoAndDownloadFileAsync(file.FileId, stream);
                            await client.SendTextMessageAsync(id, "Uploading... wait!");
                        }






                        if (System.IO.File.Exists(path))
                        {
                            user.Product.Img = path;
                            await client.SendTextMessageAsync(id, "Enterdays");
                            user.step++;
                        }

                        break;
                    }
                case 5:
                    {
                        DateTime localTime = DateTime.Now;
                        DateTime utcTime = localTime.ToUniversalTime();

                        try
                        {
                            utcTime.AddDays(Convert.ToInt32(update.Message.Text));
                        }
                        catch (Exception)
                        {

                            utcTime.AddDays(7);
                        }



                        user.Product.ExpireDate = utcTime;

                        KeyboardButton[][] keys = new KeyboardButton[][]
                                                         {
                                                        new []
                                                        {
                                                            new KeyboardButton("Vegetables"),
                                                            new KeyboardButton("Fruits"),

                                                        },
                                                          new KeyboardButton[]
                                                        {
                                                            new KeyboardButton("Drinks"),
                                                            new KeyboardButton("MilkyProducts"),

                                                        }
                                                         };
                        ReplyKeyboardMarkup markup = new(keys)
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true,
                        };




                        message = await client.SendTextMessageAsync(id, "CategoryName:", replyMarkup: markup, cancellationToken: cancellationToken);
                        user.step++;

                        
                        break;
                    }
                case 6:
                    {
                        switch (message.Text)
                        {
                            case "Vegetables": user.Product.CategoryName = Category.Vegetables; break;
                            case "Fruits": user.Product.CategoryName = Category.Fruits; break;
                            case "Drinks": user.Product.CategoryName = Category.Drinks; break;
                            case "MilkyProducts": user.Product.CategoryName = Category.MilkyProducts; break;
                        }


                        _db.Products.Update(user.Product);
                        _db.SaveChanges();



                        await client.SendTextMessageAsync
                            (id, "Succsess updated",
                            ParseMode.Markdown, 
                            replyMarkup: mainButtons.Buttons());
                        user.Product = new Product();
                        user.step = -1;


                        break;
                    }

            }
        }
    }

    private async void Remove(ITelegramBotClient client, Update? update)
    {
        long id = update.Message.Chat.Id;
        var user = _users.FirstOrDefault(x => x.ChatId == id);
        if(user != null)
        {
            switch(user.step)
            {
                case 0:
                    {
                        await client.SendTextMessageAsync(id, "Enter product id for remove");
                        user.step++;


                        break;
                    }

               case 1:
                    {
                        if(int.TryParse(update.Message.Text,out int ProductId))
                        {
                            _db.Products.Remove(await _db.Products.FirstOrDefaultAsync(x => x.Id == ProductId));
                            _db.SaveChanges();
                            user.step = -1;
                            await client.SendTextMessageAsync(id, "Succsess!", replyMarkup: mainButtons.Buttons());
                        }
                        else
                        {
                            await client.SendTextMessageAsync(id, "Error!",replyMarkup: mainButtons.Buttons());
                          
                        }
                        

                        break;
                    }


            }


        }


    }

    private async void Add(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        long id = update.Message.Chat.Id;
        var user = _users.FirstOrDefault(x => x.ChatId == id);
        if (user != null)
        {
            Message message = new Message();
            switch (user.step)
            {
                case 0:
                    {
                        await client.SendTextMessageAsync(id, "Name:");
                        user.step++;
                        break;
                    }

                case 1:
                    {
                        if (!string.IsNullOrWhiteSpace(update.Message.Text))
                        {
                            user.Product.Name = update.Message.Text;
                            await client.SendTextMessageAsync(id, "Price:");
                            user.step++;

                        }
                        else
                        {
                            user.step--;
                            await client.SendTextMessageAsync(id, "Invalid Name! \nPlease reinput value");
                        }
                        break;
                    }
                case 2:
                    {
                        if (decimal.TryParse(update.Message.Text, out decimal price))
                        {
                            user.Product.Price = price;
                            await client.SendTextMessageAsync(id, "Photo:");
                            user.step++;
                        }
                        else
                        {
                            await client.SendTextMessageAsync(id, "Invalid Price! \nPlease reinput value");
                        }
                        break;
                    }
                case 3:
                    {


                        Telegram.Bot.Types.File file = new Telegram.Bot.Types.File();
                        string path = @"D:\TestUpload\";
                        bool flag = false;
                        if (update.Message.Document != null)
                        {
                            file = await client.GetFileAsync(update.Message.Document.FileId);
                            flag = true;
                        }
                        else if (update.Message.Photo != null)
                        {
                            file = await client.GetFileAsync(update.Message.Photo.Last().FileId);
                            flag = true;
                        }
                        else
                        {
                            await client.SendTextMessageAsync(id, "Error No photo !");
                            await client.SendTextMessageAsync(id, "Enterdays");
                            user.step++;

                        }
                        if (flag)
                        {
                            path += Path.GetFileName(file.FilePath);
                            using var stream = System.IO.File.OpenWrite(path);
                            var res = await client.GetInfoAndDownloadFileAsync(file.FileId, stream);
                            await client.SendTextMessageAsync(id, "Uploading... wait!");
                        }
                       





                        if (System.IO.File.Exists(path))
                        {
                            user.Product.Img = path;
                            await client.SendTextMessageAsync(id, "Enterdays");
                            user.step++;
                        }
                        
                        break;
                    }
                case 4:
                    {
                        DateTime localTime = DateTime.Now;
                        DateTime utcTime = localTime.ToUniversalTime();

                        try
                        {
                            utcTime.AddDays(Convert.ToInt32(update.Message.Text));
                        }
                        catch (Exception)
                        {

                            utcTime.AddDays(7);
                        }



                        user.Product.ExpireDate = utcTime;

                        KeyboardButton[][] keys = new KeyboardButton[][]
                                                         {
                                                        new []
                                                        {
                                                            new KeyboardButton("Vegetables"),
                                                            new KeyboardButton("Fruits"),

                                                        },
                                                          new KeyboardButton[]
                                                        {
                                                            new KeyboardButton("Drinks"),
                                                            new KeyboardButton("MilkyProducts"),

                                                        }
                                                         };
                        ReplyKeyboardMarkup markup = new(keys)
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true,
                        };




                        message = await client.SendTextMessageAsync(id, 
                            "CategoryName:", replyMarkup: markup,
                            cancellationToken: cancellationToken);
                        user.step++;

                        
                        break;
                    }
                case 5:
                    {
                        switch (message.Text)
                        {
                            case "Vegetables": user.Product.CategoryName = Category.Vegetables; break;
                            case "Fruits": user.Product.CategoryName = Category.Fruits; break;
                            case "Drinks": user.Product.CategoryName = Category.Drinks; break;
                            case "MilkyProducts": user.Product.CategoryName = Category.MilkyProducts; break;
                        }


                        _db.Products.Add(user.Product);
                        _db.SaveChanges();





                        await client.SendTextMessageAsync(id, 
                            "Succsess", ParseMode.Markdown, 
                            replyMarkup: mainButtons.Buttons());
                        user.Product = new Product();
                        user.step = -1;


                        break;
                    }

            }
        }
    }

    private void GetById(ITelegramBotClient client, Update? update)
    {
        Console.WriteLine("xd");
    }

    private async void GetAll(ITelegramBotClient client, Update? update)
    {
        long id = update.Message.Chat.Id;
        StringBuilder stringBuilder = new StringBuilder();
        // Define column headers
        (string Header, int Width)[] columns = {
    ("ID", 5),
    ("Name", 12),
    ("Price", 8),
    ("Image", 12),
    ("Expire Date", 12),
    ("Category", 10)
};

        // Print headers
        stringBuilder.AppendLine(string.Join("", columns.Select(c => c.Header.PadRight(c.Width))));

        // Print data rows
        

        foreach (var product in _db.Products)
        {
            stringBuilder.AppendFormat("{0,-5} {1,-12} {2,-8} {3,-12} {4,-12} {5,-10}\n",
                product.Id,
                product.Name,
                product.Price.ToString("C"),
                product.Img,
                product.ExpireDate.ToString("yyyy-MM-dd"),
                Enum.GetName(typeof(Category), product.CategoryName));
        }


        using (StreamWriter fs = new("products.txt"))
        {
            fs.WriteLine(stringBuilder.ToString());

        }


        using Stream fr = System.IO.File.OpenRead("products.txt");
        await client.SendDocumentAsync(id, document: new InputOnlineFile(content: fr, fileName: "products.txt"));
        await client.SendTextMessageAsync(id, stringBuilder.ToString());

           

         await client.SendTextMessageAsync(id, "✅",
             ParseMode.Markdown, replyMarkup: mainButtons.Buttons());


    }


}

record User
{
    public long ChatId { get; set; } = 0;
    public sbyte step { get; set; } = -1;
    public Product Product { get; set; } = new Product();
    public string Command { get; set; } = "";
}