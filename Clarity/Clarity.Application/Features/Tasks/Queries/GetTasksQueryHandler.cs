using Clarity.Application.Common.Interfaces;
using Clarity.Application.Features.Tasks.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clarity.Application.Features.Tasks.Queries
{
    public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetTasksQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Where(t => t.UserId == _currentUserService.UserId) 
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