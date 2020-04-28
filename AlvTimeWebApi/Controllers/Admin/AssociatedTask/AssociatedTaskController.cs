using AlvTime.Business.AssociatedTask;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin.AssociatedTask
{
    [Route("api/admin")]
    [ApiController]
    public class AssociatedTaskController : Controller
    {
        private readonly IAssociatedTaskStorage _storage;

        public AssociatedTaskController(IAssociatedTaskStorage storage)
        {
            _storage = storage;
        }

        [HttpGet("AssociatedTasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<AssociatedTaskResponseDto>> FetchAssociatedTasks()
        {
            var tasks = _storage.GetAssociatedTasks();

            return Ok(tasks);
        }

        [HttpPost("CreateAssociatedTask")]
        [AuthorizeAdmin]
        public ActionResult<AssociatedTaskResponseDto> CreateAssociatedTask([FromBody] AssociatedTaskRequestDto associatedTask)
        {
            return Ok(_storage.CreateAssociatedTask(associatedTask));
        }

        [HttpPost("UpdateAssociatedTask")]
        [AuthorizeAdmin]
        public ActionResult<AssociatedTaskResponseDto> UpdateAssociatedTask([FromBody] AssociatedTaskUpdateDto associatedTask)
        {
            return Ok(_storage.UpdateAssociatedTask(associatedTask));
        }
    }
}
