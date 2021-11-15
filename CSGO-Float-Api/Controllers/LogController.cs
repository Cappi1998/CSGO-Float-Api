using CSGO_Float_Api;
using CSGO_Float_Api.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSGO_Float_Api.Controllers
{
    [AdminAuthorizationAttribute]
    public class LogController : Controller
    {
        public IActionResult Index()
        {
            List<string> logToShow = new List<string>();

            if (System.IO.File.Exists(Program.LogFile_Path))
            {
                logToShow = System.IO.File.ReadAllLines(Program.LogFile_Path).Reverse().ToList();
            }

            ViewBag.date = new DateTime();
            return View(logToShow);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ValidateHttpReferer]
        public IActionResult Index(string filePath, DateTime date, string Delete, string Download, string DeleteALL)
        {
            List<string> logToShow = new List<string>();

            if (Delete != null)
            {
                System.IO.File.Delete(filePath);
                ViewBag.FileSelected = filePath;

                ViewBag.date = date;
                return View(logToShow);
            }
            else if(DeleteALL != null)
            {
                System.IO.File.Delete(Program.LogFile_Path);
                System.IO.File.Delete(Program.ErrorLogFile_Path);

                ViewBag.date = date;
                return View(logToShow);
            }
            else if(Download != null)
            {
                if (System.IO.File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                    return File(fileBytes, "application/force-download", fileName);
                }
            }
            else
            {
                if (System.IO.File.Exists(filePath))
                {
                    logToShow = System.IO.File.ReadAllLines(filePath).Reverse().ToList();
                }

                var data = date.Date.ToShortDateString();
                if (data != new DateTime().Date.ToShortDateString())
                {
                    logToShow = logToShow.Where(a => a.Contains(data)).Reverse().ToList();
                }

                ViewBag.FileSelected = filePath;
                ViewBag.date = date;
                return View(logToShow);
            }

            ViewBag.date = new DateTime();
            return View(logToShow);
        }
    }
}
