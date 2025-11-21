using Fcg.User.Infra;
using FluentValidation;
using MediatR;
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});
#endregion

var app = builder.Build();

#region User Endpoints
app.MapGet("/api/users/{id}", async (Guid id, IUserQuery _userQuery) =>
{
    var user = await _userQuery.GetByIdUserAsync(id);

    return user is not null ? Results.Ok(user) : Results.NotFound();
}).RequireAuthorization().WithTags("Users");

app.MapGet("/api/users/{id}/games", async (Guid id, IUserQuery _userQuery) =>
{
    var user = await _userQuery.GetLibraryByUserAsync(id);

    return user is not null ? Results.Ok(user) : Results.NotFound();
}).RequireAuthorization().WithTags("Users");

app.MapGet("/api/users", async (IUserQuery _userQuery) =>
{
    var users = await _userQuery.GetAllUsersAsync();

    return users is not null ? Results.Ok(users) : Results.NotFound();
}).RequireAuthorization("AdminPolicy").WithTags("Users");

app.MapDelete("/api/users/{id}", async (Guid id, IMediator _mediator) =>
{
    var result = await _mediator.Send(new DeleteUserRequest { Id = id });

    return result.HasErrors
        ? Results.BadRequest(result)
        : Results.Ok(result);
}).RequireAuthorization("AdminPolicy").WithTags("Users");
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
