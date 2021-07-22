using Coravel.Invocable;
using CSGO_Float_Api.Database.Repositories;
using System.Threading.Tasks;

namespace CSGO_Float_Api.Schedule
{
    public class UpdateStatsAdminPage : IInvocable
    {
        private readonly ISteamAccountRepository _steamAccountRepository;
        private readonly ISkinRepository _skinRepository;
        private readonly IFloatRequestRepository _floatRequestRepository;
        public UpdateStatsAdminPage(ISteamAccountRepository steamAccountRepository, ISkinRepository skinRepository, IFloatRequestRepository floatRequestRepository)
        {
            _steamAccountRepository = steamAccountRepository;
            _skinRepository = skinRepository;
            _floatRequestRepository = floatRequestRepository;
        }

        public Task Invoke()
        {
            Program.statsAdmin.SkinsOnDB = _skinRepository.GetCount();
            Program.statsAdmin.AccountsOnDB = _steamAccountRepository.GetCount();
            Program.statsAdmin.RequestsOnDB = _floatRequestRepository.GetCount();

            return Task.CompletedTask;
        }
    }
}
