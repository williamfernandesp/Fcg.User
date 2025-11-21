namespace Fcg.User.Infra.Tables
{
    public class UserGame
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public DateTimeOffset DateToPurchase { get; set; }
    }
}
