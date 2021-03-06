using EzFtl.Models;
using EzFtl.Models.Api;
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
        // Events
        public event EventHandler<ChannelModel> ChannelUpdated;

        // Properties
        private readonly TimeSpan STALE_STREAM_TIMEOUT = TimeSpan.FromSeconds(15);
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
                    string name = channelConfig.GetValue<string>("Name");
                    _channels.Add(channelId, new Channel(channelId, key, name));
                    _logger.LogInformation("Added Channel {channelId}", channelId);
                }
                // TODO: For load testing. Move this into a configuration setting.
                // for (int i = 1; i <= 100; ++i)
                // {
                //     _channels.Add(i, new Channel(i, "aBcDeFgHiJkLmNoPqRsTuVwXyZ123456", $"Channel {i}"));
                //     _logger.LogInformation("Added Channel {channelId}", i);
                // }
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        public IEnumerable<ChannelModel> GetChannels()
        {
            _dataLock.EnterReadLock();
            try
            {
                RemoveStaleStreamsWithLock();
                return _channels.Values.Select(c => c.ToModel());
            }
            finally
            {
                _dataLock.ExitReadLock();
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
                OnChannelUpdated(channel.ToModel());
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
                OnChannelUpdated(channel.ToModel());
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        public void UpdateStreamMetadata(int streamId, StreamMetadataBindingModel metadata)
        {
            _dataLock.EnterWriteLock();
            try
            {
                // TODO: Maybe later we can keep an indexed list, but for now just search
                Stream stream = _channels.SelectMany(c => c.Value.Streams)
                    .Where(s => s.Key == streamId).Select(s => s.Value).FirstOrDefault();
                if (stream == null)
                {
                    throw new ArgumentException("Stream ID does not exist.");
                }
                
                stream.Metadata = metadata;
                stream.LastUpdateTime = DateTimeOffset.Now;
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        public void UpdateStreamPreview(int streamId, bool hasPreview)
        {
            _dataLock.EnterWriteLock();
            try
            {
                // TODO: Maybe later we can keep an indexed list, but for now just search
                Stream stream = _channels.SelectMany(c => c.Value.Streams)
                    .Where(s => s.Key == streamId).Select(s => s.Value).FirstOrDefault();
                if (stream == null)
                {
                    throw new ArgumentException("Stream ID does not exist.");
                }
                
                stream.HasPreview = hasPreview;
                stream.LastUpdateTime = DateTimeOffset.Now;
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }
        }

        private void RemoveStaleStreamsWithLock()
        {
            foreach (var channel in _channels.Values)
            {
                var streamIdsToRemove = channel.Streams
                    .Where(s => 
                        (DateTimeOffset.Now - s.Value.LastUpdateTime) > STALE_STREAM_TIMEOUT)
                    .Select(s => s.Key);
                foreach (var streamId in streamIdsToRemove)
                {
                    channel.Streams.Remove(streamId);
                }
            }
        }

        private void OnChannelUpdated(ChannelModel channel)
        {
            EventHandler<ChannelModel> channelUpdated = ChannelUpdated;

            if (channelUpdated != null)
            {
                channelUpdated(this, channel);
            }
        }

        // Private classes for internal management of stream data
        private class Channel
        {
            public int Id { get; set; }
            public string HmacKey { get; set; }
            public string Name { get; set; }
            public Dictionary<int, Stream> Streams { get; set; }
                = new Dictionary<int, Stream>();
            
            public Channel(int Id, string HmacKey, string Name)
            {
                this.Id = Id;
                this.HmacKey = HmacKey;
                this.Name = Name;
            }

            public ChannelModel ToModel()
            {
                return new ChannelModel()
                {
                    Id = this.Id,
                    HmacKey = this.HmacKey,
                    Name = this.Name,
                    ActiveStreams = this.Streams.Select(s => s.Value.ToModel()),
                };
            }
        }

        private class Stream
        {
            public int Id { get; set; }
            public DateTimeOffset StartedDateTime { get; set; } = DateTimeOffset.Now;

            public StreamMetadataBindingModel Metadata { get; set; }

            public DateTimeOffset LastUpdateTime { get; set; } = DateTimeOffset.Now;

            public bool HasPreview { get; set; } = false;

            public Stream(int Id)
            {
                this.Id = Id;
            }

            public StreamModel ToModel()
            {
                return new StreamModel()
                {
                    Id = this.Id,
                    HasPreview = this.HasPreview,
                };
            }
        }
    }
}