using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    [PrimaryKey(nameof(Id), nameof(ChannelId))]
    public class Post
    {
        public int Id { get; set; }
        public long ChannelId { get; set; }
        public string Message { get; set; }
        public string ChannelName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool HasMedia { get; set; }
        public SourceGroup Group { get; set; }

    }
}
