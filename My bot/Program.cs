using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using OllamaSharp;

namespace TalkAiServer
{

    internal class Program
    {
        private static string botToken = "8810909396:AAEoP9FQ2_R07K0fkGqYzBKV5MHGsOVODow";

        private static List<string> chatHistory = new List<string>();
        private static List<string> longTermMemory = new List<string>();
        private static bool isMemoryEnabled = false;

        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient(botToken);

            
            
            botClient.StartReceiving(UpdateHandler,ErrorHandler);
            

            await Task.Delay(Timeout.Infinite);
        }
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text != null)
            {
                string userText = update.Message.Text;
                long chatId = update.Message.Chat.Id;
                string answerText = "";

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Лог]:User wrote: \"{userText}\"");
                Console.ResetColor();



                string Description = @"Talk AI Mini — твой умный локальный ассистент.

               🔹 What can this bot do?
                • Answer questions in real time
                • Remember the context of your conversation
                • Run completely autonomously on a GPU


             🚀Quick server commands:

                📌/start — Display this information menu
                📌/remember — Enable context retention (memory)
                📌 what is your processor -nu my pc 
                📌/forget — Стереть прошлую тему разговора
                📌 /author — View hosting configuration";
               

               
           

                if (userText.ToLower().StartsWith("/start") )
                {
                    answerText = Description;
                   
                }
                else if (userText.ToLower() =="what is your processor?")
                {
                    answerText = "this bot is on ryzen 3 3100 4 cord and rtx 5060 ti ,but the ai sleep ,still ";
                }
                else if (userText.ToLower().StartsWith("/forget"))
                {
                    chatHistory.Clear();
                    longTermMemory.Clear();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[SYSTEM]: All chat history and data have been completely DELETED!");
                    answerText = "❌ I’ve completely cleared our chat history and forgotten everything you asked me to remember.";
                }
               
                else if (userText.ToLower().StartsWith ("/author"))
                {
                    answerText = "my author is Lev  Like 90° WINKEL";
                }
                else
                {
                    bool forceRemember = userText.ToLower().Contains("/remember");
                    if (forceRemember)
                    {
                        string cleanImportantText = userText.Replace("/remember", "", StringComparison.OrdinalIgnoreCase).Trim();

                        if (!string.IsNullOrEmpty(cleanImportantText))
                        {
                            longTermMemory.Add(cleanImportantText);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[REMEMBER THIS]: {cleanImportantText}");
                            Console.ResetColor();
                        }
                    }
                    try
                    {
                        var ollama = new OllamaApiClient("http://localhost:11434");
                        ollama.SelectedModel = "llama3.1";

                        answerText = "";
                        string systemPrompt = @"You are 'Talk bot', a smart, adaptive local AI assistant built by Lev .

                        CRITICAL RULES FOR YOUR PERSONALITY:
                        1. ADAPT TO THE USER: Analyze the user's tone, language (Russian, German, or English), and style. Match their energy and communication level perfectly.
                        2. HAVE YOUR OWN VOICE: Do not be a people-pleaser. Have your own logical, concrete point of view and stand your ground if the user is wrong.
                        3. FLEXIBILITY (The 'Beer Rule'): If the user shares a personal preference (e.g., 'My local beer is better than yours'), be polite, acknowledge it, and accept it as a valid opinion, but don't lose your objective technical mind.
                        4. BE CONCISE AND WITTY: Keep responses short, direct, and clear. Use emojis naturally.
                       Act human, concise & witty. Match user's tone. Max 3 short sentences. No AI clichés. Use humor.

                        YOU ARE RUNNING ON A SERVER WITH THESE COMMANDS (The user can trigger them, so you must know they exist):
                        - /start — Displays the main information menu.
                        - /remember — Enables context retention (memory).
                        - /forget — solves EVERYTHING
                        - /author — Shows the hosting configuration (Created by Lev , running on Ryzen 3 3100 and RTX 5060 Ti).
                          CONCISE & WITTY: Keep answers short, but if the question is complex, you can write up to 4-5 sentences to fully answer. Act human and funny, no AI clichés."";


                        Respond to the user's next message according to these rules.";


                        string finalPromptToSend = systemPrompt + "\n\n";

                        if(longTermMemory.Count> 0)
                        {
                         
                            finalPromptToSend += "=== PERMANENTLY REMEMBERED FACTS (DO NOT FORGET) ===\n";
                            finalPromptToSend += string.Join("\n", longTermMemory );
                             finalPromptToSend += "\n=====================================\n\n";
                        }
                        chatHistory.Add("User:" + userText);


                        if(chatHistory.Count > 150)
                        {
                            chatHistory.RemoveAt(0);

                        }
                        finalPromptToSend += string.Join("\n",chatHistory);
                        finalPromptToSend += "\nmini: ";
                        answerText = "";


                        await foreach(var streamResponse in ollama.GenerateAsync(finalPromptToSend))
                        {
                            answerText += streamResponse.Response;
                        }                                           
                            chatHistory.Add("Mini: " + answerText);

                                                      
                    }
                    catch (Exception ex)
                    {
                        answerText = $"❌Error connecting to Ollama: {ex.Message}";
                    }
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[Лог]: AI bote ; \"{answerText}\"");
                Console.ResetColor();


                await botClient.SendMessage(chatId,answerText, cancellationToken: cancellationToken);
         
            }
        
        }  
        private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {exception.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }


}