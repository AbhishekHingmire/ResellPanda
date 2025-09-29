
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using ResellBook.Data;
using ResellBook.Helpers;
using ResellBook.Models;
using ResellBook.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public AuthController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already exists.");

        var user = new User
        { Name=request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailVerificationCode = new Random().Next(100000, 999999).ToString(),
            VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
         user.Email,
         "ResellPanda Email Verification",
         $@"
Hello {user.Name},

Welcome to **ResellPanda**! To complete your registration, please verify your email address by using the OTP below:

**Your OTP:** {user.EmailVerificationCode}

This OTP is valid for 10 minutes. Please do not share it with anyone.

If you did not request this, please ignore this email.

Thank you,  
The ResellPanda Team
"
     );


        return Ok("Signup successful. Check your email for OTP.");
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return BadRequest("Invalid email.");
        if (user.IsEmailVerified) return BadRequest("Already verified.");
        if (user.EmailVerificationCode != request.Code) return BadRequest("Invalid code.");
        if (user.VerificationCodeExpiry < DateTime.UtcNow) return BadRequest("Code expired.");

        user.IsEmailVerified = true;
        user.EmailVerificationCode = request.Code;
        user.VerificationCodeExpiry = null;

        await _context.SaveChangesAsync();
        return Ok("Email verified successfully!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest("Invalid credentials.");
        if (!user.IsEmailVerified) return BadRequest("Email not verified.");

        var token = JwtHelper.GenerateToken(user.Email, user.Id);
        return Ok(new { Token = token });
    }
}

public record SignupRequest(string Name,string Email, string Password);
public record VerifyEmailRequest(string Email, string Code);
public record LoginRequest(string Email, string Password);
