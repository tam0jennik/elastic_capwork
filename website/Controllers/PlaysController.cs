using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using website.DataLayer.Models;
using website.Services;

namespace website.Controllers
{
    public class PlaysController : Controller
    {
        private IPlaysService playsService;

        public PlaysController()
        {
            this.playsService = playsService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Post(PlayModel value)
        {
            await playsService.Create(value);
            return RedirectToAction("Index", "Home");
        }
   }
}
