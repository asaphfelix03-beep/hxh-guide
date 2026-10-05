namespace TaskFlow.Models;

public record MemberChip(string Id, string Name)
{
    public string Initials => UserDisplay.Initials(Name);
}

public record ProjectSummary(int Id, string Name, string? Description, int TaskCount, int DoneCount, List<MemberChip> Members)
{
    public int Percent => TaskCount == 0 ? 0 : (int)Math.Round(100.0 * DoneCount / TaskCount);
}
