using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workshop.Data;
using Workshop.DTOs.Response;
using Workshop.Model;
using Workshop.Service.Interface;

namespace Workshop.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> UpdateUserAsync(Guid id, UserDTO userDto)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return false;

            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Role = userDto.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}