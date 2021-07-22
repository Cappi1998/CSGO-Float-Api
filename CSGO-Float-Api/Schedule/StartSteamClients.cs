using Coravel.Invocable;
using CSGO_Float_Api.Database.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSGO_Float_Api.Schedule
{
    public class StartSteamClients : IInvocable
    {
        private ISteamAccountRepository _steamAccountRepository;
        public StartSteamClients(ISteamAccountRepository steamAccountRepository)
        {
            _steamAccountRepository = steamAccountRepository;
        }

        public Task Invoke()
        {
            var Accounts = _steamAccountRepository.GetAllAccounts();

            Accounts.ForEach(Account =>
            {
                if (Server.SteamClients.ContainsKey(Account.Username)) return;

                SteamFloatClient SteamFloatClientAdded = new SteamFloatClient(Account, _steamAccountRepository);
                Server.SteamClients.Add(Account.Username, SteamFloatClientAdded);

                Task.Run(() => {
                    SteamFloatClientAdded.SteamConnect();
                });
                Thread.Sleep(TimeSpan.FromSeconds(5));
            });

            return Task.CompletedTask;
        }
    }
}
