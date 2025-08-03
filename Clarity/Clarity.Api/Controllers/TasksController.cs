using Clarity.Application.Common.Interfaces;
using Clarity.Application.Features.Tasks.Commands;
using Clarity.Application.Features.Tasks.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Clarity.Api.Controllers
{
    public record ParseTaskRequest(string Text);

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INlpService _nlpService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(IMediator mediator, INlpService nlpService, ILogger<TasksController> logger)
        {
            _mediator = mediator;
            _nlpService = nlpService;
            _logger = logger;
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

        [HttpPost("parse")]
        public async Task<IActionResult> ParseAndCreate([FromBody] ParseTaskRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest("Request text cannot be empty.");
                }

                _logger.LogInformation("Received parse request with text: {Text}", request.Text);

                var command = await _nlpService.ProcessTextToTaskCommand(request.Text, CancellationToken.None);

                if (command == null)
                {
                    _logger.LogWarning("NLP service returned null for text: {Text}", request.Text);
                    return BadRequest(new
                    {
                        error = "The provided text could not be processed into a task.",
                        text = request.Text,
                        suggestion = "Please provide a clearer task description."
                    });
                }

                _logger.LogInformation("Successfully processed text into command: {Command}",
                    JsonSerializer.Serialize(command));

                var taskId = await _mediator.Send(command);

                _logger.LogInformation("Task created with ID: {TaskId}", taskId);

                return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, new { id = taskId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ParseAndCreate endpoint");
                return StatusCode(500, new
                {
                    error = "An internal error occurred while processing the request.",
                    details = ex.Message
                });
            }
        }
    }
}