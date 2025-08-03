using Clarity.Application.Features.Tasks.Dtos;
using MediatR;

namespace Clarity.Application.Features.Tasks.Queries
{
    public class GetTaskByIdQuery : IRequest<TaskDto?>
    {
        public int Id { get; set; }
    }
}