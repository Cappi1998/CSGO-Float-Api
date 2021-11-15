using Coravel.Invocable;
using CSGO_Float_Api.Database.Repositories;
using CSGO_Float_Api.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CSGO_Float_Api.SteamFloatClient;

namespace CSGO_Float_Api.Schedule
{
    public class CheckAccountStatus : IInvocable
    {
        private readonly ISteamAccountRepository _steamAccountRepository;
        private readonly ISkinRepository _skinRepository;
        private readonly IFloatRequestRepository _floatRequestRepository;
        public CheckAccountStatus(ISteamAccountRepository steamAccountRepository, ISkinRepository skinRepository, IFloatRequestRepository floatRequestRepository)
        {
            _steamAccountRepository = steamAccountRepository;
            _skinRepository = skinRepository;
            _floatRequestRepository = floatRequestRepository;
        }

        //string = Key -> Username // Int = TryCount
        public static Dictionary<string, int> TryStartCount = new Dictionary<string, int>();

        public Task Invoke()
        {
            var ClientToRestart = Server.SteamClients.Values.Where(a => a.CurrentState == SteamClientState.ErrorWhileConnecting).ToList();

            foreach (var FloatClient in ClientToRestart)
            {
                if (TryStartCount.ContainsKey(FloatClient.UserInfo.Username))
                {
                    Log.error($"Account: {FloatClient.UserInfo.Username} will no longer start as it failed 5 times in a row.");
                    int trycount = TryStartCount[FloatClient.UserInfo.Username];
                    if (trycount >= 4) continue;
                }

                var Account = _steamAccountRepository.Get(FloatClient.UserInfo.Username);

                SteamFloatClient SteamFloatClientAdded = new SteamFloatClient(Account, _steamAccountRepository);
                Server.SteamClients[Account.Username] = SteamFloatClientAdded;

                Task.Run(() => {
                    SteamFloatClientAdded.SteamConnect();
                });

                if (TryStartCount.ContainsKey(FloatClient.UserInfo.Username)) TryStartCount[FloatClient.UserInfo.Username]++;
                else TryStartCount.Add(FloatClient.UserInfo.Username, 1);

                Thread.Sleep(TimeSpan.FromSeconds(60));
            }

            return Task.CompletedTask;
        }
    }
}
