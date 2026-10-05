using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class TaskItem
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    public Priority Priority { get; set; } = Priority.Normal;

    public DateOnly? DueDate { get; set; }

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsOverdue(DateOnly today) => !IsDone && DueDate < today;
}
