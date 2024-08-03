using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ReenbitNetCamp.Models
{
    public class ChatContext : DbContext
    {
        public ChatContext()
        {
        }

        public ChatContext(DbContextOptions<ChatContext> options)
        : base(options)
        {
        }

        public virtual DbSet<Message> Messages { get; set; }
    }
}
