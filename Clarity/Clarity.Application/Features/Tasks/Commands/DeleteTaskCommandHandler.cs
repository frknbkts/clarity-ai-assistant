using Clarity.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteTaskCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.TaskItems.FindAsync(new object[] { request.Id }, cancellationToken);

            if (task == null)
            {
                throw new System.Exception($"Task with Id {request.Id} not found.");
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}