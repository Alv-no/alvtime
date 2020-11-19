using AlvTime.Business.AssociatedTask;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
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

        [HttpPost("AssociatedTasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<AssociatedTaskResponseDto>> CreateAssociatedTask([FromBody] IEnumerable<AssociatedTaskRequestDto> associatedTasks)
        {
            List<AssociatedTaskResponseDto> response = new List<AssociatedTaskResponseDto>();

            foreach (var associatedTask in associatedTasks)
            {
                response.Add(_storage.CreateAssociatedTask(associatedTask));
            }

            return Ok(response);
        }

        [HttpPut("AssociatedTasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<AssociatedTaskResponseDto>> UpdateAssociatedTask([FromBody] IEnumerable<AssociatedTaskUpdateDto> associatedTasks)
        {
            List<AssociatedTaskResponseDto> response = new List<AssociatedTaskResponseDto>();

            foreach (var associatedTask in associatedTasks)
            {
                response.Add(_storage.UpdateAssociatedTask(associatedTask));
            }

            return Ok(response);
        }
    }
}
