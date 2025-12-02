using Fcg.User.Common;
using MediatR;

namespace Fcg.User.Application.Requests
{
    public class DebitWalletRequest : IRequest<Response>
    {
        public Guid Id { get; set; }
        public decimal Debit { get; set; }
    }
}
