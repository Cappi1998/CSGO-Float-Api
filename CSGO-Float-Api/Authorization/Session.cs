using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSGO_Float_Api
{
    public class Session
    {

        private IHttpContextAccessor _context;
        public Session(IHttpContextAccessor contex)//Contrutor que vai receber o httpcontext
        {
            _context = contex;
        }

        /*
         * 
         *CRUD - Cadastrar/Atualizar/Consultar/Remover - RemoverTodos/Exist
         *
         */

        public void Register(string key, string value)
        {
            _context.HttpContext.Session.SetString(key, value);
        }

        public void Update(string key, string value)
        {
            Remove(key);
            _context.HttpContext.Session.SetString(key, value);
        }

        public void Remove(string key)
        {

            if (Exist(key))
            {
              _context.HttpContext.Session.Remove(key);
            }

        }

        public string Get(string key)
        {
           return  _context.HttpContext.Session.GetString(key);
        }

        public bool Exist(string key)
        {
            if(_context.HttpContext.Session.GetString(key) == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public void RemoveAll()
        {
            _context.HttpContext.Session.Clear();
        }

    }
}
