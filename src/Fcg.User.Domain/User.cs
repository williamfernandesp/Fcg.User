namespace Fcg.User.Domain
{
    public class User
    {
        public Guid Id { get; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public List<UserGame> Library { get; private set; } = [];

        public User(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio ou nulo.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio ou nulo.", nameof(email));

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
        }

        public User(Guid id, string name, string email, IEnumerable<UserGame> library)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id não pode ser vazio ou nulo.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio ou nulo.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio ou nulo.", nameof(email));

            Id = id;
            Name = name;
            Email = email;
            Library = library?.Distinct().ToList() ?? [];
        }

        public void AddGameToLibrary(Guid gameId)
        {
            if (gameId == Guid.Empty)
                throw new ArgumentException("ID do game inválido.", nameof(gameId));

            if (Library.Select(x => x.GameId).Contains(gameId))
                throw new InvalidOperationException($"O game com ID '{gameId}' já está na biblioteca do usuário.");

            Library.Add(new UserGame(gameId, DateTimeOffset.Now));
        }

        public void RemoveGameFromLibrary(Guid gameId)
        {
            var game = Library.FirstOrDefault(x => x.GameId == gameId);

            if (game != null)
                Library.Remove(game);
        }
    }
}
