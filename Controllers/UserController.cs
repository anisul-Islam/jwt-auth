using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.User;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public UserController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 3)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
                return ApiResponse.Success(users, "All Users are returned successfully");
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse.NotFound("User not found");
                }
                return ApiResponse.Success(user, "User is returned successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return ApiResponse.ServerError("Something went wrong when we fetch the user data");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ApiResponse.BadRequest("Invalid User Data");
                }

                if (newUserData == null)
                {
                    return ApiResponse.BadRequest("Invalid User Data");
                }
                var newUser = _userService.CreateUserAsync(newUserData);
                return ApiResponse.Created(newUser, "User created successfully");

            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse.Conflict("User already exist with this email");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return ApiResponse.ServerError("Something went wrong when we fetch the user data");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ApiResponse.BadRequest("Invalid User Data");
                }
                var loggedInUser = await _userService.LoginUserAsync(loginDto);
                if (loggedInUser == null)
                {
                    return ApiResponse.Unauthorized("Invalid credentials");
                }

                var token = _authService.GenerateJwt(loggedInUser);


                return ApiResponse.Success(new { token, loggedInUser }, "User Logged In successfully");

            }

            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return ApiResponse.ServerError("Something went wrong when we fetch the user data");
            }
        }

        // api/users/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile(Guid userId)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.Claims.Any(c=> c.Type == ClaimTypes.Role && c.Value == "Admin");
                if(!isAdmin){
                    return ApiResponse.Forbidden("Only admin can visit this route");
                }
                Console.WriteLine($"{userIdString}");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return ApiResponse.Unauthorized("User Id is misisng from token");
                }

                if (!Guid.TryParse(userIdString, out userId))
                {
                    return ApiResponse.BadRequest("Invalid User Id");
                }
                var user = await _userService.GetUserByIdAsync(userId);

                return ApiResponse.Success(user, "User profile is returned successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
                return ApiResponse.ServerError("Something went wrong when we fetch the user data");
            }
        }

    }
}