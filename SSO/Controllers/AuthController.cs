using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SSO.Data;
using SSO.DTO;
using SSO.Models;

namespace SSO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                return Ok(new { Result = "User Registered Successfully" });
            }
            return BadRequest(result.Errors);
        }



        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var token = GenerateJwtToken(user);
                var loginResponse = new LoginResponseDTO
                {
                    Token = token,
                };

                return Ok(loginResponse);
            }
            return Unauthorized("Invalid username or password");
        }

        [HttpPost("generateSSOToken")]
        public async Task<ActionResult<SSOTokenResponseDTO>> GenerateSSOToken()
        {
            try { 
            var userId = User.FindFirstValue("User_Id");
            if (userId == null) return NotFound();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var ssoToken = new SSOToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(), // Generate a unique SSO token
                ExpiryDate = DateTime.UtcNow.AddMinutes(10), // Set an expiration time for the token
                IsUsed = false
            };
            _context.SSOTokens.Add(ssoToken);
            await _context.SaveChangesAsync();
            SSOTokenResponseDTO ssoTokenResponseDTO = new SSOTokenResponseDTO()
            {
                SSOToken = ssoToken.Token
            };
                return Ok(ssoTokenResponseDTO);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur and return a server error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ValidateSSOToken")]
        public async Task<ActionResult<ValidateSSOTokenResponseDTO>> ValidateSSOToken([FromBody] ValidateSSOTokenRequesDTO request)
        {
            try
            {
                var ssoToken=await _context.SSOTokens.FirstOrDefaultAsync(t => t.Token == request.SSOToken);
                if (ssoToken == null || ssoToken.IsUsed || ssoToken.IsExpired)
                {
                    return BadRequest("Invalid or expired SSO token");
                }
                ssoToken.IsUsed = true;
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(ssoToken.UserId);
                if (user == null)
                {
                    return BadRequest("Invalid User");
                }
                var newJwtToken = GenerateJwtToken(user);
                ValidateSSOTokenResponseDTO validateSSOTokenResponseDTO = new ValidateSSOTokenResponseDTO()
                {
                    Token = newJwtToken,
                    UserDetails = new UserDetails()
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName
                    }
                };
                return Ok(validateSSOTokenResponseDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new List<Claim>
                {
                new Claim("User_Id", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
                };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}

