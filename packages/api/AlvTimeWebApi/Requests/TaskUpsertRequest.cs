using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class TaskUpsertRequest
{
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Locked { get; set; }
    [Required]
    public decimal CompensationRate { get; set; }
    public bool Imposed { get; set; }
}