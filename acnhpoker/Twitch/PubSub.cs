using AsyncAwaitBestPractices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Interfaces;

namespace ACNHPoker
{
    class PubSub : IDisposable
    {
        public static ITwitchPubSub MyPubSub;

        private static TwitchBot MyTwitchBot;

        private static DataTable itemSource = null;
        private static DataTable recipeSource = null;
        private static Dictionary<string, string> ItemDict;
        private static Dictionary<string, string> ReverseDict;
        private static Dictionary<string, string> ColorDict;
        private static Dictionary<string, string> RecipeDict;
        private static Dictionary<string, string> VillagerDict;

        private static dodo dodoSetup;

        private string channelId;
        private string accessToken;

        private string DropItemRewardId;
        private string DropRecipeRewardId;
        private string InjectVillagerRewardId;

        public PubSub(TwitchBot TwitchBot, string TwitchChannelid, string TwitchChannelAccessToken, dodo owner)
        {
            channelId = TwitchChannelid;
            accessToken = TwitchChannelAccessToken;
            DropItemRewardId = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "DropItemRewardId");
            DropRecipeRewardId = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "DropRecipeRewardId");
            InjectVillagerRewardId = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "InjectVillagerRewardId");

            dodoSetup = owner;

            MyTwitchBot = TwitchBot;

            if (File.Exists(Utilities.itemPath))
            {
                itemSource = loadItemCSV(Utilities.itemPath);
                recipeSource = loadItemCSV(Utilities.recipePath);
                ItemDict = CreateItemDictionary(Utilities.itemPath); // Name -> ID
                ReverseDict = CreateReverseDictionary(Utilities.itemPath); // ID -> Name
                ColorDict = CreateColorDictionary(Utilities.itemPath); // ID -> Color
                RecipeDict = CreateRecipeDictionary(Utilities.recipePath); // ID <-> Name
                VillagerDict = CreateVillagerDictionary(); // Rname -> Iname
            }

            connect().SafeFireAndForget();
        }

        private async Task connect()
        {
            //Set up twitchlib pubsub
            MyPubSub = new TwitchPubSub();
            MyPubSub.OnListenResponse += OnListenResponse;
            MyPubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
            MyPubSub.OnPubSubServiceClosed += OnPubSubServiceClosed;
            MyPubSub.OnPubSubServiceError += OnPubSubServiceError;

            //Set up listeners
            //ListenToBits(channelId);
            //ListenToChatModeratorActions(channelId, channelId);
            //ListenToCommerce(channelId);
            //ListenToFollows(channelId);
            //ListenToLeaderboards(channelId);
            //ListenToPredictions(channelId);
            //ListenToRaid(channelId);
            ListenToRewards(channelId);
            //ListenToSubscriptions(channelId);
            //ListenToVideoPlayback(channelId);
            //ListenToWhispers(channelId);

            //Connect to pubsub
            MyPubSub.Connect();

            await Task.Delay(-1);
        }

        #region Whisper Events

        private void ListenToWhispers(string channelId)
        {
            MyPubSub.OnWhisper += PubSub_OnWhisper;
            MyPubSub.ListenToWhispers(channelId);
        }

        private void PubSub_OnWhisper(object sender, OnWhisperArgs e)
        {
            //_logger.Information($"{e.Whisper.DataObjectWhisperReceived.Recipient.DisplayName} send a whisper {e.Whisper.DataObjectWhisperReceived.Body}");
        }

        #endregion

        #region Video Playback Events

        private void ListenToVideoPlayback(string channelId)
        {
            MyPubSub.OnStreamUp += PubSub_OnStreamUp;
            MyPubSub.OnStreamDown += PubSub_OnStreamDown;
            MyPubSub.OnViewCount += PubSub_OnViewCount;
            //MyPubSub.OnCommercial += PubSub_OnCommercial;
            MyPubSub.ListenToVideoPlayback(channelId);
        }

        private void PubSub_OnViewCount(object sender, OnViewCountArgs e)
        {
            //_logger.Information($"Current viewers: {e.Viewers}");
        }

        private void PubSub_OnStreamDown(object sender, OnStreamDownArgs e)
        {
            //_logger.Information($"The stream is down");
        }

        private void PubSub_OnStreamUp(object sender, OnStreamUpArgs e)
        {
            //_logger.Information($"The stream is up");
        }

        #endregion

        #region Subscription Events

        private void ListenToSubscriptions(string channelId)
        {
            MyPubSub.OnChannelSubscription += PubSub_OnChannelSubscription;
            MyPubSub.ListenToSubscriptions(channelId);
        }

        private void PubSub_OnChannelSubscription(object sender, OnChannelSubscriptionArgs e)
        {
            var gifted = e.Subscription.IsGift ?? false;
            if (gifted)
            {
                //_logger.Information($"{e.Subscription.DisplayName} gifted a subscription to {e.Subscription.RecipientName}");
            }
            else
            {
                var cumulativeMonths = e.Subscription.CumulativeMonths ?? 0;
                if (cumulativeMonths != 0)
                {
                    //_logger.Information($"{e.Subscription.DisplayName} just subscribed (total of {cumulativeMonths} months)");
                }
                else
                {
                    //_logger.Information($"{e.Subscription.DisplayName} just subscribed");
                }

            }

        }

        #endregion

        #region Reward Events

        private void ListenToRewards(string channelId)
        {
            MyPubSub.OnRewardRedeemed += PubSub_OnRewardRedeemed;
            MyPubSub.OnCustomRewardCreated += PubSub_OnCustomRewardCreated;
            MyPubSub.OnCustomRewardDeleted += PubSub_OnCustomRewardDeleted;
            MyPubSub.OnCustomRewardUpdated += PubSub_OnCustomRewardUpdated;
            MyPubSub.ListenToRewards(channelId);
        }

        private void PubSub_OnCustomRewardUpdated(object sender, OnCustomRewardUpdatedArgs e)
        {
            dodoSetup.WriteLog($"Reward {e.RewardTitle} \"{e.RewardId}\" has been updated", false);
        }

        private void PubSub_OnCustomRewardDeleted(object sender, OnCustomRewardDeletedArgs e)
        {
            dodoSetup.WriteLog($"Reward {e.RewardTitle} \"{e.RewardId}\" has been removed", false);
        }

        private void PubSub_OnCustomRewardCreated(object sender, OnCustomRewardCreatedArgs e)
        {
            dodoSetup.WriteLog($"Reward {e.RewardTitle} \"{e.RewardId}\" has been created", false);
        }

        private async void PubSub_OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            //Statuses can be:
            // "UNFULFILLED": when a user redeemed the reward
            // "FULFILLED": when a broadcaster or moderator marked the reward as complete
            /*
            if (e.Status == "UNFULFILLED")
            {
                Debug.Print($"{e.DisplayName} redeemed: {e.RewardTitle}");
            }
            */
            //Debug.Print($"Reward from {e.DisplayName} ({e.RewardTitle}) ({e.RewardId}) ({e.Message}) has been marked as complete");
            Debug.Print($"{e.TimeStamp} {e.ChannelId} {e.DisplayName} {e.RewardTitle} {e.RewardId} : {e.Message}");

            if (e.RewardId.ToString().Equals(DropItemRewardId)) // Drop item
            {
                string name = "";
                string num = "0";
                string message = e.Message.Replace('’', '\'').Trim();
                if (message.Contains(","))
                {
                    string[] temp = message.Split(',');
                    if (temp.Length >= 2)
                    {
                        name = temp[0].Trim();
                        num = temp[temp.Length - 1].Trim();
                    }
                }
                else
                {
                    name = message;
                }

                await CheckAndAddItem(name, num, e.DisplayName);
            }
            else if (e.RewardId.ToString().Equals(DropRecipeRewardId)) // Drop recipe
            {
                string name = e.Message.Replace('’', '\'').Trim();

                await CheckAndAddRecipe(name, e.DisplayName);
            }
            else if (e.RewardId.ToString().Equals(InjectVillagerRewardId)) // Inject villager
            {
                string name = e.Message.Replace('’', '\'').Replace('é','e').Replace('É','E').Replace('[', ' ').Replace(']', ' ').Trim();

                await CheckAndAddVillager(name, e.DisplayName, e.Message);
            }
        }

        public static async Task CheckAndAddItem(string name, string numStr, string displayName)
        {
            int num = 0;
            int otherColor = 0;
            string colorStr = "";


            if (numStr.Contains("[") && numStr.Contains("]"))                       // ,[2]     ,[red]
            {
                string tempNum = numStr.Replace("[", "").Replace("]", "");          // 2 red
                try
                {
                    otherColor = int.Parse(tempNum);                                // 2
                }
                catch
                {
                    otherColor = 0;
                    colorStr = tempNum.ToLower();                                   // red
                    Debug.Print("OtherColor Invalid");
                }
            }
            else
            {
                try
                {
                    num = int.Parse(numStr);                                        // ,2
                }
                catch
                {
                    num = 0;
                    Debug.Print("Num Invalid");
                }
            }

            string hexValue = "0";
            if (num > 0)
            {
                hexValue = (num - 1).ToString("X");
            }

            //-----------------------------------------------------------------
            string id = "";
            string color = "";
            if (ItemDict.ContainsKey(name.ToLower()) && otherColor == 0)            // exect name       exect name,[red]
            {
                DataView dv = new DataView(itemSource);
                dv.RowFilter = string.Format("eng LIKE '%{0}%' AND color = '{1}'", EscapeLikeValue(name), EscapeLikeValue(colorStr));
                if (dv.Count > 0)                                                   // exect name,[red]
                {
                    DataRowView drv = dv[0];
                    id = drv["id"].ToString();
                    color = drv["color"].ToString();
                }
                else
                {
                    id = ItemDict[name.ToLower()];
                    if (ColorDict.ContainsKey(id))
                        color = ColorDict[id];
                }
            }
            else
            {
                if (!colorStr.Equals(string.Empty))                                 // LIKE name,[red]
                {
                    DataView dv = new DataView(itemSource);
                    dv.RowFilter = string.Format("eng LIKE '%{0}%' AND color = '{1}'", EscapeLikeValue(name), EscapeLikeValue(colorStr));
                    if (dv.Count > 0)
                    {
                        DataRowView drv = dv[0];
                        id = drv["id"].ToString();
                        color = drv["color"].ToString();
                    }
                }


                if (id.Equals(string.Empty))
                {
                    DataView dv = new DataView(itemSource);
                    dv.RowFilter = string.Format("eng LIKE '%{0}%'", EscapeLikeValue(name));
                    if (dv.Count > 0)
                    {
                        DataRowView drv;

                        if (ItemDict.ContainsKey(name.ToLower()))                   // exect name,[2]
                        {
                            int StartRow = 9999;
                            int EndRow = 0;
                            for (int i = 0; i < dv.Count; i++)
                            {
                                if (dv[i]["eng"].ToString().ToLower() == name.ToLower())
                                {
                                    if (i < StartRow)
                                        StartRow = i;
                                    if (i > EndRow)
                                        EndRow = i;
                                }
                            }

                            if (StartRow + otherColor <= EndRow)
                            {
                                drv = dv[StartRow + otherColor];
                            }
                            else
                            {
                                drv = dv[EndRow];
                            }

                        }
                        else
                        {
                            if (otherColor > dv.Count)
                            {
                                drv = dv[dv.Count - 1];
                            }
                            else
                            {
                                drv = dv[otherColor];
                            }
                        }
                        id = drv["id"].ToString();
                        color = drv["color"].ToString();
                    }
                    else
                    {
                        if (name.Contains("[") && name.Contains("]"))
                            await MyTwitchBot.SendMessage($"Sorry, I am unable to find an item with the name \"{name}\". Did you forget the comma \",\" before the Brackets \"[ ]\"?");
                        else if (name.Contains("0") || name.Contains("1") || name.Contains("2") || name.Contains("3") || name.Contains("4") || name.Contains("5") || name.Contains("6") || name.Contains("7") || name.Contains("8") || name.Contains("9"))
                            await MyTwitchBot.SendMessage($"Sorry, I am unable to find an item with the name \"{name}\". Did you forget the comma \",\" before the number?");
                        else
                            await MyTwitchBot.SendMessage($"Sorry, I am unable to find an item with the name \"{name}\". Are you sure you are using the correct \"English\" name?");
                        return;
                    }
                }
            }




            Debug.Print($"id {id} | hexValue {hexValue} | color {color}");


            if (!id.Equals(String.Empty))
            {
                UInt16 itemId;
                bool success = UInt16.TryParse(id, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out itemId);

                if (!color.Equals(string.Empty))
                {
                    if (ReverseDict.ContainsKey(id))
                        await MyTwitchBot.SendMessage($"{displayName}, your order of \"{ReverseDict[id]}\" ({color}) have been received!");
                    else
                        await MyTwitchBot.SendMessage($"{displayName}, your order have been received!");
                }
                else if (success && hexValue != "0")
                {
                    if (ItemAttr.hasQuantity(itemId))
                        await MyTwitchBot.SendMessage($"{displayName}, your order of \"{ReverseDict[id]}\" Quantity: [ {num} ]  have been received!");
                    else
                        await MyTwitchBot.SendMessage($"{displayName}, your order of \"{ReverseDict[id]}\" Variation: [ {hexValue} ]  have been received!");
                }
                else
                {
                    if (ReverseDict.ContainsKey(id))
                        await MyTwitchBot.SendMessage($"{displayName}, your order of \"{ReverseDict[id]}\" have been received!");
                    else
                        await MyTwitchBot.SendMessage($"{displayName}, your order have been received!");
                }

                if (ReverseDict.ContainsKey(id))
                    dodoSetup.WriteLog($"{displayName} | {ReverseDict[id]} | {id} | {hexValue} | {color}", true);
                else
                    dodoSetup.WriteLog($"{displayName} | {id} | {hexValue} | {color}", true);

                string path = Form1.GetImagePathFromID(id, itemSource, Convert.ToUInt32("0x" + hexValue, 16));

                Image image = null;
                if (File.Exists(path))
                    image = Image.FromFile(path);

                dodo.AddItem(displayName, id, hexValue, ReverseDict[id], color, image);
            }
        }

        public static async Task CheckAndAddRecipe(string name, string displayName)
        {
            string ItemId = "16A2";

            string RecipeId = "";
            if (RecipeDict.ContainsKey(name.ToLower()))
            {
                RecipeId = RecipeDict[name.ToLower()];
            }
            else
            {
                DataView dv = new DataView(recipeSource);
                dv.RowFilter = string.Format("eng LIKE '%{0}%'", EscapeLikeValue(name));
                if (dv.Count > 0)
                {
                    DataRowView drv;
                    drv = dv[0];
                    RecipeId = drv["id"].ToString();
                }
                else
                {
                    await MyTwitchBot.SendMessage($"Sorry, I am unable to find an recipe with the name \"{name}\". Are you sure you are using the correct \"English\" name?");
                    return;
                }
            }

            Debug.Print($"ItemId {ItemId} | hexValue {RecipeId}");

            if (!RecipeId.Equals(String.Empty))
            {
                if (RecipeDict.ContainsKey(RecipeId))
                    await MyTwitchBot.SendMessage($"{displayName}, your order of \"{RecipeDict[RecipeId]} recipe\" have been received!");
                else
                    await MyTwitchBot.SendMessage($"{displayName}, your order of recipe have been received!");

                if (RecipeDict.ContainsKey(RecipeId))
                    dodoSetup.WriteLog($"{displayName} | {RecipeDict[RecipeId]} recipe | {RecipeId} ", true);
                else
                    dodoSetup.WriteLog($"{displayName} | {RecipeId}", true);

                dodo.AddItem(displayName, ItemId, RecipeId, RecipeDict[RecipeId] + " recipe", "");
            }
        }

        private async Task CheckAndAddVillager(string name, string displayName, string userInput)
        {
            if (VillagerDict.ContainsKey(name.ToLower()))
            {
                string Iname = VillagerDict[name.ToLower()];

                if (Iname.Equals("cbr18") || Iname.Equals("der10") || Iname.Equals("elp11") || Iname.Equals("gor11") || Iname.Equals("rbt20") || Iname.Equals("shp14"))
                {
                    await MyTwitchBot.SendMessage($"Sorry, but Sanrio Villagers cannot be invited. Amiibo is the only way to get them legitimately.");
                    return;
                }

                string Rname = Utilities.RealName[Iname];
                Image img;
                string path = Utilities.GetVillagerImage(Iname);

                if (!path.Equals(string.Empty))
                    img = Image.FromFile(path);
                else
                    img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));

                await MyTwitchBot.SendMessage($"{displayName}, villager \"{name}\" is now packing up. Please wait for the confirmation before flying in.");
                dodoSetup.WriteLog($"{displayName} | {Iname} | {Rname} ", true);
                dodo.AddVillager(displayName, Iname, Rname, img);
            }
            else
            {
                await MyTwitchBot.SendMessage($"Sorry, I am unable to find a villager with the name \"{userInput}\". Are you sure you are using the correct \"English\" name?");
                return;
            }
        }

        public static string EscapeLikeValue(string valueWithoutWildcards)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < valueWithoutWildcards.Length; i++)
            {
                char c = valueWithoutWildcards[i];
                if (c == '*' || c == '%' || c == '[' || c == ']')
                    sb.Append("[").Append(c).Append("]");
                else if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private DataTable loadItemCSV(string filePath)
        {
            var dt = new DataTable();

            File.ReadLines(filePath).Take(1)
                .SelectMany(x => x.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(x => dt.Columns.Add(x.Trim()));

            File.ReadLines(filePath).Skip(1)
                .Select(x => x.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(line => dt.Rows.Add(line));

            if (dt.Columns.Contains("id"))
                dt.PrimaryKey = new DataColumn[1] { dt.Columns["id"] };

            return dt;
        }

        private Dictionary<string, string> CreateItemDictionary(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        if (!dict.ContainsKey(parts[2].ToLower()))
                            dict.Add(parts[2].ToLower(), parts[0]);
                    }
                    //Debug.Print(parts[0]);
                }
            }

            return dict;
        }

        private Dictionary<string, string> CreateReverseDictionary(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        if (!dict.ContainsKey(parts[0]))
                            dict.Add(parts[0], parts[2].ToLower());
                    }
                }
            }

            return dict;
        }

        private Dictionary<string, string> CreateColorDictionary(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        if (parts.Length > 13 && !dict.ContainsKey(parts[0]))
                            dict.Add(parts[0], parts[13]);
                    }
                    //Debug.Print(parts[0]);
                }
            }

            return dict;
        }

        private Dictionary<string, string> CreateRecipeDictionary(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        dict.Add(parts[0], parts[2]);
                        dict.Add(parts[2], parts[0]);
                    }
                }
            }

            return dict;
        }

        private Dictionary<string, string> CreateVillagerDictionary()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in Utilities.RealName)
            {
                dict.Add(entry.Value.Replace('é', 'e').Replace('É', 'E').ToLower(), entry.Key);
            }

            if (dict.ContainsKey("empty"))
                dict.Remove("empty");

            return dict;
        }

        #endregion

        /*
        #region Outgoing Raid Events

        private void ListenToRaid(string channelId)
        {
            MyPubSub.OnRaidUpdate += PubSub_OnRaidUpdate;
            MyPubSub.OnRaidUpdateV2 += PubSub_OnRaidUpdateV2;
            MyPubSub.OnRaidGo += PubSub_OnRaidGo;
            MyPubSub.ListenToRaid(channelId);
        }

        private void PubSub_OnRaidGo(object sender, OnRaidGoArgs e)
        {
            _logger.Information($"Execute raid for {e.TargetDisplayName}");
        }

        private void PubSub_OnRaidUpdateV2(object sender, OnRaidUpdateV2Args e)
        {
            _logger.Information($"Started raid to {e.TargetDisplayName} with {e.ViewerCount} viewers");
        }

        private void PubSub_OnRaidUpdate(object sender, OnRaidUpdateArgs e)
        {
            _logger.Information($"Started Raid to {e.TargetChannelId} with {e.ViewerCount} viewers will start in {e.RemainingDurationSeconds} seconds");
        }

        #endregion

        #region Prediction Events

        private void ListenToPredictions(string channelId)
        {
            MyPubSub.OnPrediction += PubSub_OnPrediction;
            MyPubSub.ListenToPredictions(channelId);
        }

        private void PubSub_OnPrediction(object sender, OnPredictionArgs e)
        {
            //if (e.Type == PredictionType.EventCreated)
            {
                _logger.Information($"A new prediction has started: {e.Title}");
            }

            //if (e.Type == PredictionType.EventUpdated)
            {
                if (e.Status == PredictionStatus.Active)
                {
                    var winningOutcome = e.Outcomes.First(x => e.WinningOutcomeId.Equals(x.Id));
                    _logger.Information($"Prediction: {e.Status}, {e.Title} => winning: {winningOutcome.Title}({winningOutcome.TotalPoints} points by {winningOutcome.TotalUsers} users)");
                }

                if (e.Status == PredictionStatus.Resolved)
                {
                    var winningOutcome = e.Outcomes.First(x => e.WinningOutcomeId.Equals(x.Id));
                    _logger.Information($"Prediction: {e.Status}, {e.Title} => Won: {winningOutcome.Title}({winningOutcome.TotalPoints} points by {winningOutcome.TotalUsers} users)");
                }
            }
        }

        #endregion

        #region Leaderboard Events

        private void ListenToLeaderboards(string channelId)
        {
            MyPubSub.OnLeaderboardBits += PubSub_OnLeaderboardBits;
            MyPubSub.OnLeaderboardSubs += PubSub_OnLeaderboardSubs;
            MyPubSub.ListenToLeaderboards(channelId);
        }

        private void PubSub_OnLeaderboardSubs(object sender, OnLeaderboardEventArgs e)
        {
            _logger.Information($"Gifted Subs leader board");
            foreach (LeaderBoard leaderBoard in e.TopList)
            {
                _logger.Information($"{leaderBoard.Place}) {leaderBoard.UserId} ({leaderBoard.Score})");
            }
        }

        private void PubSub_OnLeaderboardBits(object sender, OnLeaderboardEventArgs e)
        {
            _logger.Information($"Bits leader board");
            foreach (LeaderBoard leaderBoard in e.TopList)
            {
                _logger.Information($"{leaderBoard.Place}) {leaderBoard.UserId} ({leaderBoard.Score})");
            }
        }

        #endregion

        #region Follow Events

        private void ListenToFollows(string channelId)
        {
            MyPubSub.OnFollow += PubSub_OnFollow;
            MyPubSub.ListenToFollows(channelId);
        }

        private void PubSub_OnFollow(object sender, OnFollowArgs e)
        {
            _logger.Information($"{e.Username} is now following");
        }

        #endregion

        #region Commerce Events

        private void ListenToCommerce(string channelId)
        {
            MyPubSub.OnChannelCommerceReceived += PubSub_OnChannelCommerceReceived;
            MyPubSub.ListenToCommerce(channelId);
        }

        private void PubSub_OnChannelCommerceReceived(object sender, OnChannelCommerceReceivedArgs e)
        {
            _logger.Information($"{e.ItemDescription} => {e.Username}: {e.PurchaseMessage} ");
        }

        #endregion

        #region Moderator Events

        private void ListenToChatModeratorActions(string myTwitchId, string channelId)
        {
            MyPubSub.OnTimeout += PubSub_OnTimeout;
            MyPubSub.OnBan += PubSub_OnBan;
            MyPubSub.OnMessageDeleted += PubSub_OnMessageDeleted;
            MyPubSub.OnUnban += PubSub_OnUnban;
            MyPubSub.OnUntimeout += PubSub_OnUntimeout;
            MyPubSub.OnHost += PubSub_OnHost;
            MyPubSub.OnSubscribersOnly += PubSub_OnSubscribersOnly;
            MyPubSub.OnSubscribersOnlyOff += PubSub_OnSubscribersOnlyOff;
            MyPubSub.OnClear += PubSub_OnClear;
            MyPubSub.OnEmoteOnly += PubSub_OnEmoteOnly;
            MyPubSub.OnEmoteOnlyOff += PubSub_OnEmoteOnlyOff;
            MyPubSub.OnR9kBeta += PubSub_OnR9kBeta;
            MyPubSub.OnR9kBetaOff += PubSub_OnR9kBetaOff;
            MyPubSub.ListenToChatModeratorActions(myTwitchId, channelId);
        }

        private void PubSub_OnR9kBetaOff(object sender, OnR9kBetaOffArgs e)
        {
            _logger.Information($"{e.Moderator} disabled R9K mode");
        }

        private void PubSub_OnR9kBeta(object sender, OnR9kBetaArgs e)
        {
            _logger.Information($"{e.Moderator} enabled R9K mode");
        }

        private void PubSub_OnEmoteOnlyOff(object sender, OnEmoteOnlyOffArgs e)
        {
            _logger.Information($"{e.Moderator} disabled emote only mode");
        }

        private void PubSub_OnEmoteOnly(object sender, OnEmoteOnlyArgs e)
        {
            _logger.Information($"{e.Moderator} enabled emote only mode");
        }

        private void PubSub_OnClear(object sender, OnClearArgs e)
        {
            _logger.Information($"{e.Moderator} cleared the chat");
        }

        private void PubSub_OnSubscribersOnlyOff(object sender, OnSubscribersOnlyOffArgs e)
        {
            _logger.Information($"{e.Moderator} disabled subscriber only mode");
        }

        private void PubSub_OnSubscribersOnly(object sender, OnSubscribersOnlyArgs e)
        {
            _logger.Information($"{e.Moderator} enabled subscriber only mode");
        }

        private void PubSub_OnHost(object sender, OnHostArgs e)
        {
            _logger.Information($"{e.Moderator} started host to {e.HostedChannel}");
        }

        private void PubSub_OnUntimeout(object sender, OnUntimeoutArgs e)
        {
            _logger.Information($"{e.UntimeoutedUser} undid the timeout of {e.UntimeoutedUser}");
        }

        private void PubSub_OnUnban(object sender, OnUnbanArgs e)
        {
            _logger.Information($"{e.UnbannedBy} unbanned {e.UnbannedUser}");
        }

        private void PubSub_OnMessageDeleted(object sender, OnMessageDeletedArgs e)
        {
            _logger.Information($"{e.DeletedBy} deleted the message \"{e.Message}\" from {e.TargetUser}");
        }

        private void PubSub_OnBan(object sender, OnBanArgs e)
        {
            _logger.Information($"{e.BannedBy} banned {e.BannedUser} ({e.BanReason})");
        }

        private void PubSub_OnTimeout(object sender, OnTimeoutArgs e)
        {
            _logger.Information($"{e.TimedoutBy} timed out {e.TimedoutUser} ({e.TimeoutReason}) for {e.TimeoutDuration.Seconds} seconds");
        }

        #endregion

        #region Bits Events

        private void ListenToBits(string channelId)
        {
            MyPubSub.OnBitsReceived += PubSub_OnBitsReceived;
            MyPubSub.ListenToBitsEvents(channelId);
        }

        private void PubSub_OnBitsReceived(object sender, OnBitsReceivedArgs e)
        {
            _logger.Information($"{e.Username} trowed {e.TotalBitsUsed} bits");
        }

        #endregion
        */

        #region Pubsub events

        private void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            Debug.Print($"{e.Exception.Message}");
        }

        private void OnPubSubServiceClosed(object sender, EventArgs e)
        {
            Debug.Print($"Connection closed to pubsub server");
        }

        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            dodoSetup.WriteLog($"Connected to Twitch pubsub server", true);
            MyPubSub.SendTopics(accessToken);
        }

        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
            {
                Debug.Print($"Failed to listen! Response{e.Response}");
            }
        }

        public void Dispose()
        {
            MyPubSub.Disconnect();
        }

        #endregion
    }
}
