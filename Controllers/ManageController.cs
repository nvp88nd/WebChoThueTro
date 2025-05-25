using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebNhaTro.Data;
using WebNhaTro.Models;
using Microsoft.EntityFrameworkCore;

namespace WebNhaTro.Controllers;

public class ManageController : Controller
{
    private readonly ApplicationDbContext _context;

    public ManageController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult NewPost()
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> NewPost([FromForm] CreatePostViewModel model)
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }
        if (ModelState.IsValid)
        {
            var post = new Post
            {
                OwnerID = userId,
                Title = model.Title,
                Price = model.Price,
                Area = model.Area,
                Address = model.Address,
                Description = model.Description,
                RoomType = model.RoomType,
                Amenities = model.Amenities,
                Status = "Chờ duyệt",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            
            // Tạo uploads nếu chưa tồn tại
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Lưu ảnh
            foreach (var file in model.Images)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Lưu bản ghi ảnh vào DB
                    var postImage = new PostImage
                    {
                        PostID = post.PostID,
                        ImageUrl = "/uploads/" + uniqueFileName
                    };

                    await _context.PostImages.AddAsync(postImage);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng bài thành công! Hãy chờ quản trị viên duyệt bài.";
            //return RedirectToAction("ListPost", "Manage");
        }
        return RedirectToAction("ListPost", "Manage");
    }

    public async Task<IActionResult> ListPost()
    {
        var role = HttpContext.Session.GetString("Role");
        var userId = HttpContext.Session.GetInt32("UserID");

        if (string.IsNullOrEmpty(role) || userId is null or 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var postsQuery = _context.Posts.Include(p => p.Owner).AsQueryable();

        if (role == "ChuTro")
        {
            postsQuery = postsQuery.Where(p => p.OwnerID == userId);
        }

        var posts = await postsQuery
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(posts);
    }
    public async Task<IActionResult> Approve(int id)
    {
        if (HttpContext.Session.GetInt32("UserID") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return BadRequest("Chỉ quản trị viên mới có quyền duyệt bài.");
        }
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        post.Status = "Đã duyệt";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Duyệt bài thành công!";
        return RedirectToAction("ListPost", "Manage");
    }
    public async Task<IActionResult> Reject(int id)
    {
        if (HttpContext.Session.GetInt32("UserID") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return BadRequest("Chỉ quản trị viên mới có quyền từ chối bài.");
        }
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        post.Status = "Đã từ chối";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Từ chối bài thành công!";
        return RedirectToAction("ListPost", "Manage");
    }
    
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        var role = HttpContext.Session.GetString("Role");

        if (userId == null || userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var post = await _context.Posts.Include(p => p.PostImages).FirstOrDefaultAsync(p => p.PostID == id);
        if (post == null)
        {
            return NotFound("Bài đăng không tồn tại.");
        }

        // Kiểm tra quyền xóa bài đăng
        if (role == "ChuTro" && post.OwnerID != userId)
        {
            return Forbid("Bạn không có quyền xóa bài đăng này.");
        }

        // Xóa các hình ảnh liên quan
        if (post.PostImages != null && post.PostImages.Any())
        {
            foreach (var image in post.PostImages)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.PostImages.RemoveRange(post.PostImages);
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Xóa bài đăng thành công!";
        return RedirectToAction("ListPost", "Manage");
    }

    public async Task<IActionResult> ListUsers()
    {
        var role = HttpContext.Session.GetString("Role");

        if (string.IsNullOrEmpty(role) || role != "Admin")
        {
            return RedirectToAction("Login", "Account");
        }

        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }
}
