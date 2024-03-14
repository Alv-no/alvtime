using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class UserUpdateRequest
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [Required]
    public int EmployeeId { get; set; }
}