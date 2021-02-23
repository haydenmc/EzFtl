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
using Microsoft.Extensions.Configuration;

namespace EzFtl.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly StreamManagerService _streamManager;

        private string _janusUri;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger,
            StreamManagerService streamManager)
        {
            _logger = logger;
            _streamManager = streamManager;

            var config = (IConfigurationRoot)configuration;
            _janusUri = config.GetValue<string>("JanusUri", "http://localhost:8088/janus");
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
                var channel = _streamManager.GetChannel(channelId);
                var posterImageUri = channel.ActiveStreams.Any(s => s.HasPreview) ? 
                    Url.Action("Preview", new { streamId = channel.Id }) : "";
                var viewModel = new ChannelViewModel()
                {
                    ChannelId = channel.Id,
                    ChannelName = channel.Name,
                    JanusUri = _janusUri,
                    PosterImageUri = posterImageUri,
                };
                return View(viewModel);
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
