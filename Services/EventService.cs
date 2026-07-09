using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;

namespace NOTESPACK.Services
{
    public class EventService
    {
        private readonly IDbContextFactory<EventContext> _contextFactory;

        public EventService(IDbContextFactory<EventContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Event>> GetEventsByUserIdAsync(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task AddEventAsync(Event newEvent, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            newEvent.UserId = userId;
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event updatedEvent, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var existingEvent = await context.Events
                .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id && e.UserId == userId);

            if (existingEvent != null)
            {
                existingEvent.Title = updatedEvent.Title;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.Date = updatedEvent.Date;
                existingEvent.Location = updatedEvent.Location;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteEventAsync(int id, int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var eventToDelete = await context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            
            if (eventToDelete != null)
            {
                context.Events.Remove(eventToDelete);
                await context.SaveChangesAsync();
            }
        }
    }
}