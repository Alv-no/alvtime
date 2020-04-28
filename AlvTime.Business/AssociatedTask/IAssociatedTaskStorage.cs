using System.Collections.Generic;

namespace AlvTime.Business.AssociatedTask
{
    public interface IAssociatedTaskStorage
    {
        IEnumerable<AssociatedTaskResponseDto> GetAssociatedTasks();
        AssociatedTaskResponseDto CreateAssociatedTask(AssociatedTaskRequestDto associatedTask);
        AssociatedTaskResponseDto UpdateAssociatedTask(AssociatedTaskUpdateDto associatedTask);
    }
}
