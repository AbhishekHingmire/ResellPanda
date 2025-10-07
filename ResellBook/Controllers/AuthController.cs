
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Helpers;
using ResellBook.Models;
using ResellBook.Services;
using ResellBook.Utils;

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
        // Input validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");
        if (request.Password.Length < 4)
            return BadRequest("Password must be at least 4 characters.");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsEmailVerified = false
        };
        SimpleLogger.LogNormal("AuthController", "Signup", $"Signup Attempted");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        Console.WriteLine($"New user created: {user.Email}, ID: {user.Id}");
        SimpleLogger.LogNormal("AuthController", "Signup", $"Account Created");
        // Generate verification OTP
        await CreateAndSendOtp(user, VerificationType.EmailVerification);

        return Ok("Signup successful. Check your email for OTP.");
    }

    // ------------------ RESEND OTP -------------------
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BadRequest("Email not found.");
        if (user.IsEmailVerified) return BadRequest("Email already verified.");

        Console.WriteLine($"Resending OTP for email: {email}");

        await CreateAndSendOtp(user, VerificationType.EmailVerification);
        return Ok("OTP resent. Check your email.");
    }

    // ------------------ VERIFY EMAIL -------------------
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        // Validate request
        var validationError = ValidateVerifyEmailRequest(request);
        if (validationError != null) return validationError;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return BadRequest("Invalid email.");
        if (user.IsEmailVerified) return BadRequest("Already verified.");

        // Log verification attempt
        Console.WriteLine($"Verifying email for {request.Email} with OTP: {request.Code}");

        // Get all active verifications for debugging
        var allVerifications = await _context.UserVerifications
            .Where(v => v.UserId == user.Id && v.Type == VerificationType.EmailVerification && !v.IsUsed)
            .ToListAsync();

        Console.WriteLine($"Found {allVerifications.Count} active verifications for user {user.Email}:");
        foreach (var v in allVerifications)
        {
            Console.WriteLine($"- OTP: {v.Code}, Expiry: {IndianTimeHelper.ToIndianFormat(v.Expiry)}, IsExpired: {IndianTimeHelper.IsExpired(v.Expiry)}");
        }

        var verification = await _context.UserVerifications
            .FirstOrDefaultAsync(v => v.UserId == user.Id &&
                                      v.Code == request.Code &&
                                      v.Type == VerificationType.EmailVerification &&
                                      !v.IsUsed);

        if (verification == null)
        {
            Console.WriteLine($"No matching verification found for OTP: {request.Code}");
            return BadRequest("Invalid OTP.");
        }

        if (IndianTimeHelper.IsExpired(verification.Expiry))
        {
            Console.WriteLine($"OTP expired. Expiry: {IndianTimeHelper.ToIndianFormat(verification.Expiry)}, Current: {IndianTimeHelper.ToIndianFormat(IndianTimeHelper.UtcNow)}");
            return BadRequest("Expired OTP.");
        }

        // Mark verified
        user.IsEmailVerified = true;
        verification.IsUsed = true;
        await _context.SaveChangesAsync();

        Console.WriteLine($"Email verified successfully for {user.Email}");
        return Ok("Email verified successfully!");
    }

    // ------------------ LOGIN -------------------
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            SimpleLogger.LogNormal("AuthController", "Login", $"Login attempt for email: {request.Email}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                SimpleLogger.LogCritical("AuthController", "Login", $"Failed login attempt for email: {request.Email}");
                return BadRequest("Invalid credentials.");
            }
            
            if (!user.IsEmailVerified) 
            {
                SimpleLogger.LogCritical("AuthController", "Login", $"Unverified email login attempt: {request.Email}", null, user.Id.ToString());
                return BadRequest("Email not verified.");
            }

            var token = JwtHelper.GenerateToken(user.Email, user.Id);
            SimpleLogger.LogNormal("AuthController", "Login", "Login successful", user.Id.ToString());
            
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("AuthController", "Login", "Login method error", ex);
            return StatusCode(500, "Login failed");
        }
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
        // Validate request
        var validationError = ValidateVerifyEmailRequest(request);
        if (validationError != null) return validationError;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return BadRequest("Invalid email.");

        // Log verification attempt
        Console.WriteLine($"Verifying reset OTP for {request.Email} with OTP: {request.Code}");

        // Get all active password reset verifications for debugging
        var allVerifications = await _context.UserVerifications
            .Where(v => v.UserId == user.Id && v.Type == VerificationType.PasswordReset && !v.IsUsed)
            .ToListAsync();

        Console.WriteLine($"Found {allVerifications.Count} active reset verifications for user {user.Email}:");
        foreach (var v in allVerifications)
        {
            Console.WriteLine($"- OTP: {v.Code}, Expiry: {IndianTimeHelper.ToIndianFormat(v.Expiry)}, IsExpired: {IndianTimeHelper.IsExpired(v.Expiry)}");
        }

        var verification = await _context.UserVerifications
            .FirstOrDefaultAsync(v => v.UserId == user.Id &&
                                      v.Code == request.Code &&
                                      v.Type == VerificationType.PasswordReset &&
                                      !v.IsUsed);

        if (verification == null)
        {
            Console.WriteLine($"No matching reset verification found for OTP: {request.Code}");
            return BadRequest("Invalid OTP.");
        }

        if (IndianTimeHelper.IsExpired(verification.Expiry))
        {
            Console.WriteLine($"Reset OTP expired. Expiry: {IndianTimeHelper.ToIndianFormat(verification.Expiry)}, Current: {IndianTimeHelper.ToIndianFormat(IndianTimeHelper.UtcNow)}");
            return BadRequest("Expired OTP.");
        }

        verification.IsUsed = true;
        await _context.SaveChangesAsync();

        Console.WriteLine($"Reset OTP verified successfully for {user.Email}");
        return Ok("OTP verified successfully. You can now reset your password.");
    }

    // ------------------ RESET PASSWORD -------------------
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // Validate request
        var validationError = ValidateResetPasswordRequest(request);
        if (validationError != null) return validationError;

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
        // First, invalidate any existing unused OTPs for this user and type
        var existingOtps = await _context.UserVerifications
            .Where(v => v.UserId == user.Id && 
                       v.Type == type && 
                       !v.IsUsed)
            .ToListAsync();

        foreach (var existingOtp in existingOtps)
        {
            existingOtp.IsUsed = true; // Mark as used to invalidate
        }

        // Generate new OTP
        var otp = new Random().Next(100000, 999999).ToString();

        var verification = new UserVerification
        {
            UserId = user.Id,
            Code = otp,
            Type = type,
            Expiry = IndianTimeHelper.AddMinutesToNow(10),
            IsUsed = false
        };

        _context.UserVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Log for debugging
        Console.WriteLine($"Generated OTP: {otp} for User: {user.Email}, Type: {type}");

        string subject = type == VerificationType.EmailVerification
            ? "ResellPanda Email Verification"
            : "ResellPanda Password Reset";

        string body = type == VerificationType.EmailVerification
            ? $@"Hello {user.Name},

