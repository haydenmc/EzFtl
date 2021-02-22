using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EzFtl.Models;
using EzFtl.Services;
using EzFtl.Models.ViewModels;

namespace EzFtl.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly StreamManagerService _streamManager;

        public HomeController(ILogger<HomeController> logger, StreamManagerService streamManager)
        {
            _logger = logger;
            _streamManager = streamManager;
        }

        [Route("")]
        public IActionResult Index()
        {
            var pageModel = new HomeViewModel()
            {
                Channels = _streamManager.GetChannels(),
            };
            return View(pageModel);
        }

        [Route("{channelId}")]
        public IActionResult Channel(int channelId)
        {
            try
            {
                return View(_streamManager.GetChannel(channelId));
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        [Route("{streamId}/preview")]
        public IActionResult Preview(int streamId)
        {
            var previewImagePath = AppContext.BaseDirectory + $"/data/previews/{streamId}.jpg";
            if (System.IO.File.Exists(previewImagePath))
            {
                return File(System.IO.File.OpenRead(previewImagePath), "image/jpeg");
            }
            return NotFound();
        }
    }
}
