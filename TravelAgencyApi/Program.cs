using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Data.SqlClient;
using TravelAgencyApi.Repositories;
using TravelAgencyApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI – repozytorium
builder.Services.AddScoped<IClientRepository, SqlClientRepository>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientValidator>();

var app = builder.Build();

// ---------- Middleware ----------
app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (SqlException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new { error = "Błąd serwera" });
    }
});

app.MapControllers();
app.Run();