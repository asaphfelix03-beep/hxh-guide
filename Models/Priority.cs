namespace TaskFlow.Models;

public enum Priority
{
    Low = 0,
    Normal = 1,
    High = 2,
}

public static class PriorityExtensions
{
    public static string Label(this Priority priority) => priority switch
    {
        Priority.Low => "Basse",
        Priority.High => "Haute",
        _ => "Normale",
    };
}
