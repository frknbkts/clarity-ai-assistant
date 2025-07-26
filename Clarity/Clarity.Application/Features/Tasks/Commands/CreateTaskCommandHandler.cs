using Clarity.Application.Common.Interfaces;
using Clarity.Domain.Entities;
using MediatR;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateTaskCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Notes = request.Notes,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow 
            };

            _context.TaskItems.Add(task);

            await _context.SaveChangesAsync(cancellationToken);

            return task.Id;
        }
    }
}