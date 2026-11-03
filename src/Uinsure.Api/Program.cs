using Uinsure.Api.Infrastructure;
using Uinsure.Api.Services;
using Uinsure.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IPolicyRepository, InMemoryPolicyRepository>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

builder.Services.AddScoped<PolicyValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
