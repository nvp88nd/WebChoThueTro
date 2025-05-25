using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebNhaTro.Data;
using WebNhaTro.Models;
using Microsoft.EntityFrameworkCore;

namespace WebNhaTro.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public AccountController(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Register()
    {
        return View();
    }
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        string cacheKey = $"login_{model.Phone}";
        if (_cache.TryGetValue($"block_{model.Phone}", out _))
        {
            return Json(new { success = false, message = "Bạn đã đăng nhập sai quá 3 lần, vui lòng thử lại sau 30 phút" });
        }

        var user = await _context.Users.FirstOrDefaultAsync(m => m.Phone == model.Phone);
        if (user == null)
        {
            return FailedLogin(cacheKey, model.Phone);
        }
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return FailedLogin(cacheKey, model.Phone);
        }

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("UserName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(new { success = true, message = "Đăng nhập thành công!", user = user.FullName });
    }
    private IActionResult FailedLogin(string cacheKey, string phone)
    {
        int failedCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return 0;
        });
        failedCount++;
        if(failedCount >= 3)
        {
            _cache.Set($"block_{phone}", true, TimeSpan.FromMinutes(5));
            return Json(new { success = false, message = "Bạn đã đăng nhập sai quá 3 lần, vui lòng thử lại sau 5 phút" });
        }
        _cache.Set(cacheKey, failedCount, TimeSpan.FromMinutes(30));
        return Json(new { success = false, message = $"Số điện thoại hoặc mật khẩu không đúng! Bạn còn {3 - failedCount} lần thử." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        var existingUserByPhone = await _context.Users.FirstOrDefaultAsync(m => m.Phone == model.Phone);
        if (existingUserByPhone != null)
        {
            return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });
        }
        var existingUserByEmail = await _context.Users.FirstOrDefaultAsync(m => m.Email == model.Email);
        if (existingUserByEmail != null)
        {
            return Json(new { success = false, message = "Email đã tồn tại!" });
        }
        if(model.Role != "ChuTro" && model.Role != "NguoiThue")
        {
            return BadRequest(new { success = false, message = "Vai trò không hợp lệ!" });
        }
        var newUser = new User
        {
            FullName = model.FullName,
            Phone = model.Phone,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = model.Role
        };
        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Đăng ký thành công!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser([FromBody] UserViewModel user)
    {
        if (HttpContext.Session.GetInt32("UserID") == 0)
        {
            return BadRequest(new { success = false, message = "Người dùng không hợp lệ!" });
        }
        var existingUser = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID"));
        if (existingUser == null)
        {
            return NotFound(new { success = false, message = "Người dùng không tồn tại!" });
        }

        // Kiểm tra nếu số điện thoại đã tồn tại (ngoài thông tin cũ)
        if (existingUser.Phone != user.Phone)
        {
            var existingUserByPhone = await _context.Users
                .FirstOrDefaultAsync(m => m.Phone == user.Phone && m.UserID != existingUser.UserID);
            if (existingUserByPhone != null)
            {
                return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });
            }
        }

        // Kiểm tra nếu email đã tồn tại (ngoài thông tin cũ)
        if (existingUser.Email != user.Email)
        {
            var existingUserByEmail = await _context.Users
                .FirstOrDefaultAsync(m => m.Email == user.Email && m.UserID != existingUser.UserID);
            if (existingUserByEmail != null)
            {
                return Json(new { success = false, message = "Email đã tồn tại!" });
            }
        }

        existingUser.FullName = user.FullName;
        existingUser.Phone = user.Phone;
        existingUser.Email = user.Email;
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Cập nhật thông tin thành công!" });
    }
    
    public async Task<IActionResult> Profile()
    {
        int? userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null || userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    public IActionResult ChangePassword()
    {
        int? userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null || userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordViewModel model)
    {
        if (HttpContext.Session.GetInt32("UserID") == 0)
        {
            return BadRequest(new { success = false, message = "Người dùng không hợp lệ!" });
        }
        var existingUser = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID"));
        if (existingUser == null)
        {
            return NotFound(new { success = false, message = "Người dùng không tồn tại!" });
        }
        if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, existingUser.PasswordHash))
        {
            return Json(new { success = false, message = "Mật khẩu cũ không đúng!" });
        }
        if (model.NewPassword != model.ConfirmPassword)
        {
            return Json(new { success = false, message = "Mật khẩu mới không khớp!" });
        }
        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Đổi mật khẩu thành công!" });
    }
    public IActionResult EditUser(int id)
    {
        int? adminId = HttpContext.Session.GetInt32("UserID");
        if (adminId == null || adminId == 0)
        {
            return RedirectToAction("Login", "Account");
        }
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            return BadRequest("Chỉ quản trị viên mới có quyền chỉnh sửa người dùng.");
        }
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(new UserViewModel2
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser2([FromBody] UserViewModel2 user)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _context.Users.FindAsync(user.UserID);
            if (existingUser == null)
            {
                return NotFound(new { success = false, message = "Người dùng không tồn tại!" });
            }
            // Kiểm tra nếu số điện thoại đã tồn tại (ngoài thông tin cũ)
            if (existingUser.Phone != user.Phone)
            {
                var existingUserByPhone = await _context.Users
                    .FirstOrDefaultAsync(m => m.Phone == user.Phone && m.UserID != existingUser.UserID);
                if (existingUserByPhone != null)
                {
                    return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });
                }
            }

            // Kiểm tra nếu email đã tồn tại (ngoài thông tin cũ)
            if (existingUser.Email != user.Email)
            {
                var existingUserByEmail = await _context.Users
                    .FirstOrDefaultAsync(m => m.Email == user.Email && m.UserID != existingUser.UserID);
                if (existingUserByEmail != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại!" });
                }
            }

            existingUser.FullName = user.FullName;
            existingUser.Phone = user.Phone;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thông tin thành công!" });
        }
        return BadRequest(new { success = false, message = "Cập nhật thông tin không thành công!" });
    }

    public async Task<IActionResult> DeleteUser(int id)
    {
        if (HttpContext.Session.GetInt32("UserID") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            return BadRequest("Chỉ quản trị viên mới có quyền xóa người dùng.");
        }
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("ListUsers", "Manage");
    }
}
