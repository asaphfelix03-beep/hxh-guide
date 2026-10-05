using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class ProjectInput
{
    [Display(Name = "Nom du projet")]
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(80, ErrorMessage = "80 caractères maximum.")]
    public string Name { get; set; } = "";

    [Display(Name = "Description (facultative)")]
    [StringLength(300, ErrorMessage = "300 caractères maximum.")]
    public string? Description { get; set; }

    public void ApplyTo(Project project)
    {
        project.Name = Name.Trim();
        project.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
    }
}
