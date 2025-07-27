
namespace Clarity.Application.Common.Interfaces
{
    public interface ICalendarService
    {
        Task AddEventAsync(string userId, string title, string? notes, DateTime startTime, DateTime endTime);
    }
}
