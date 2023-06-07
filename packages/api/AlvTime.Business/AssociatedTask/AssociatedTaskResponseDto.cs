namespace AlvTime.Business.AssociatedTask;

public class AssociatedTaskResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public string FromDate { get; set; }
    public string EndDate { get; set; }
}