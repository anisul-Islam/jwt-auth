using api.Dtos;
using api.Dtos.Pagination;
using api.Dtos.User;
using api.EF_CORE;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class UserService
    {
        private readonly AppDbContext _appDbcontext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;

        public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
        {
            _appDbcontext = context;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<PaginationResult<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize)
        {

            var totalUserCount = await _appDbcontext.Users.CountAsync();

            var users = await _appDbcontext.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
           .Select(u => new UserDto
           {
               UserId = u.UserId,
               Name = u.Name,
               Email = u.Email,
               Address = u.Address,
               Image = u.Image,
               IsAdmin = u.IsAdmin,
               IsBanned = u.IsBanned,
           }).ToListAsync();

            return new PaginationResult<UserDto>
            {
                Items = users,
                TotalCount = totalUserCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }



        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _appDbcontext.Users.FindAsync(userId);
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto newUserData)
        {

            var user = new User
            {
                Name = newUserData.Name,
                Email = newUserData.Email,
                Password = _passwordHasher.HashPassword(null, newUserData.Password),
                Address = newUserData.Address,
                Image = newUserData.Image,
                IsAdmin = newUserData.IsAdmin,
                IsBanned = newUserData.IsBanned,
            };
            _appDbcontext.Users.Add(user);
            await _appDbcontext.SaveChangesAsync();

            var newUserDto = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                Image = user.Image,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
            };
            return newUserDto;

        }

        public async Task<UserDto?> LoginUserAsync(LoginDto loginDto)
        {


            var user = await _appDbcontext.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                Image = user.Image,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
            };

            return userDto;

        }

    }
}