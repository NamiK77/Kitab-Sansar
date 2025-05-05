// // using Microsoft.AspNetCore.Mvc;
// // using Workshop.Model;
// // using Workshop.Service;
// // using System.Threading.Tasks;

// // namespace Workshop.Controllers
// // {
// //     [Route("api/[controller]")]
// //     [ApiController]
// //     public class BookController : ControllerBase
// //     {
// //         private readonly BookService _service;
// //         public BookController(BookService service)
// //         {
// //             _service = service;
// //         }

// //         [HttpGet]
// //         public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

// //         [HttpGet("{id}")]
// //         public async Task<IActionResult> GetById(int id)
// //         {
// //             var book = await _service.GetByIdAsync(id);
// //             if (book == null) return NotFound();
// //             return Ok(book);
// //         }

// //         [HttpPost]
// //         public async Task<IActionResult> Add(Book book)
// //         {
// //             var created = await _service.AddAsync(book);
// //             return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
// //         }

// //         [HttpPut("{id}")]
// //         public async Task<IActionResult> Update(int id, Book book)
// //         {
// //             if (id != book.Id) return BadRequest();
// //             var updated = await _service.UpdateAsync(book);
// //             if (updated == null) return NotFound();
// //             return Ok(updated);
// //         }

// //         [HttpDelete("{id}")]
// //         public async Task<IActionResult> Delete(int id)
// //         {
// //             var deleted = await _service.DeleteAsync(id);
// //             if (!deleted) return NotFound();
// //             return NoContent();
// //         }
// //     }
// // }

// using Microsoft.AspNetCore.Mvc;
// using Workshop.Model;
// using Workshop.Service;
// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Hosting;
// using System.Linq;

// namespace Workshop.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class BookController : ControllerBase
//     {
//         private readonly BookService _service;
//         private readonly IWebHostEnvironment _env;
//         private readonly string _uploadsDir;

        

//         public BookController(BookService service, IWebHostEnvironment env)
//         {
//             _service = service;
//             _env = env;
//             _uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
//             if (!Directory.Exists(_uploadsDir))
//             {
//                 Directory.CreateDirectory(_uploadsDir);
//             }
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetById(int id)
//         {
//             var book = await _service.GetByIdAsync(id);
//             if (book == null) return NotFound();
//             return Ok(book);
//         }

//         [HttpPost]
//         public async Task<IActionResult> Add(Book book)
//         {
//             var created = await _service.AddAsync(book);
//             return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> Update(int id, Book book)
//         {
//             if (id != book.Id) return BadRequest();
//             var updated = await _service.UpdateAsync(book);
//             if (updated == null) return NotFound();
//             return Ok(updated);
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> Delete(int id)
//         {
//             var deleted = await _service.DeleteAsync(id);
//             if (!deleted) return NotFound();
//             return NoContent();
//         }

//         [HttpPost("upload")]
//         public async Task<IActionResult> Upload([FromForm] IFormFile file)
//         {
//             if (file == null || file.Length == 0)
//             {
//                 return BadRequest(new { message = "No file uploaded." });
//             }

//             // Validate file type
//             var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
//             var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
//             if (!allowedExtensions.Contains(extension))
//             {
//                 return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed." });
//             }

//             // Validate file size (max 5MB)
//             if (file.Length > 5 * 1024 * 1024)
//             {
//                 return BadRequest(new { message = "File size exceeds 5MB limit." });
//             }

//             try
//             {
//                 // Generate unique filename
//                 var fileName = $"{Guid.NewGuid()}{extension}";
//                 var filePath = Path.Combine(_uploadsDir, fileName);

//                 // Save file
//                 using (var stream = new FileStream(filePath, FileMode.Create))
//                 {
//                     await file.CopyToAsync(stream);
//                 }

//                 // Construct URL
//                 var baseUrl = $"{Request.Scheme}://{Request.Host}";
//                 var url = $"{baseUrl}/uploads/{fileName}";

//                 return Ok(new { url });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { message = $"Failed to upload image: {ex.Message}" });
//             }
//         }
//     }
// }


using Microsoft.AspNetCore.Mvc;
using Workshop.Model;
using Workshop.Service;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Workshop.Data;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookService _service;
        

        public BookController(BookService service)
        {
            _service = service;
            
        } 
           

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Book book)
        {
            var created = await _service.AddAsync(book);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Book book)
        {
            if (id != book.Id) return BadRequest();
            var updated = await _service.UpdateAsync(book);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

    }
     
}