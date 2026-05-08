using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using quanlytaikhaon.Models;

namespace quanlytaikhaon.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyDangNhapContext _context;

        public TaiKhoanController(QuanLyDangNhapContext context)
        {
            _context = context;
        }

        // ==========================================
        // CRUD SCAFFOLD (tự sinh)
        // ==========================================

        // GET: TaiKhoan
        public async Task<IActionResult> Index()
        {
            return View(await _context.TaiKhoans.ToListAsync());
        }

        // GET: TaiKhoan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // GET: TaiKhoan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TaiKhoan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTaiKhoan,TenDangNhap,MatKhauHash,HoTen,Email,VaiTro,TrangThai,NgayTao")] TaiKhoan taiKhoan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // POST: TaiKhoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTaiKhoan,TenDangNhap,MatKhauHash,HoTen,Email,VaiTro,TrangThai,NgayTao")] TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taiKhoan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(taiKhoan.MaTaiKhoan))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: TaiKhoan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // POST: TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan != null)
            {
                _context.TaiKhoans.Remove(taiKhoan);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoans.Any(e => e.MaTaiKhoan == id);
        }

        // ==========================================
        // ĐĂNG NHẬP / ĐĂNG KÝ / DASHBOARD / LOGOUT
        // ==========================================

        // GET: TaiKhoan/Login
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì redirect luôn
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Admin")
                return RedirectToAction("Dashboard");
            if (role == "User")
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: TaiKhoan/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Tìm user theo tên đăng nhập
            var user = _context.TaiKhoans
                .FirstOrDefault(u => u.TenDangNhap == model.Username);

            // Kiểm tra user tồn tại & mật khẩu đúng
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.MatKhauHash))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Kiểm tra tài khoản còn hoạt động
        if (user.TrangThai == false)
                {
                    ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                    return View(model);
                }

            // Lưu thông tin vào Session
            HttpContext.Session.SetString("UserRole", user.VaiTro);
            HttpContext.Session.SetString("UserName", user.TenDangNhap);
            HttpContext.Session.SetString("HoTen", user.HoTen ?? user.TenDangNhap);
            HttpContext.Session.SetInt32("UserId", user.MaTaiKhoan);

            // Redirect theo role
            if (user.VaiTro == "Admin")
                return RedirectToAction("Dashboard", "TaiKhoan");

            return RedirectToAction("Index", "Home");
        }

        // GET: TaiKhoan/Dashboard — chỉ Admin mới vào được
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login");       // chưa đăng nhập

            if (role != "Admin")
                return RedirectToAction("Index", "Home"); // không phải admin

            return View();
        }

        // GET: TaiKhoan/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: TaiKhoan/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Kiểm tra trùng tên đăng nhập
            var isExist = _context.TaiKhoans.Any(x => x.TenDangNhap == model.TenDangNhap);
            if (isExist)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại!");
                return View(model);
            }

            // Kiểm tra trùng email
            var emailExist = _context.TaiKhoans.Any(x => x.Email == model.Email);
            if (emailExist)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(model);
            }

            // Tạo tài khoản mới
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap!,
                HoTen       = model.HoTen,
                Email       = model.Email,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                VaiTro      = "User",
                TrangThai   = true,
                NgayTao     = DateTime.Now
            };

            _context.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // Đăng ký xong → về trang Login
            return RedirectToAction("Login");
        }

        // GET: TaiKhoan/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
