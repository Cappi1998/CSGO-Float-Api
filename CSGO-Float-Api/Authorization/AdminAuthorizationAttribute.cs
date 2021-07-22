using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CSGO_Float_Api.Models;

namespace CSGO_Float_Api
{
    /*
     * Tipos de Filtros:
     * - Autorização
     * - Recurso
     * - Acão 
     * - Exceção
     * - Resultado
     * 
     */

    public class AdminAuthorizationAttribute : Attribute, IAuthorizationFilter
    {

        AdminLogin _adminLogin;

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            _adminLogin = (AdminLogin)context.HttpContext.RequestServices.GetService(typeof(AdminLogin));

            Admin admin = _adminLogin.GetClient();

            if (admin == null)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", new { area = "" });
            }

        }
    }
}
