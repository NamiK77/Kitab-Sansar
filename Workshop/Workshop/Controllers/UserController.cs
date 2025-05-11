// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Workshop.Data;
// using Workshop.DTOs.Response;
// using Workshop.Model;

// namespace Workshop.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class UserController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public UserController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/User
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
//         {
//             var users = await _context.Users
//                 .Select(u => new UserDTO
//                 {
//                     Id = u.Id,
//                     Username = u.Username,
//                     Email = u.Email,
//                     Role = u.Role
//                 })
//                 .ToListAsync();

//             return Ok(users);
//         }

//         // GET: api/User/5
//         [HttpGet("{id}")]
//         public async Task<ActionResult<UserDTO>> GetUser(Guid id)
//         {
//             var user = await _context.Users.FindAsync(id);

//             if (user == null)
//             {
//                 return NotFound();
//             }

//             var userDto = new UserDTO
//             {
//                 Id = user.Id,
//                 Username = user.Username,
//                 Email = user.Email,
//                 Role = user.Role
//             };

//             return Ok(userDto);
//         }

//         // DELETE: api/User/5
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteUser(Guid id)
//         {
//             var user = await _context.Users.FindAsync(id);
//             if (user == null)
//             {
//                 return NotFound();
//             }

//             _context.Users.Remove(user);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.DTOs.Response;
using Workshop.Service.Interface;
using Workshop.Service;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserDTO userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest();
            }

            var result = await _userService.UpdateUserAsync(id, userDto);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}