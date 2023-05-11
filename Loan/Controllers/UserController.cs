using Loan.Core.Entities;
using Loan.Models;
using Loan.Service.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;


namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SignInManager<Core.Entities.User> _signInManager;
        private readonly UserManager<Core.Entities.User> _userManager;
        private readonly IConfiguration _configuration;

        private readonly ILogger<UserController> _logger;
        public UserController(SignInManager<Core.Entities.User> signInManager,
                              UserManager<Core.Entities.User> userManager,
                              IConfiguration configuration,
                              ILogger<UserController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.Username);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                    if (result.Succeeded)
                    {
                        var jwtHelper = new JwtHelper(_configuration, _userManager);
                        var token = await jwtHelper.GenerateToken(user);
                        return Ok(new { token });
                    }
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logger.LogError(ex, "An error occurred during registration");

                var response = new
                {
                    error = "An error occurred during login",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                var user = new Core.Entities.User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Username,
                    Name = registerDto.Name,
                    LastName = registerDto.LastName,
                    Age = registerDto.Age,
                    MonthlyIncome = registerDto.MonthlyIncome
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
                    if (roleResult.Succeeded)
                    {
                        return Ok(new { message = "User registered successfully" });
                    }
                    else
                    {
                        await _userManager.DeleteAsync(user);
                        return BadRequest(roleResult.Errors);
                    }
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logger.LogError(ex, "An error occurred during registration");

                var response = new
                {
                    error = "An error occurred during registration",
                    message = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
