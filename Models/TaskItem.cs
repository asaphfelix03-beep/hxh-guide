using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class TaskItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TaskState State { get; set; } = TaskState.Todo;

    /// <summary>Ordre d'affichage dans sa colonne du tableau Kanban.</summary>
    public int Position { get; set; }

    public Priority Priority { get; set; } = Priority.Normal;

    public DateOnly? DueDate { get; set; }

    /// <summary>Étiquettes normalisées, séparées par des virgules (voir <see cref="TagList"/>).</summary>
    [MaxLength(200)]
    public string Tags { get; set; } = "";

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }

    public string? CreatedById { get; set; }
    public AppUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public IReadOnlyList<string> TagList => TaskFlow.Models.TagList.Parse(Tags);

    public bool IsOverdue(DateOnly today) => State != TaskState.Done && DueDate < today;

    /// <summary>Change l'état en tenant à jour la date de fin.</summary>
    public void MoveTo(TaskState state)
    {
        if (state == TaskState.Done && State != TaskState.Done)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (state != TaskState.Done)
        {
            CompletedAt = null;
        }
        State = state;
    }
}

public enum TaskState
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
}

public enum Priority
{
    Low = 0,
    Normal = 1,
    High = 2,
}

public static class EnumLabels
{
    public static string Label(this TaskState state) => state switch
    {
        TaskState.InProgress => "En cours",
        TaskState.Done => "Terminé",
        _ => "À faire",
    };

    public static string Label(this Priority priority) => priority switch
    {
        Priority.Low => "Basse",
        Priority.High => "Haute",
        _ => "Normale",
    };
}
