using Fcg.User.Application.Requests;
using Fcg.User.Common;
using Fcg.User.Domain.Queries;
using Fcg.User.Proxy.Auth.Client.Interface;
using MediatR;

namespace Fcg.User.Application.Handlers
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, Response>
    {
        private readonly IUserQuery _userQuery;
        private readonly IClientAuth _clientAuth;

        public GetUserByIdHandler(IUserQuery userQuery, IClientAuth clientAuth)
        {
            _userQuery = userQuery;
            _clientAuth = clientAuth;
        }

        public async Task<Response> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
        {
            var response = new Response();

            var user = await _userQuery.GetUserByIdAsync(request.Id, cancellationToken);

            if (user == null)
            {
                response.AddError($"Usuário {request.Id} não encontrado!");
                return response;
            }

            var externalResponse = await _clientAuth.GetAuthUserAsync(user.Id);

            if (externalResponse.HasErrors)
            {
                response.AddError("Erro ao criar usuário externo.");
            }

            user.Email = externalResponse.Result.Email;

            var gameIds = user.Library != null ? [.. user.Library!.Select(g => g.Id)] : new List<Guid>();

            // TODO: Tem que pegar os jogos da biblioteca do usuário no projeto Game.

            response.SetResult(user);

            return response;
        }
    }
}
