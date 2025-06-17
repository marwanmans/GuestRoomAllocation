// Services/IUserService.cs
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;
using System.Security.Claims;

namespace GuestRoomAllocation.Web.Services
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal user);
        Task<bool> HasApartmentAccessAsync(int userId, int apartmentId);
        Task<bool> HasGuestAccessAsync(int userId, int guestId);
        Task<bool> HasRoomAccessAsync(int userId, int roomId);
        Task<List<int>> GetUserApartmentIdsAsync(int userId);
        Task<List<int>> GetUserGuestIdsAsync(int userId);
        Task<List<Apartment>> GetUserApartmentsAsync(int userId);
        Task<List<Guest>> GetUserGuestsAsync(int userId);
        bool IsAdmin(ClaimsPrincipal user);
        int GetUserId(ClaimsPrincipal user);
    }
}

// Services/UserService.cs
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GuestRoomAllocation.Web.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userId = GetUserId(user);
            if (userId == 0) return null;

            return await _context.Users
                .Include(u => u.ApartmentAccess)
                    .ThenInclude(a => a.Apartment)
                .Include(u => u.GuestAccess)
                    .ThenInclude(g => g.Guest)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public int GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public bool IsAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }

        public async Task<bool> HasApartmentAccessAsync(int userId, int apartmentId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Admins have access to everything
            if (user.Role == UserRole.Admin) return true;

            // Check specific apartment access
            return await _context.UserApartmentAccess
                .AnyAsync(ua => ua.UserId == userId && ua.ApartmentId == apartmentId);
        }

        public async Task<bool> HasGuestAccessAsync(int userId, int guestId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Admins have access to everything
            if (user.Role == UserRole.Admin) return true;

            // Check specific guest access
            return await _context.UserGuestAccess
                .AnyAsync(ug => ug.UserId == userId && ug.GuestId == guestId);
        }

        public async Task<bool> HasRoomAccessAsync(int userId, int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            return await HasApartmentAccessAsync(userId, room.ApartmentId);
        }

        public async Task<List<int>> GetUserApartmentIdsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<int>();

            // Admins have access to all apartments
            if (user.Role == UserRole.Admin)
            {
                return await _context.Apartments.Select(a => a.Id).ToListAsync();
            }

            // Return user's specific apartment access
            return await _context.UserApartmentAccess
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.ApartmentId)
                .ToListAsync();
        }

        public async Task<List<int>> GetUserGuestIdsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<int>();

            // Admins have access to all guests
            if (user.Role == UserRole.Admin)
            {
                return await _context.Guests.Select(g => g.Id).ToListAsync();
            }

            // Return user's specific guest access
            return await _context.UserGuestAccess
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GuestId)
                .ToListAsync();
        }

        public async Task<List<Apartment>> GetUserApartmentsAsync(int userId)
        {
            var apartmentIds = await GetUserApartmentIdsAsync(userId);
            return await _context.Apartments
                .Where(a => apartmentIds.Contains(a.Id))
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<List<Guest>> GetUserGuestsAsync(int userId)
        {
            var guestIds = await GetUserGuestIdsAsync(userId);
            return await _context.Guests
                .Where(g => guestIds.Contains(g.Id))
                .OrderBy(g => g.LastName)
                .ThenBy(g => g.FirstName)
                .ToListAsync();
        }
    }
}