using Clarity.Application.Common.Interfaces;
using Clarity.Application.Features.Tasks.Dtos;
using MediatR;

namespace Clarity.Application.Features.Tasks.Queries
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _context.TaskItems.FindAsync(new object[] { request.Id }, cancellationToken);

            if (task == null)
            {
                return null; 
            }

            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                IsCompleted = task.IsCompleted,
                DueDate = task.DueDate
            };
        }
    }
}