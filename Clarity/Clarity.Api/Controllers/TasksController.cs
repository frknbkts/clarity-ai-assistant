using Clarity.Application.Features.Tasks.Commands;
using Clarity.Application.Features.Tasks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Clarity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        // Controller'ın Application katmanıyla konuşmasını sağlayacak olan aracı
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskCommand command)
        {
            var taskId = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, new { id = taskId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _mediator.Send(new GetTasksQuery());
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var query = new GetTaskByIdQuery { Id = id };
            var task = await _mediator.Send(query);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTaskCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(); 
            }

            try
            {
                await _mediator.Send(command);
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(); 
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mediator.Send(new DeleteTaskCommand { Id = id });
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound();
            }

            return NoContent(); 
        }
    }
}