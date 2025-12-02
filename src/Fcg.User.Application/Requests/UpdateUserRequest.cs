using Fcg.User.Common;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Fcg.User.Application.Requests
{
    public class UpdateUserRequest : IRequest<Response>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do usuário não pode ser vazio.")
                .MaximumLength(100).WithMessage("O nome do usuário não pode exceder 100 caracteres.");
        }
    }
}
