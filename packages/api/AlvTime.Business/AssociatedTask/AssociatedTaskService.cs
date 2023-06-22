using System.Threading.Tasks;

namespace AlvTime.Business.AssociatedTask;

public class AssociatedTaskService
{
    private readonly IAssociatedTaskStorage _associatedTaskStorage;

    public AssociatedTaskService(IAssociatedTaskStorage associatedTaskStorage)
    {
        _associatedTaskStorage = associatedTaskStorage;
    }

    public async Task<AssociatedTaskResponseDto> CreateAssociatedTask(AssociatedTaskRequestDto associatedTask)
    {
        return await _associatedTaskStorage.CreateAssociatedTask(associatedTask);
    }

    public async Task<AssociatedTaskResponseDto> UpdateAssociatedTask(AssociatedTaskUpdateDto associatedTask)
    {
        return await _associatedTaskStorage.UpdateAssociatedTask(associatedTask);
    }
}

