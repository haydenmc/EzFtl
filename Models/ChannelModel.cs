using System.Collections.Generic;

namespace EzFtl.Models
{
    public class ChannelModel
    {
        public int Id { get; set; }
        public string HmacKey { get; set; }
        public string Name { get; set; }
        public IEnumerable<StreamModel> ActiveStreams { get; set; }
    }
}