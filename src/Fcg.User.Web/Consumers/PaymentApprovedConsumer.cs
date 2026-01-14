using Fcg.Contracts.Payments;
using Fcg.User.Infra;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.User.Web.Consumers;

public class PaymentApprovedConsumer : IConsumer<PaymentApprovedEvent>
{
    private readonly FcgUserDbContext _db;

    public PaymentApprovedConsumer(FcgUserDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        // Se for compra de jogo (GameId != null), o débito já é feito via HTTP hoje.
        // Aqui focamos no evento de crédito de saldo (quando GameId é null).
        if (context.Message.GameId is not null)
            return;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == context.Message.UserId, context.CancellationToken);

        if (user is null)
            return;

        user.Wallet += context.Message.Amount;
        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
