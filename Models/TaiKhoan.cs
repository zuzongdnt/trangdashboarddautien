using System;
using System.Collections.Generic;

namespace quanlytaikhaon.Models;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? VaiTro { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }
}
