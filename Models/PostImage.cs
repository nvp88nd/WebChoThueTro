using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebNhaTro.Models
{
    public class PostImage
    {
        [Key]
        public int ImageID { get; set; }

        [ForeignKey("Post")]
        public int PostID { get; set; }
        public Post? Post { get; set; }

        [MaxLength(255)]
        public required string ImageUrl { get; set; }
    }
}