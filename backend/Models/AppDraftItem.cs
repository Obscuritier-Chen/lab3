namespace backend.Models;

public class AppDraft
{
    enum Directions{D1, D2, D3}
    enum Status{draft, submitted}

    Guid Id { get; set;} = Guid.NewGuid();
    string Nickname {get; set;}
    Directions Direction {get; set;}
    string reason {get; set;}
    Status DraftStatus {get; set;}
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
