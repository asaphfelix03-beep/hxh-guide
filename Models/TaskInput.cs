using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

/// <summary>
/// Données saisies dans le formulaire de tâche.
/// Séparé de <see cref="TaskItem"/> pour qu'on ne puisse pas modifier le projet, l'auteur ou les dates.
/// </summary>
public class TaskInput
{
    [Display(Name = "Titre")]
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le titre ne doit pas dépasser 100 caractères.")]
    public string Title { get; set; } = "";

    [Display(Name = "Description")]
    [StringLength(2000, ErrorMessage = "La description ne doit pas dépasser 2000 caractères.")]
    public string? Description { get; set; }

    [Display(Name = "Colonne")]
    public TaskState State { get; set; } = TaskState.Todo;

    [Display(Name = "Priorité")]
    public Priority Priority { get; set; } = Priority.Normal;

    [Display(Name = "Échéance")]
    public DateOnly? DueDate { get; set; }

    [Display(Name = "Assignée à")]
    public string? AssigneeId { get; set; }

    [Display(Name = "Étiquettes")]
    [StringLength(200, ErrorMessage = "Trop d'étiquettes.")]
    public string? Tags { get; set; }

    public static TaskInput From(TaskItem task) => new()
    {
        Title = task.Title,
        Description = task.Description,
        State = task.State,
        Priority = task.Priority,
        DueDate = task.DueDate,
        AssigneeId = task.AssigneeId,
        Tags = TagList.ToInput(task.Tags),
    };

    public void ApplyTo(TaskItem task)
    {
        task.Title = Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        task.Priority = Priority;
        task.DueDate = DueDate;
        task.AssigneeId = string.IsNullOrEmpty(AssigneeId) ? null : AssigneeId;
        task.Tags = TagList.Normalize(Tags);
        task.MoveTo(State);
    }
}