Welcome to ResellPanda. Please verify your email using the OTP below.

Your OTP: {otp}

This OTP is valid for 10 minutes."
            : $@"Hello {user.Name},

We received a request to reset your password.  
Use the OTP below to reset your password:

Your OTP: {otp}

This OTP is valid for 10 minutes.";

        try
        {
            await _emailService.SendEmailAsync(user.Email, subject, body);
            Console.WriteLine($"Email sent successfully to {user.Email} with OTP: {otp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            throw;
        }
    }
    
    // ------------------ VALIDATION HELPER -------------------
    private IActionResult? ValidateVerifyEmailRequest(VerifyEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("OTP code is required.");
        if (request.Code.Length != 6)
            return BadRequest("OTP code must be 6 digits.");
        if (!request.Code.All(char.IsDigit))
            return BadRequest("OTP code must contain only digits.");
        return null;
    }

    private IActionResult? ValidateResetPasswordRequest(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("New password is required.");
        if (request.NewPassword.Length < 6)
            return BadRequest("Password must be at least 6 characters.");
        return null;
    }
}

public record SignupRequest(string Name, string Email, string Password);
public record VerifyEmailRequest(string Email, string Code);
public record LoginRequest(string Email, string Password);
public record ResetPasswordRequest(string Email, string NewPassword);