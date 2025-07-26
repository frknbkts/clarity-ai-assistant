using Clarity.Application.Common.Interfaces;
using Clarity.Domain.Entities;
using MediatR;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateTaskCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
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

            return task.Id;
        }
    }
}