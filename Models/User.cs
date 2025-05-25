using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebNhaTro.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [MaxLength(100)]
        public required string FullName { get; set; }
        [MaxLength(20)]
        public required string Phone { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        [MaxLength(255)]
        public required string Email { get; set; }

        public ICollection<Post>? Posts { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
    }
}
