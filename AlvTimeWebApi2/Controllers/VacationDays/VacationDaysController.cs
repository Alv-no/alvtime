using AlvTimeApi.DataBaseModels;
using AlvTimeApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AlvTimeApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "EasyAuth")]
    public class VacationDaysController : Controller
    {
        private readonly ApplicationDbContext _database;

        public VacationDaysController(ApplicationDbContext database)
        {
            _database = database;
        }

        [HttpGet("VacationDays")]
        public ActionResult<VacationDaysDto> FetchVacationDays()
        {
            var user = RetrieveUser();

            var vacationDays = _database.VacationDays
                .Where(x => x.UserId == user.Id)
                .Select(x => new VacationDaysDto
                {
                    AvailableDays = x.AvailableDays,
                    UsedDays = x.UsedDays
                });

            return Ok(vacationDays);
        }

        [HttpPost("VacationDays")]
        public ActionResult<VacationDaysDto> UpsertVacationDays(int registeredVacationDays)
        {
            User user = RetrieveUser();

            VacationDays vacationDaysObject = RetrieveExistingVacationDays(user);

            if(vacationDaysObject.AvailableDays >= registeredVacationDays)
            {
                vacationDaysObject.UsedDays += registeredVacationDays;
                vacationDaysObject.AvailableDays -= registeredVacationDays;
            }

            _database.SaveChanges();

            return Ok(_database.VacationDays.FirstOrDefault(x => x.UserId == user.Id));
        }

        private User RetrieveUser()
        {
            var username = HttpContext.User.Identity.Name ?? "NameNotFound";
            var user = _database.User.FirstOrDefault(x => x.Email.Trim() == username.Trim());
            return user;
        }

        private VacationDays RetrieveExistingVacationDays(User user)
        {
            return _database.VacationDays.FirstOrDefault(x =>
                x.UserId == user.Id);
        }
    }
}
