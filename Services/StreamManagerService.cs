using EzFtl.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EzFtl.Services
{
    public class StreamManagerService
    {
        private ILogger<StreamManagerService> _logger;
        private ReaderWriterLockSlim _dataLock = new ReaderWriterLockSlim();
        private int _lastAssignedStreamId = 0;
        private Dictionary<int, Channel> _channels = new Dictionary<int, Channel>();

        public StreamManagerService(ILogger<StreamManagerService> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            // Parse configuration values to populate channels
            var config = (IConfigurationRoot)configuration;
            var channelsConfig = config.GetSection("Channels");
            _dataLock.EnterWriteLock();
            try
            {
                foreach (var channelConfig in channelsConfig.GetChildren())
                {
                    int channelId = channelConfig.GetValue<int>("Id");
                    string key = channelConfig.GetValue<string>("Key");
                    _channels.Add(channelId, new Channel(channelId, key));
                    _logger.LogInformation("Added Channel {channelId}", channelId);
                }
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        public ChannelModel GetChannel(int channelId)
        {
            _dataLock.EnterReadLock();
            try
            {
                if (!(_channels.ContainsKey(channelId)))
                {
                    throw new ArgumentException("Channel ID does not exist.");
                }

                return _channels.GetValueOrDefault(channelId).ToModel();
            }
            finally
            {
                _dataLock.ExitReadLock();
            }
        }

        public int AddStream(int channelId)
        {
            _dataLock.EnterWriteLock();
            try
            {
                if (!(_channels.ContainsKey(channelId)))
                {
                    throw new ArgumentException("Channel ID does not exist.");
                }
                Channel channel = _channels.GetValueOrDefault(channelId);
                if (channel.Streams.Count > 0)
                {
                    throw new ArgumentException("Channel already has active stream.");
                }

                int assignedStreamId = ++_lastAssignedStreamId;
                channel.Streams.Add(assignedStreamId, new Stream(assignedStreamId));
                return assignedStreamId;
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        public void RemoveStream(int streamId)
        {
            _dataLock.EnterWriteLock();
            try
            {
                // TODO: Maybe later we can keep an indexed list, but for now just search
                Channel channel = null;
                foreach (var channelPair in _channels)
                {
                    if (channelPair.Value.Streams.Any(s => s.Key == streamId))
                    {
                        channel = channelPair.Value;
                        break;
                    }
                }
                if (channel == null)
                {
                    throw new ArgumentException("Stream ID does not exist.");
                }
                
                channel.Streams.Remove(streamId);
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        // Private classes for internal management of stream data
        private class Channel
        {
            public int Id { get; set; }
            public string HmacKey { get; set; }
            public Dictionary<int, Stream> Streams { get; set; }
                = new Dictionary<int, Stream>();
            
            public Channel(int Id, string HmacKey)
            {
                this.Id = Id;
                this.HmacKey = HmacKey;
            }

            public ChannelModel ToModel()
            {
                return new ChannelModel()
                {
                    Id = this.Id,
                    HmacKey = this.HmacKey,
                };
            }
        }

        private class Stream
        {
            public int Id { get; set; }
            public DateTimeOffset StartedDateTime { get; set; }
                = DateTimeOffset.Now;

            public Stream(int Id)
            {
                this.Id = Id;
            }

            public StreamModel ToModel()
            {
                return new StreamModel()
                {
                    Id = this.Id,
                };
            }
        }
    }
}