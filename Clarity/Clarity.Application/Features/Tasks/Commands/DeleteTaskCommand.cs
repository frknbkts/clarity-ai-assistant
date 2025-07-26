using MediatR;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class DeleteTaskCommand : IRequest
    {
        public int Id { get; set; }
    }
}
