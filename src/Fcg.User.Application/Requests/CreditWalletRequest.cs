using Fcg.User.Common;
using MediatR;

namespace Fcg.User.Application.Requests
{
    public class CreditWalletRequest : IRequest<Response>
    {
        public Guid Id { get; set; }
        public decimal Credit { get; set; }
    }
}
