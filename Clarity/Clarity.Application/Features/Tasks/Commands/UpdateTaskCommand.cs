using MediatR;

namespace Clarity.Application.Features.Tasks.Commands
{
    public class UpdateTaskCommand : IRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}