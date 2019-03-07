using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using website.Models;
using website.Services;

namespace website.Controllers
{
    public class HomeController : Controller
    {
        private IPlaysService playsService;

        public HomeController(IPlaysService playsService)
        {
            this.playsService = playsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery]string input)
        {
           var result = await playsService.Select();
           return View(result.Select(r => new SearchResult
           {
               line_id = r.LineId,
               line_number = r.LineNumber,
               speech_number = r.SpeechNumber,
               play_name = r.PlayName,
               speaker = r.Speaker,
               text_entry = r.TextEntry
           }));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
