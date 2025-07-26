using Clarity.Application.Common.Interfaces;
using Clarity.Application.Features.Tasks.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clarity.Application.Features.Tasks.Queries
{
    public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTasksQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _context.TaskItems
                .AsNoTracking() 
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    DueDate = t.DueDate
                })
                .ToListAsync(cancellationToken);

            return tasks;
        }
    }
}