namespace Fcg.User.Infra.Tables
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public ICollection<UserGame> Library { get; set; } = [];
    }
}
