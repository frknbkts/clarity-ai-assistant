using Clarity.Application.Common.Interfaces;
using Microsoft.Extensions.Logging; 
using System.Diagnostics; 

namespace Clarity.Infrastructure.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendTaskReminderAsync(string userEmail, string taskTitle, DateTime dueDate)
        {
            var message = $"--- TASK REMINDER --- \nTo: {userEmail}\nSubject: Reminder: {taskTitle}\n\nHi,\n\nThis is a reminder that your task '{taskTitle}' is due at {dueDate.ToLocalTime()}.\n\nThank you,\nClarity AI Assistant";

            _logger.LogInformation("Sending Task Reminder: {Message}", message);
            Debug.WriteLine(message);

            return Task.CompletedTask;
        }
    }
}