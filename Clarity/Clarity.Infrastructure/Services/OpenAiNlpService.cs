using Clarity.Application.Common.Interfaces;
using Clarity.Application.Features.Tasks.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Clarity.Infrastructure.Services
{
    public class OpenAiNlpService : INlpService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiNlpService> _logger;
        private const string ModelName = "gpt-3.5-turbo";

        public OpenAiNlpService(IConfiguration configuration, ILogger<OpenAiNlpService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CreateTaskCommand?> ProcessTextToTaskCommand(string text, CancellationToken cancellationToken)
        {
            try
            {
                var apiKey = _configuration["OpenAI:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("OpenAI API key is not configured");
                    return null;
                }

                _logger.LogInformation("Processing text: {Text}", text);

                // OpenAI Client oluşturma
                var client = new OpenAIClient(apiKey);
                var chatClient = client.GetChatClient(ModelName);

                var systemPrompt = @"You are a task processing assistant. Convert the user's text into a JSON object with these exact fields:
{
  ""Title"": ""string - brief task title"",
  ""Notes"": ""string or null - additional details"",
  ""DueDate"": ""string or null - ISO 8601 format (YYYY-MM-DDTHH:mm:ssZ)""
}

Rules:
- Respond ONLY with valid JSON
- Use null for empty fields
- Extract meaningful title from the text
- If date mentioned, convert to ISO format
- If no date mentioned, use null for DueDate";

                var userPrompt = $"Today is {DateTime.UtcNow:yyyy-MM-dd}. Convert this text to task JSON: {text}";

                _logger.LogDebug("System prompt: {SystemPrompt}", systemPrompt);
                _logger.LogDebug("User prompt: {UserPrompt}", userPrompt);

                // Mesajları hazırlama
                var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(systemPrompt),
                    ChatMessage.CreateUserMessage(userPrompt)
                };

                // Chat completion isteği - basit versiyon
                var completionResult = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

                if (completionResult?.Value?.Content?.Count > 0)
                {
                    var jsonResponse = completionResult.Value.Content[0].Text;
                    _logger.LogInformation("OpenAI Response: {Response}", jsonResponse);

                    // JSON'u temizle (eğer markdown formatında dönerse)
                    var cleanJson = CleanJsonResponse(jsonResponse);
                    _logger.LogDebug("Cleaned JSON: {CleanJson}", cleanJson);

                    var command = JsonSerializer.Deserialize<CreateTaskCommand>(cleanJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    });

                    _logger.LogInformation("Successfully parsed command: Title={Title}, Notes={Notes}, DueDate={DueDate}",
                        command?.Title, command?.Notes, command?.DueDate);

                    return command;
                }

                _logger.LogWarning("No content received from OpenAI");
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API: {ErrorMessage}", ex.Message);
                return null;
            }
        }

        private string CleanJsonResponse(string response)
        {
            // Markdown code block'larını temizle
            response = response.Trim();

            if (response.StartsWith("```json"))
            {
                response = response.Substring(7);
            }
            else if (response.StartsWith("```"))
            {
                response = response.Substring(3);
            }

            if (response.EndsWith("```"))
            {
                response = response.Substring(0, response.Length - 3);
            }

            return response.Trim();
        }
    }
}