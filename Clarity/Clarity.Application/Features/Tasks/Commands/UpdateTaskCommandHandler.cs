using Clarity.Application.Common.Interfaces;
using MediatR;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTaskCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.TaskItems.FindAsync(new object[] { request.Id }, cancellationToken);

            if (task == null)
            {
                throw new System.Exception($"Task with Id {request.Id} not found.");
            }

            task.Title = request.Title;
            task.Notes = request.Notes;
            task.DueDate = request.DueDate;
            task.IsCompleted = request.IsCompleted;
            task.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}