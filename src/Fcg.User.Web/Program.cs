using Fcg.User.Application.Requests;
using Fcg.User.Domain.Queries;
using Fcg.User.Infra;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddApplicationLayer();
builder.Services.AddInfraLayer(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpContextAccessor();

#region Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT com prefixo 'Bearer '"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region Authentication & Authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"))
    .AddPolicy("InternalPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            // pega o httpContext
            if (context.Resource is not HttpContext http) return false;

            // pega a chave enviada no header
            if (!http.Request.Headers.TryGetValue("X-Internal-Key", out var providedKey))
                return false;

            // busca a chave configurada (em appsettings)
            var expectedKey = http.RequestServices
                .GetRequiredService<IConfiguration>()
                .GetValue<string>("InternalApiKey");

            return expectedKey == providedKey;
        });
    });
#endregion

var app = builder.Build();

#region User Endpoints
app.MapGet("/api/users/{id}", async (Guid id, IMediator _mediator) =>
{
    var response = await _mediator.Send(new GetUserByIdRequest { Id = id });

    return Results.Ok(response);
}).RequireAuthorization().WithTags("Users");

app.MapPut("/api/users/{id}", async (Guid id, [FromBody] UpdateUserRequest request, IMediator _mediator) =>
{
    var response = await _mediator.Send(request);

    return Results.Ok(response);
}).RequireAuthorization().WithTags("Users");

app.MapGet("/api/users", async (IUserQuery _userQuery) =>
{
    var users = await _userQuery.GetUsersAsync();

    return users is not null ? Results.Ok(users) : Results.NotFound();
}).RequireAuthorization("AdminPolicy").WithTags("Users");

app.MapDelete("/api/users/{id}", async (Guid id, IMediator _mediator) =>
{
    var response = await _mediator.Send(new DeleteUserRequest { Id = id });

    return Results.Ok(response);
}).RequireAuthorization("AdminPolicy").WithTags("Users");

app.MapPost("/api/users", async ([FromBody] RegisterUserRequest request, IMediator _mediator) =>
{
    var response = await _mediator.Send(request);

    return Results.Ok(response);
}).RequireAuthorization("InternalPolicy").WithTags("Users");

app.MapPost("/api/users/{id}/credit", async (Guid id, [FromBody] CreditWalletRequest request, IMediator _mediator) =>
{
    var result = await _mediator.Send(request);

    return result.HasErrors
        ? Results.BadRequest(result)
        : Results.Ok(result);
}).RequireAuthorization("InternalPolicy").WithTags("Users");

app.MapPost("/api/users/{id}/debit", async (Guid id, [FromBody] DebitWalletRequest request, IMediator _mediator) =>
{
    var result = await _mediator.Send(request);

    return result.HasErrors
        ? Results.BadRequest(result)
        : Results.Ok(result);
}).RequireAuthorization("InternalPolicy").WithTags("Users");
#endregion

#region Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
#endregion
