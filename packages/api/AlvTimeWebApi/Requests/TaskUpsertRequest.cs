using System.ComponentModel.DataAnnotations;
using AlvTime.Business.Tasks;

namespace AlvTimeWebApi.Requests;

public class TaskUpsertRequest
{
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Locked { get; set; }
    [Required]
    public CompensationType CompensationType { get; set; }
    public bool Imposed { get; set; }
}