using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class ProjectUpsertRequest
{
    [Required]
    public string Name { get; set; }
}