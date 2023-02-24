using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace NewsDogBackgroundService
{
    public static class Extensions
    {
        public static Dictionary<long, SourceGroup> GetChannelsGroupsDictionary(this IConfiguration config)
        {
            var channelsGroupSection = config.GetSection("ChannelsGroups").GetChildren();
            var channelsGroups = channelsGroupSection.AsEnumerable();
            Dictionary<long, SourceGroup> channelGroups = new Dictionary<long, SourceGroup>();
            foreach (var channelGroup in channelsGroups)
            {
                channelGroups.Add(Convert.ToInt64(channelGroup.Key), Enum.Parse<SourceGroup>(channelGroup.Value));
            }

            return channelGroups;
        }
    }
}
