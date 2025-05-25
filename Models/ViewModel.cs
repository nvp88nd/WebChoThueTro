using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebNhaTro.Models
{
    public class LoginViewModel
    {
        public required string Phone { get; set; }
        public required string Password { get; set; }
    }
    public class RegisterViewModel
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }
    public class UserViewModel
    {
        public int UserID { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
    }
    public class UserViewModel2
    {
        public int UserID { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Role { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public int UserID { get; set; }
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    public class PostViewModel
    {
        public int PostID { get; set; }
        public required string Title { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public required string Address { get; set; }
        public required string RoomType { get; set; }
        public string? Description { get; set; }
        public string? Amenities { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>(); 
        public bool IsFavorited { get; set; } = false;
    }
    public class AddressParts
    {
        public string Detail { get; set; } = "";
        public string Ward { get; set; } = "";
        public string District { get; set; } = "";
        public string Province { get; set; } = "";
    }
    
    public class CreatePostViewModel
    {
        public int PostID { get; set; }
        public required string Title { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public required string Address { get; set; }
        public required string Description { get; set; }
        public required string RoomType { get; set; }
        public string? Amenities { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

}
