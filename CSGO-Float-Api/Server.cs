using CSGO_Float_Api.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CSGO_Float_Api
{
    public class Server
    {
        public static Dictionary<string, SteamFloatClient> SteamClients = new Dictionary<string, SteamFloatClient>();
        public static AutoResetEvent QueueGetsAnElementEvent = new AutoResetEvent(false);
        public static ConcurrentQueue<ulong> SkinsQueue = new ConcurrentQueue<ulong>();
        public static ConcurrentDictionary<ulong, Skin> SkinsDatabase = new ConcurrentDictionary<ulong, Skin>();

        public static void AddSkinToQueue(Skin SkinRequested)
        {
            if (SkinsDatabase.TryAdd(SkinRequested.param_a, SkinRequested))
            {
                SkinsQueue.Enqueue(SkinRequested.param_a);
                QueueGetsAnElementEvent.Set();
            }
            else
            {
                return;
            }
        }
        public static SteamFloatClient GetSteamFloatClient(string Username)
        {
            if (Server.SteamClients.ContainsKey(Username))
            {
                return Server.SteamClients[Username];
            }
            else return null;
        }
    }
}
