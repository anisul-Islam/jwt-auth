using System.Security.Claims;
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

            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
            return ApiResponse.Success(users, "All Users are returned successfully");

        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse.NotFound("Hello: User not found");
            }

            return ApiResponse.Success(user, "User is returned successfully");

        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserData)
        {

            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid User Data");
            }


            var newUser = await _userService.CreateUserAsync(newUserData);
            return ApiResponse.Created(newUser, "User created successfully");

        }




        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto loginDto)
        {

            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid User Data");
            }
            var loggedInUser = await _userService.LoginUserAsync(loginDto);


            var token = _authService.GenerateJwt(loggedInUser);


            return ApiResponse.Success(new { token, loggedInUser }, "User Logged In successfully");

        }

        // api/users/profile
        [Authorize(Roles = "Admin")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile(Guid userId)
        {

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var isAdmin = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            // if (!isAdmin)
            // {
            //     return ApiResponse.Forbidden("Only admin can visit this route");
            // }
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

    }
}