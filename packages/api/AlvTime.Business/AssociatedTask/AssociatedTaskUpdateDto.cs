using System;

namespace AlvTime.Business.AssociatedTask
{
    public class AssociatedTaskUpdateDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? TaskId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
