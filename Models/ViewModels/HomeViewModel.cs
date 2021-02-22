using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EzFtl.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ChannelModel> Channels { get; set; }
    }
}