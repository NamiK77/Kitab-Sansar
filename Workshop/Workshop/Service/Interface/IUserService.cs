using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.DTOs.Response;

namespace Workshop.Service.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(Guid id);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> UpdateUserAsync(Guid id, UserDTO userDto);
    }
}