using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebNhaTro.Models
{
    public class Post
    {
        [Key]
        public int PostID { get; set; }
        [ForeignKey("User")]
        public int OwnerID { get; set; }
        public User? Owner { get; set; }
        
        [MaxLength(200)]
        public required string Title { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        [MaxLength(255)]
        public required string Address { get; set; }
        public string? Description { get; set; }
        [MaxLength(50)]
        public required string RoomType { get; set; }
        public string? Amenities { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public ICollection<PostImage>? PostImages { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
    }
}