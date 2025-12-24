using Fcg.User.Application.Requests;
using Fcg.User.Common;
using Fcg.User.Domain.Queries;
using Fcg.User.Proxy.Auth.Client.Interface;
using Fcg.User.Proxy.Games.Client.Interface;
using MediatR;

namespace Fcg.User.Application.Handlers
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, Response>
    {
        private readonly IUserQuery _userQuery;
        private readonly IClientAuth _clientAuth;
        private readonly IClientGames _clientGame;

        public GetUserByIdHandler(IUserQuery userQuery, IClientAuth clientAuth, IClientGames clientGame)
        {
            _userQuery = userQuery;
            _clientAuth = clientAuth;
            _clientGame = clientGame;
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

            if (externalResponse.HasErrors || string.IsNullOrEmpty(externalResponse.Result.Email))
            {
                response.AddError("Erro ao buscar usuário externo.");
                return response;
            }

            user.Email = externalResponse.Result.Email;

            var gameIds = await _userQuery.GetGamesByUserIdAsync(user.Id, cancellationToken) ?? [];

            if (gameIds.Any())
            {
                foreach ( var gameId in gameIds)
                {
                    var externalGameResponse = await _clientGame.GetGameAsync(gameId);

                    if (externalGameResponse.HasErrors)
                    {
                        response.AddError("Erro ao obter jogos da biblioteca do usuário.");
                    }
                    else
                    {
                        var gameResposne = externalGameResponse.Result;

                        var game = new Domain.Queries.Responses.GameResponse
                        {
                            Id = gameResposne.Id,
                            Title = gameResposne.Title,
                            Description = gameResposne.Description,
                            Price = gameResposne.Price,
                            Genre = gameResposne.Genre
                        };

                        if (game != null)
                        {
                            user.Library?.Add(game);
                        }
                    }
                }
            }

            response.SetResult(user);

            return response;
        }
    }
}
