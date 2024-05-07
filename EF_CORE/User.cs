using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.EF_CORE
{
    [Table("Users")]
    public class User
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Note: Include cautiously
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;// Include only if necessary for the context
        public bool IsBanned { get; set; } = false;// Include only if necessary for the context
        public DateTime CreatedAt { get; set; }
    }
}