using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNhaTro.Data;
using WebNhaTro.Models;

namespace WebNhaTro.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;

    public PostController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Detail(int id)
    {
        var post = await _context.Posts
        .Include(p => p.PostImages)
        .Include(p => p.Owner)
        .FirstOrDefaultAsync(p => p.PostID == id);

        if (post == null)
        {
            return NotFound();
        }
        return View(post);
    }
    public IActionResult EditPost(int id)
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }var post = _context.Posts
        .Include(p => p.PostImages)
        .FirstOrDefault(p => p.PostID == id);
        if (post == null || post.OwnerID != userId)
        {
            return NotFound();
        }
        var model = new PostViewModel
        {
            PostID = post.PostID,
            Title = post.Title,
            Price = post.Price,
            Area = post.Area,
            Address = post.Address,
            RoomType = post.RoomType,
            Description = post.Description ?? "",
            Amenities = post.Amenities,
            ImageUrls = post.PostImages != null 
                    ? post.PostImages.Select(img => img.ImageUrl).ToList() 
                    : new List<string>()
        };

        AddressParts addressParts = HomeController.SplitAddressFromEnd(post.Address);

        ViewBag.Province = addressParts.Province;
        ViewBag.District = addressParts.District;
        ViewBag.Ward = addressParts.Ward;
        ViewBag.Detail = addressParts.Detail;

        return View(model);
    }
    [HttpPost]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePost([FromForm] CreatePostViewModel model)
    {
        int userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }
        if (ModelState.IsValid)
        {
            var existingPost = await _context.Posts
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.PostID == model.PostID);
            if (existingPost == null || existingPost.OwnerID != userId)
            {
                return NotFound();
            }
            existingPost.Title = model.Title;
            existingPost.Price = model.Price;
            existingPost.Area = model.Area;
            existingPost.Address = model.Address;
            existingPost.Description = model.Description;
            existingPost.RoomType = model.RoomType;
            existingPost.Amenities = model.Amenities;
            existingPost.UpdatedDate = DateTime.Now;
            existingPost.Status = "Chờ duyệt";

            await _context.SaveChangesAsync();
            
            // Tạo uploads nếu chưa tồn tại
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Lưu ảnh
            if (model.Images == null || model.Images.Count == 0)
            {
                return RedirectToAction("ListPost", "Manage");
            }
            else
            {
                if (existingPost.PostImages != null && existingPost.PostImages.Count > 0)
                {
                    foreach (var image in existingPost.PostImages)
                    {
                        var filePath = Path.Combine(uploadPath, image.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _context.PostImages.RemoveRange(existingPost.PostImages);
                }    
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
                            PostID = existingPost.PostID,
                            ImageUrl = "/uploads/" + uniqueFileName
                        };

                        await _context.PostImages.AddAsync(postImage);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
        return RedirectToAction("ListPost", "Manage");
    }
}
