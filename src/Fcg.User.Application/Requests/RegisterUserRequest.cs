using Fcg.User.Application.Handlers;
using Fcg.User.Common;
using MediatR;

namespace Fcg.User.Application.Requests
{
    public class RegisterUserRequest : IRequest<CreateUserResponse>
    {
        public Guid Id { get; set; }
    }
}
