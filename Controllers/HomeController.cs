using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebNhaTro.Data;
using WebNhaTro.Models;
using Microsoft.EntityFrameworkCore;

namespace WebNhaTro.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? province, string? district, string? ward, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea, int page = 1)
    {
        int pageSize = 6;

        int currentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;
        var favorites = _context.Favorites
                        .Where(f => f.UserID == currentUserId)
                        .Select(f => f.PostID)
                        .ToHashSet();

        var posts = await _context.Posts
            .Where(p => p.Status == "Đã duyệt")
            .OrderByDescending(p => p.CreatedDate)
            .Include(p => p.PostImages) // Nếu có navigation property
            .ToListAsync();

        var postViewModels = posts.Select(p => new PostViewModel
        {
            PostID = p.PostID,
            Title = p.Title,
            Price = p.Price,
            Area = p.Area,
            Address = SplitAddressFromEnd(p.Address).District + ", " + SplitAddressFromEnd(p.Address).Province,
            RoomType = p.RoomType,
            Description = !string.IsNullOrEmpty(p.Description) && p.Description.Length > 160
                            ? p.Description.Substring(0, 160) + "..."
                            : p.Description,
            Amenities = p.Amenities,
            CreatedDate = p.CreatedDate,
            ImageUrls = p.PostImages != null
                        ? p.PostImages.Select(img => img.ImageUrl).Take(3).ToList()
                        : new List<string>(),
            IsFavorited = favorites.Contains(p.PostID)
        });

        // Áp dụng bộ lọc
        if (!string.IsNullOrEmpty(province))
        {
            postViewModels = postViewModels.Where(p => p.Address.Contains(province));
        }
        if (!string.IsNullOrEmpty(district))
        {
            postViewModels = postViewModels.Where(p => p.Address.Contains(district));
        }
        if (!string.IsNullOrEmpty(ward))
        {
            postViewModels = postViewModels.Where(p => p.Address.Contains(ward));
        }
        if (minPrice.HasValue)
        {
            postViewModels = postViewModels.Where(p => p.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            postViewModels = postViewModels.Where(p => p.Price <= maxPrice.Value);
        }
        if (minArea.HasValue)
        {
            postViewModels = postViewModels.Where(p => p.Area >= minArea.Value);
        }
        if (maxArea.HasValue)
        {
            postViewModels = postViewModels.Where(p => p.Area <= maxArea.Value);
        }

        var paginatedPosts = PaginatedList<PostViewModel>.Create(postViewModels.ToList(), page, pageSize);

        return View(paginatedPosts);
    }

    public static AddressParts SplitAddressFromEnd(string fullAddress)
    {
        var parts = fullAddress.Split(',').Select(p => p.Trim()).ToList();

        if (parts.Count < 4)
        {
            return new AddressParts { Detail = fullAddress };
        }

        return new AddressParts
        {
            Province = parts[^1],             // phần cuối
            District = parts[^2],             // phần gần cuối
            Ward = parts[^3],                 // ...
            Detail = string.Join(", ", parts.Take(parts.Count - 3)) // còn lại là chi tiết
        };
    }
    [HttpPost]
    public IActionResult ToggleFavorite(int postId)
    {
        var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var existing = _context.Favorites.FirstOrDefault(f => f.UserID == userId && f.PostID == postId);
        if (existing != null)
        {
            _context.Favorites.Remove(existing);
        }
        else
        {
            var favorite = new Favorite
            {
                UserID = userId,
                PostID = postId,
                CreatedDate = DateTime.Now
            };
            _context.Favorites.Add(favorite);
        }

        _context.SaveChanges();
        return Json(new { success = true });
    }

    public async Task<IActionResult> Favorite(int page = 1)
    {
        int pageSize = 6;

        int? currentUserId = HttpContext.Session.GetInt32("UserID");
        if (currentUserId == null || currentUserId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        // Lấy danh sách PostID yêu thích của người dùng
        var favoritePostIds = await _context.Favorites
            .Where(f => f.UserID == currentUserId)
            .Select(f => f.PostID)
            .ToListAsync();

        // Lấy các bài đăng yêu thích
        var favoritePosts = await _context.Posts
            .Where(p => favoritePostIds.Contains(p.PostID) && p.Status == "Đã duyệt")
            .OrderByDescending(p => p.CreatedDate)
            .Include(p => p.PostImages)
            .ToListAsync();

        // Chuyển sang ViewModel
        var postViewModels = favoritePosts.Select(p => new PostViewModel
        {
            PostID = p.PostID,
            Title = p.Title,
            Price = p.Price,
            Area = p.Area,
            Address = SplitAddressFromEnd(p.Address).District + ", " + SplitAddressFromEnd(p.Address).Province,
            RoomType = p.RoomType,
            Description = !string.IsNullOrEmpty(p.Description) && p.Description.Length > 160
                            ? p.Description.Substring(0, 160) + "..."
                            : p.Description,
            Amenities = p.Amenities,
            CreatedDate = p.CreatedDate,
            ImageUrls = p.PostImages != null
                ? p.PostImages.Select(img => img.ImageUrl).Take(3).ToList()
                : new List<string>(),
            IsFavorited = true
        }).ToList();

        // Phân trang
        var paginatedPosts = PaginatedList<PostViewModel>.Create(postViewModels.AsQueryable(), page, pageSize);

        return View(paginatedPosts);
    }
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
