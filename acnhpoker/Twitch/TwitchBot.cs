using AsyncAwaitBestPractices;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ACNHPoker
{
    public class TwitchBot : IDisposable
    {
        const string ip = "irc.chat.twitch.tv";
        const int port = 6667;
        TcpClient tcpClient;

        string password;
        string botUsername;
        string channel;

        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();

        public event TwitchChatEventHandler OnMessage = delegate { };
        public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);

        private bool stop = false;
        public class TwitchChatMessage : EventArgs
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
        }

        public TwitchBot(string TwitchBotUserName, string TwitchBotOauth, string TwitchChannelName)
        {
            botUsername = TwitchBotUserName;
            password = TwitchBotOauth;
            channel = TwitchChannelName;

            connect().SafeFireAndForget();
        }

        private async Task connect()
        {
            Start().SafeFireAndForget();
            //We could .SafeFireAndForget() these two calls if we want to
            await JoinChannel(channel);
            await SendMessage(channel, "Chat bot has started up!");

            OnMessage += async (sender, twitchChatMessage) =>
            {
                Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
                //Listen for !hey command
                if (twitchChatMessage.Message.StartsWith("!hey"))
                {
                    await SendMessage(twitchChatMessage.Channel, $"Hey there {twitchChatMessage.Sender}");
                }
            };

            await Task.Delay(-1);
        }

        public async Task Start()
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);
            streamReader = new StreamReader(tcpClient.GetStream());
            streamWriter = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };

            await streamWriter.WriteLineAsync($"PASS {password}");
            await streamWriter.WriteLineAsync($"NICK {botUsername}");
            connected.SetResult(0);

            while (true)
            {
                if (stop)
                    break;

                string line = await streamReader.ReadLineAsync();
                Console.WriteLine(line);

                if (line == null)
                    continue;

                string[] split = line.Split(' ');
                //PING :tmi.twitch.tv
                //Respond with PONG :tmi.twitch.tv
                if (line.StartsWith("PING"))
                {
                    Console.WriteLine("PONG");
                    await streamWriter.WriteLineAsync($"PONG {split[1]}");
                }

                if (split.Length > 2 && split[1] == "PRIVMSG")
                {
                    //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
                    // ^^^^^^^^
                    //Grab this name here
                    int exclamationPointPosition = split[0].IndexOf("!");
                    string username = split[0].Substring(1, exclamationPointPosition - 1);
                    //Skip the first character, the first colon, then find the next colon
                    int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
                    string message = line.Substring(secondColonPosition + 1);//Everything past the second colon
                    string channel = split[2].TrimStart('#');

                    OnMessage(this, new TwitchChatMessage
                    {
                        Message = message,
                        Sender = username,
                        Channel = channel
                    });
                }
            }
        }

        public async Task SendMessage(string channel, string message)
        {
            await connected.Task;
            await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
        }

        public async Task SendMessage(string message)
        {
            await connected.Task;
            await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
        }

        public async Task JoinChannel(string channel)
        {
            await connected.Task;
            await streamWriter.WriteLineAsync($"JOIN #{channel}");
        }

        public void Dispose()
        {
            stop = true;
            streamReader.Close();
            streamWriter.Close();
            tcpClient.Close();
        }
    }
}
