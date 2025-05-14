using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.DTOs;

namespace Workshop.Service.Interface
{
    public interface IBookmarkService
    {
        Task<List<BookmarkDto>> GetUserBookmarksAsync(Guid userId);
        Task<bool> AddBookmarkAsync(Guid userId, int bookId);
        Task<bool> RemoveBookmarkAsync(Guid userId, int bookId);
        Task<bool> IsBookmarkedAsync(Guid userId, int bookId);
    }
}