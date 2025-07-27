using Clarity.Application.Common.Interfaces;
using Clarity.Domain.Entities;
using MediatR;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; 

namespace Clarity.Application.Features.Tasks.Commands
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateTaskCommandHandler> _logger;
        private readonly ICalendarService _calendarService;

        public CreateTaskCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IBackgroundJobClient backgroundJobClient,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateTaskCommandHandler> logger,
            ICalendarService calendarService) 
        {
            _context = context;
            _currentUserService = currentUserService;
            _backgroundJobClient = backgroundJobClient;
            _userManager = userManager;
            _logger = logger;
            _calendarService = calendarService;
        }

        public async Task<int> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Notes = request.Notes,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = _currentUserService.UserId!
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task {TaskId} created successfully for User {UserId}", task.Id, task.UserId);


            if (task.DueDate.HasValue)
            {
                _logger.LogInformation("Task has a DueDate: {DueDate}. Proceeding to schedule a reminder.", task.DueDate.Value);

                var reminderTime = task.DueDate.Value.AddMinutes(-30);
                _logger.LogInformation("Calculated reminder time: {ReminderTime}", reminderTime);

                var now = DateTime.UtcNow;
                _logger.LogInformation("Current UTC time is: {Now}", now);

                if (reminderTime > now)
                {
                    _logger.LogInformation("Reminder time is in the future. Attempting to find user.");
                    var user = await _userManager.FindByIdAsync(_currentUserService.UserId!);

                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        _logger.LogInformation("User {Email} found. Scheduling the job.", user.Email);

                        _backgroundJobClient.Schedule<INotificationService>(
                            service => service.SendTaskReminderAsync(user.Email, task.Title, task.DueDate.Value),
                            reminderTime);

                        _logger.LogInformation("Job scheduled successfully for TaskId {TaskId} at {ReminderTime}", task.Id, reminderTime);
                    }
                    else
                    {
                        _logger.LogWarning("Could not schedule job because user was not found or has no email. UserId: {UserId}", _currentUserService.UserId);
                    }
                }
                else
                {
                    _logger.LogWarning("Reminder time {ReminderTime} is in the past. Job will not be scheduled.", reminderTime);
                }
            }
            else
            {
                _logger.LogInformation("Task has no DueDate. No reminder will be scheduled.");
            }

            if (task.DueDate.HasValue)
            {
                var eventEndTime = task.DueDate.Value.AddHours(1);
                await _calendarService.AddEventAsync(task.UserId, task.Title, task.Notes, task.DueDate.Value, eventEndTime);
            }

            return task.Id;
        }
    }
}