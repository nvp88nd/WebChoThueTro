using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebNhaTro.Models
{
    public class Favorite
    {
        [ForeignKey("User")]
        public int UserID { get; set; }
        public User? User { get; set; }

        [ForeignKey("Post")]
        public int PostID { get; set; }
        public Post? Post { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}