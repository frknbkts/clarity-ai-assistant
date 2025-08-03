using Clarity.Application.Features.Tasks.Dtos;
using MediatR;

namespace Clarity.Application.Features.Tasks.Queries
{
    public class GetTasksQuery : IRequest<List<TaskDto>>
    {

    }
}