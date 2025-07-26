using Clarity.Application.Common.Interfaces;
using Clarity.Infrastructure.Persistence;
using Clarity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Clarity.Application.Features.Tasks.Commands.CreateTaskCommand).Assembly));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<INlpService, OpenAiNlpService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();