
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // ------------------ SIGNUP -------------------
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsEmailVerified = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate verification OTP
        await CreateAndSendOtp(user, VerificationType.EmailVerification);

        return Ok("Signup successful. Check your email for OTP.");
    }

    // ------------------ RESEND OTP -------------------
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BadRequest("Email not found.");
        if (user.IsEmailVerified) return BadRequest("Email already verified.");

        await CreateAndSendOtp(user, VerificationType.EmailVerification);
        return Ok("OTP resent. Check your email.");
    }

    // ------------------ VERIFY EMAIL -------------------
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return BadRequest("Invalid email.");
        if (user.IsEmailVerified) return BadRequest("Already verified.");

        var verification = await _context.UserVerifications
            .FirstOrDefaultAsync(v => v.UserId == user.Id &&
                                      v.Code == request.Code &&
                                      v.Type == VerificationType.EmailVerification &&
                                      !v.IsUsed);

        if (verification == null || verification.Expiry < DateTime.UtcNow)
            return BadRequest("Invalid or expired OTP.");

        // Mark verified
        user.IsEmailVerified = true;
        verification.IsUsed = true;
        await _context.SaveChangesAsync();

        return Ok("Email verified successfully!");
    }

    // ------------------ LOGIN -------------------
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

    // ------------------ FORGOT PASSWORD -------------------
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BadRequest("Email not found.");

        await CreateAndSendOtp(user, VerificationType.PasswordReset);
        return Ok("Password reset OTP sent to your email.");
    }

    // ------------------ VERIFY RESET OTP -------------------
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return BadRequest("Invalid email.");

        var verification = await _context.UserVerifications
            .FirstOrDefaultAsync(v => v.UserId == user.Id &&
                                      v.Code == request.Code &&
                                      v.Type == VerificationType.PasswordReset &&
                                      !v.IsUsed);

        if (verification == null || verification.Expiry < DateTime.UtcNow)
            return BadRequest("Invalid or expired OTP.");

        verification.IsUsed = true;
        await _context.SaveChangesAsync();

        return Ok("OTP verified successfully. You can now reset your password.");
    }

    // ------------------ RESET PASSWORD -------------------
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return BadRequest("Invalid email.");

        // Ensure OTP was verified before reset
        var verification = await _context.UserVerifications
            .Where(v => v.UserId == user.Id && v.Type == VerificationType.PasswordReset && v.IsUsed)
            .OrderByDescending(v => v.Expiry)
            .FirstOrDefaultAsync();

        if (verification == null) return BadRequest("OTP not verified.");

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok("Password reset successfully!");
    }

    // ------------------ HELPER METHOD -------------------
    private async Task CreateAndSendOtp(User user, VerificationType type)
    {
        var otp = new Random().Next(100000, 999999).ToString();

        var verification = new UserVerification
        {
            UserId = user.Id,
            Code = otp,
            Type = type,
            Expiry = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.UserVerifications.Add(verification);
        await _context.SaveChangesAsync();

        string subject = type == VerificationType.EmailVerification
            ? "ResellPanda Email Verification"
            : "ResellPanda Password Reset";

        string body = type == VerificationType.EmailVerification
            ? $@"Hello {user.Name},

Welcome to **ResellPanda**! Please verify your email using the OTP below:

**Your OTP:** {otp}

This OTP is valid for 10 minutes."
            : $@"Hello {user.Name},

We received a request to reset your password.  
Use the OTP below to reset your password:

**Your OTP:** {otp}

This OTP is valid for 10 minutes.";

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }
}
public record SignupRequest(string Name, string Email, string Password);
public record VerifyEmailRequest(string Email, string Code);
public record LoginRequest(string Email, string Password);
public record ResetPasswordRequest(string Email, string NewPassword);
