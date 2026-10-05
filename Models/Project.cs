using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class Project
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = "";

    [MaxLength(300)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProjectMember> Members { get; set; } = [];

    public List<TaskItem> Tasks { get; set; } = [];
}

public class ProjectMember
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = null!;

    public ProjectRole Role { get; set; } = ProjectRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public enum ProjectRole
{
    Member = 0,
    Owner = 1,
}
