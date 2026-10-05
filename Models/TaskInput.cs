using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

/// <summary>
/// Données saisies par l'utilisateur (formulaires et API).
/// Séparé de <see cref="TaskItem"/> pour qu'on ne puisse pas modifier Id ou CreatedAt.
/// </summary>
public class TaskInput
{
    [Display(Name = "Titre")]
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le titre ne doit pas dépasser 100 caractères.")]
    public string Title { get; set; } = "";

    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "La description ne doit pas dépasser 500 caractères.")]
    public string? Description { get; set; }

    [Display(Name = "Priorité")]
    public Priority Priority { get; set; } = Priority.Normal;

    [Display(Name = "Échéance")]
    public DateOnly? DueDate { get; set; }

    [Display(Name = "Terminée")]
    public bool IsDone { get; set; }

    public static TaskInput From(TaskItem task) => new()
    {
        Title = task.Title,
        Description = task.Description,
        Priority = task.Priority,
        DueDate = task.DueDate,
        IsDone = task.IsDone,
    };

    public void ApplyTo(TaskItem task)
    {
        task.Title = Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        task.Priority = Priority;
        task.DueDate = DueDate;
        task.IsDone = IsDone;
    }
}
