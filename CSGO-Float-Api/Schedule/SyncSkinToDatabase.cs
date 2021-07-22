using Coravel.Invocable;
using CSGO_Float_Api.Database.Repositories;
using CSGO_Float_Api.Models;
using CSGO_Float_Api.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CSGO_Float_Api.Schedule
{
    public class SyncSkinToDatabase : IInvocable
    {
        private readonly ISkinRepository _skinRepository;
        public SyncSkinToDatabase(ISkinRepository skinRepository)
        {
            _skinRepository = skinRepository;
        }

        public Task Invoke()
        {
            var SkinsDone = Server.SkinsDatabase.Where(a => a.Value.Float != 0).ToList();

            SkinsDone.ForEach(a =>
            {
                Skin skin;
                bool remoseSucess = Server.SkinsDatabase.TryRemove(a.Value.param_a, out skin);

                Skin skinDB = _skinRepository.Get(skin.param_a);
                if (skinDB == null) return;

                skinDB.Float = skin.Float;
                skinDB.Pattern = skin.Pattern;
                _skinRepository.Update(skinDB);
                Log.info($"AssedID:{skinDB.param_a} => Float:{skinDB.Float}", ConsoleColor.Green);
            });

            return Task.CompletedTask;
        }
    }
}
