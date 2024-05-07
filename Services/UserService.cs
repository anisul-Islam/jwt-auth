using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Pagination;
using api.Dtos.User;
using api.EF_CORE;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class UserService
    {
        private readonly AppDbContext _appDbcontext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _appDbcontext = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<PaginationResult<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("An error occured");
            }
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _appDbcontext.Users
                .Where(u => u.UserId == userId)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Address = u.Address,
                    Image = u.Image,
                    IsAdmin = u.IsAdmin,
                    IsBanned = u.IsBanned,
                }).FirstOrDefaultAsync();

                return user;
            }
            catch (Exception e)
            {
                throw new Exception("An error occured");
            }
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto newUserData)
        {
            try
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
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Could not save the user to database: ", ex);
            }
        }

        public async Task<UserDto?> LoginUserAsync(LoginDto loginDto)
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("An error occured");
            }
        }

    }
}