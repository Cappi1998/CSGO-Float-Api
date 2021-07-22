using CSGO_Float_Api.Authorization;
using CSGO_Float_Api.Database.Repositories;
using CSGO_Float_Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SteamAuth;
using System;
using System.Linq;
using X.PagedList;

namespace CSGO_Float_Api.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminLogin _adminLogin;
        private readonly ISteamAccountRepository _steamAccountRepository;
        private IApplicationLifetime _applicationLifetime { get; set; }

        public AdminController(AdminLogin adminLogin, ISteamAccountRepository steamAccountRepository, IApplicationLifetime applicationLifetime)
        {
            _adminLogin = adminLogin;
            _steamAccountRepository = steamAccountRepository;
            _applicationLifetime = applicationLifetime;
        }

        [HttpGet]
        [AdminAuthorizationAttribute]
        public IActionResult Index()
        {
            return View(Program.statsAdmin);
        }
        
        [HttpPost]
        [AdminAuthorizationAttribute]
        [ValidateAntiForgeryToken]
        [ValidateHttpRefererAttribute]
        public IActionResult RestartAplication()
        {
            TempData["MSG_Sucess"] = "Application restarted successfully.";
            System.Diagnostics.Process.Start(Program.ProcessFileName);
            _applicationLifetime.StopApplication();
            return RedirectToAction(nameof(Login));
        }

        #region AddAccount
        [HttpGet]
        [AdminAuthorizationAttribute]
        [ValidateHttpRefererAttribute]
        public IActionResult AddAccount()
        {
            return View();
        }

        [HttpPost]
        [AdminAuthorizationAttribute]
        [ValidateAntiForgeryToken]
        [ValidateHttpRefererAttribute]
        public IActionResult AddAccount(SteamAccount account)
        {
            if (ModelState.IsValid)
            {
                var accDB = _steamAccountRepository.Get(account.Username);

                if(accDB !=null)
                {
                    TempData["MSG_Error"] = "There is already an account with this login in the database!";
                    return View(account);
                }
                else
                {
                    _steamAccountRepository.Add(account);
                    TempData["MSG_Sucess"] = "Account successfully added.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                return View(account);
            }
        }
        #endregion

        #region UpdateAccount
        [HttpGet]
        [AdminAuthorizationAttribute]
        [ValidateHttpRefererAttribute]
        public IActionResult UpdateAccount(string? Username)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                TempData["MSG_Error"] = "Account not found!";
                return RedirectToAction(nameof(Index));
            }

            var AccDB = _steamAccountRepository.Get(Username);
            if(AccDB == null)
            {
                TempData["MSG_Error"] = "Account not found!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(AccDB);
            }
        }

        [HttpPost]
        [AdminAuthorizationAttribute]
        [ValidateAntiForgeryToken]
        [ValidateHttpRefererAttribute]
        public IActionResult UpdateAccount(SteamAccount account)
        {
            if (ModelState.IsValid)
            {
                var accDB = _steamAccountRepository.Get(account.Username);

                if (accDB == null)
                {
                    TempData["MSG_Error"] = "Account not found!";
                    return RedirectToAction(nameof(AccountList));
                }
                else
                {
                    _steamAccountRepository.Update(account);
                    TempData["MSG_Sucess"] = "Account successfully updated.";
                    return RedirectToAction(nameof(AccountList));
                }
            }
            else
            {
                return View(account);
            }
        }
        #endregion

        [HttpGet]
        [AdminAuthorizationAttribute]
        [ValidateHttpRefererAttribute]
        public IActionResult GenGuardCode(string Username)
        {
            SteamAccount account = _steamAccountRepository.Get(Username);

            var code = new SteamGuardAccount();  // Gen 2fA steam 
            code.SharedSecret = account.Shared_secret;
            string GuardCode = code.GenerateSteamGuardCode();

            TempData["Msg_Sucess"] = $"Steam Guard Code successfully generated: {GuardCode}";
            return RedirectToAction(nameof(AccountList));
        }

        [HttpGet]
        [AdminAuthorizationAttribute]
        [ValidateHttpRefererAttribute]
        public IActionResult DeleteAccount(string Username)
        {
            var accDB = _steamAccountRepository.Get(Username);
            if (accDB == null)
            {
                TempData["MSG_Error"] = "Account not found!";
                return RedirectToAction(nameof(AccountList));
            }
            else
            {
                _steamAccountRepository.Delete(accDB);
                TempData["MSG_Sucess"] = "Account successfully deleted.";
                return RedirectToAction(nameof(AccountList));
            }
        }

        [HttpGet]
        [AdminAuthorizationAttribute]
        public IActionResult AccountList(int? page)
        {
            IPagedList<SteamAccount> accs = _steamAccountRepository.GetAllAccounts(page);
            return View(accs);
        }

        [HttpGet]
        [AdminAuthorizationAttribute]
        [ValidateHttpRefererAttribute]
        public IActionResult AddMultipleAccounts()
        {
            return View();
        }

        [HttpPost]
        [AdminAuthorizationAttribute]
        [ValidateAntiForgeryToken]
        [ValidateHttpRefererAttribute]
        public IActionResult AddMultipleAccounts(string list)
        {

            if (!string.IsNullOrWhiteSpace(list))
            {
                var AccountsList  = list.Split(new[] { "\r\n", "\r", "\n", "\n\r" }, StringSplitOptions.None).ToList();
                int SucessCount = 0;
                int FailedCount = 0;
                AccountsList.ForEach(acc =>
                {
                    var accountData = acc.Split(":");

                    if (accountData.Length < 3)
                    {
                        FailedCount++;
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(accountData[0]) || string.IsNullOrWhiteSpace(accountData[1]))
                        {
                            FailedCount++;
                            return;
                        }

                        SteamAccount account = new SteamAccount { Username = accountData[0], Password = accountData[1], Shared_secret= accountData[2] };

                        var accDB = _steamAccountRepository.Get(account.Username);
                        if(accDB == null)
                        {
                            _steamAccountRepository.Add(account);
                        }
                        else
                        {
                            _steamAccountRepository.Update(account);
                        }

                        SucessCount++;
                    }
                });

                TempData["MSG_Sucess"] = $"{SucessCount} Accounts successfully added and {FailedCount} accounts failed and were not added!";
                return RedirectToAction(nameof(AccountList));
            }
            else
            {
                TempData["MSG_Error"] = "Invalid format!";
                return View();
            }
        }

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateHttpRefererAttribute]
        public IActionResult Login(Admin admin)
        {
            if (admin.Email == Program.admin.Email && admin.Password == Program.admin.Password)
            {
                _adminLogin.Logout();
                _adminLogin.Login(admin);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["MSG_Error"] = "Incorrect Credentials!!";
                return View();
            }
        }

        [HttpPost]
        [AdminAuthorizationAttribute]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _adminLogin.Logout();
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
