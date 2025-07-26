using Clarity.Application.Features.Tasks.Commands;

namespace Clarity.Application.Common.Interfaces
{
    public interface INlpService
    {
        Task<CreateTaskCommand?> ProcessTextToTaskCommand(string text, CancellationToken cancellationToken);
    }
}