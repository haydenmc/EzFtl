using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EzFtl.Models;
using EzFtl.Models.Api;
using EzFtl.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EzFtl.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly ILogger<ApiController> _logger;

        private readonly StreamManagerService _streamManager;

        public ApiController(ILogger<ApiController> logger, StreamManagerService streamManager)
        {
            _logger = logger;
            _streamManager = streamManager;
        }

        [HttpGet]
        [Route("hmac/{channelId}")]
        public ActionResult<HmacKeyResponseModel> GetHmacKey(string channelId)
        {
            int parsedChannelId;
            if (!(int.TryParse(channelId, out parsedChannelId)))
            {
                _logger.LogInformation("HMAC request failed: ID {channel} is not an integer",
                    channelId);
                return BadRequest("Channel ID must be an integer value.");
            }
            try
            {
                ChannelModel channelModel = _streamManager.GetChannel(parsedChannelId);
                _logger.LogInformation("HMAC key returned for Channel {channel}", parsedChannelId);
                return Ok(new HmacKeyResponseModel()
                {
                    HmacKey = channelModel.HmacKey,
                });
            }
            catch (ArgumentException)
            {
                _logger.LogInformation("HMAC request failed: Channel {channel} not found",
                    parsedChannelId);
                return BadRequest("Channel with given ID could not be found.");
            }
        }

        [HttpPost]
        [Route("start/{channelId}")]
        public ActionResult<StartStreamResponseModel> PostStartStream(string channelId)
        {
            int parsedChannelId;
            if (!(int.TryParse(channelId, out parsedChannelId)))
            {
                _logger.LogInformation("Start request failed: ID {channel} is not an integer",
                    channelId);
                return BadRequest("Channel ID must be an integer value.");
            }
            try
            {
                int streamId = _streamManager.AddStream(parsedChannelId);
                _logger.LogInformation("Stream {stream} started for Channel {channel}", streamId,
                    parsedChannelId);
                return Ok(new StartStreamResponseModel()
                {
                    StreamId = streamId.ToString(),
                });
            }
            catch (ArgumentException e)
            {
                _logger.LogInformation("Start request failed: {reason}",
                    e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("metadata/{streamId}")]
        public IActionResult PostUpdateMetadata(string streamId,
            [FromBody] StreamMetadataBindingModel metadata)
        {
            int parsedStreamId;
            if (!(int.TryParse(streamId, out parsedStreamId)))
            {
                _logger.LogInformation("Metadata request failed: ID {stream} is not an integer",
                    streamId);
                return BadRequest("Stream ID must be an integer value.");
            }
            try
            {
                _streamManager.UpdateStreamMetadata(parsedStreamId, metadata);
                _logger.LogInformation("Stream {stream} metadata updated", parsedStreamId);
                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogInformation("Metadata request failed: {reason}",
                    e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("end/{streamId}")]
        public IActionResult PostEndStream(string streamId)
        {
            int parsedStreamId;
            if (!(int.TryParse(streamId, out parsedStreamId)))
            {
                _logger.LogInformation("End request failed: ID {stream} is not an integer",
                    streamId);
                return BadRequest("Stream ID must be an integer value.");
            }
            try
            {
                _streamManager.RemoveStream(parsedStreamId);
                _logger.LogInformation("Stream {stream} ended", parsedStreamId);
                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogInformation("End request failed: {reason}",
                    e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("preview/{streamId}")]
        public async Task<IActionResult> PostPreview(string streamId,
            [FromForm] IFormFile thumbdata)
        {
            // Parse stream ID
            int parsedStreamId;
            if (!(int.TryParse(streamId, out parsedStreamId)))
            {
                _logger.LogInformation("Preview request failed: ID {stream} is not an integer",
                    streamId);
                return BadRequest("Stream ID must be an integer value.");
            }

            // Write file to disk
            var targetDir = Directory.CreateDirectory(AppContext.BaseDirectory + "/data/previews");
            using (FileStream s = System.IO.File.Create(
                targetDir.FullName + $"/{parsedStreamId}.jpg"))
            {
                await thumbdata.OpenReadStream().CopyToAsync(s);
            }

            // Update stream store
            try
            {
                _streamManager.UpdateStreamPreview(parsedStreamId, true);
                _logger.LogInformation("Preview image received for stream {streamId}", streamId);
                return Ok();
            }
            catch (ArgumentException e)
            {
                _logger.LogInformation("Preview request failed: {reason}",
                    e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}