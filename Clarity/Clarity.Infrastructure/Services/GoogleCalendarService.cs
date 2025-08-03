using Clarity.Application.Common.Interfaces;
using Clarity.Domain.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Clarity.Infrastructure.Services
{
    public class GoogleCalendarService : ICalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GoogleCalendarService> _logger; 

        public GoogleCalendarService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ILogger<GoogleCalendarService> logger) 
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger; 
        }

        public async Task AddEventAsync(string userId, string title, string? notes, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Attempting to add event to Google Calendar for User {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for ID {UserId}", userId);
                return;
            }

            if (string.IsNullOrEmpty(user.GoogleRefreshToken))
            {
                _logger.LogWarning("User {Email} does not have a Google Refresh Token. Skipping calendar event creation.", user.Email);
                return;
            }

            _logger.LogInformation("User {Email} has a refresh token. Proceeding to create calendar service.", user.Email);

            try
            {
                var flow = CreateGoogleAuthorizationCodeFlow();
                var userCredential = new UserCredential(flow, userId, new Google.Apis.Auth.OAuth2.Responses.TokenResponse
                {
                    RefreshToken = user.GoogleRefreshToken
                });

                var calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = "Clarity AI Assistant"
                });

                var newEvent = new Event()
                {
                    Summary = title,
                    Description = notes,
                    Start = new EventDateTime() { DateTimeDateTimeOffset = startTime, TimeZone = "UTC" },
                    End = new EventDateTime() { DateTimeDateTimeOffset = endTime, TimeZone = "UTC" }
                };

                _logger.LogInformation("Creating event '{Title}' in Google Calendar.", title);
                await calendarService.Events.Insert(newEvent, "primary").ExecuteAsync();
                _logger.LogInformation("Successfully created event '{Title}' in Google Calendar.", title);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating Google Calendar event for User {UserId}", userId);
            }
        }


        private GoogleAuthorizationCodeFlow CreateGoogleAuthorizationCodeFlow()
        {
            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["GoogleAuth:ClientId"],
                    ClientSecret = _configuration["GoogleAuth:ClientSecret"]
                },
                Scopes = new[] { CalendarService.Scope.CalendarEvents }
            });
        }
    }
}