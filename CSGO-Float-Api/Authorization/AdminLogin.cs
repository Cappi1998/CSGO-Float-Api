using CSGO_Float_Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSGO_Float_Api
{
    public class AdminLogin
    {
        private string key = "Admin.Login";

        private Session _session;

        public AdminLogin(Session session)
        {
            _session = session;
        }

        public void Login(Admin admin)
        {
            //Serializar
            string admin_string = JsonConvert.SerializeObject(admin);
            _session.Register(key, admin_string);
        }

        public Admin GetClient()
        {
            if (_session.Exist(key))
            {
                ///Deserializar 
                string admin_Json = _session.Get(key);

                return JsonConvert.DeserializeObject<Admin>(admin_Json);
            }
            else
            {
                return null;
            }
        }

        public void Logout()
        {
            _session.RemoveAll();
        }
    }
}
