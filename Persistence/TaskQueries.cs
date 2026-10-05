using TaskFlow.Models;

namespace TaskFlow.Persistence;

public static class TaskQueries
{
    /// <summary>Filtre par statut : "active" (en cours), "done" (terminées), sinon toutes.</summary>
    public static IQueryable<TaskItem> WithStatus(this IQueryable<TaskItem> query, string? status) => status switch
    {
        "active" => query.Where(t => !t.IsDone),
        "done" => query.Where(t => t.IsDone),
        _ => query,
    };

    /// <summary>En cours d'abord, puis priorité haute, puis échéance la plus proche (sans échéance en dernier).</summary>
    public static IQueryable<TaskItem> InDisplayOrder(this IQueryable<TaskItem> query) => query
        .OrderBy(t => t.IsDone)
        .ThenByDescending(t => t.Priority)
        .ThenBy(t => t.DueDate == null)
        .ThenBy(t => t.DueDate)
        .ThenByDescending(t => t.CreatedAt);
}
