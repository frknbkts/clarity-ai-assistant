using MediatR;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class CreateTaskCommand : IRequest<int>
    {
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? DueDate { get; set; }
    }
}