using Microsoft.AspNetCore.Diagnostics;
using Uinsure.Api.Infrastructure;
using Uinsure.Api.Services;
using Uinsure.Api.Validation.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IPolicyRepository, InMemoryPolicyRepository>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();

        switch (feature?.Error)
        {
            case ValidationException validationException:
                context.Response.StatusCode = 400;

                await context.Response.WriteAsJsonAsync(new
                {
                    validationException.Errors
                });
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = 404;

                await context.Response.WriteAsJsonAsync(new
                {
                    Error = notFoundException.Message
                });
                break;
        }
    });
});

app.Run();
