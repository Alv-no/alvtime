using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.AssociatedTask
{
    public interface IAssociatedTaskStorage
    {
        Task<IEnumerable<AssociatedTaskResponseDto>> GetAssociatedTasks();
        Task<AssociatedTaskResponseDto> CreateAssociatedTask(AssociatedTaskRequestDto associatedTask);
        Task<AssociatedTaskResponseDto> UpdateAssociatedTask(AssociatedTaskUpdateDto associatedTask);
    }
}
