using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Economy
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly AlvTime_dbContext _database;

        public AdminController(AlvTime_dbContext database)
        {
            _database = database;
        }

        [HttpGet("EconomyInfo")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<EconomyDto>> FetchEconomyInfo()
        {
            var info = _database.VDataDump
                .Select(x => new EconomyDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    Date = x.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Email = x.Email,
                    HourRate = x.HourRate,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,
                    TaskId = x.TaskId,
                    TaskName = x.TaskName,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Value = x.Value
                })
                .ToList();
            return Ok(info);
        }

        [HttpPost("CreateTask")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> CreateNewTask([FromBody] IEnumerable<CreateTaskDto> tasksToBeCreated)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeCreated)
            {
                if (TaskDoesNotExist(task))
                {
                    var newTask = new Task
                    {
                        Description = task.Description,
                        Favorite = false,
                        Locked = task.Locked,
                        Name = task.Name,
                        Project = task.Project
                    };
                    _database.Add(newTask);
                    _database.SaveChanges();

                    response.Add(ReturnCreatedTask(task));
                }
            }
            return Ok(response);
        }

        [HttpPost("LockTask")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CreateTaskDto>> UpdateLockTask([FromBody] IEnumerable<LockTaskDto> tasksToBeUpdated)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                var taskToBeUpdated = _database.Task
                    .Where(x => x.Id == task.Id)
                    .FirstOrDefault();

                taskToBeUpdated.Locked = task.Locked;
                _database.SaveChanges();

                response.Add(ReturnUpdatedTask(task));
            }
            return Ok(response);
        }

        [HttpPost("CreateCustomer")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> CreateNewCustomer([FromBody] IEnumerable<CreateCustomerDto> customersToBeCreated)
        {
            List<CustomerDto> response = new List<CustomerDto>();

            foreach (var customer in customersToBeCreated)
            {
                if (CustomerDoesNotExist(customer))
                {
                    var newCustomer = new Customer
                    {
                        Name = customer.Name
                    };
                    _database.Add(newCustomer);
                    _database.SaveChanges();

                    response.Add(ReturnCreatedCustomer(customer));
                }
            }
            return Ok(response);
        }

        [HttpPost("CreateProject")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> CreateNewProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
        {
            List<ProjectDto> response = new List<ProjectDto>();

            foreach (var project in projectsToBeCreated)
            {
                if (ProjectDoesNotExist(project))
                {
                    var newProject = new Project
                    {
                        Customer = project.Customer,
                        Name = project.Name,
                    };
                    _database.Add(newProject);
                    _database.SaveChanges();

                    response.Add(ReturnCreatedProject(project));
                }
            }
            return Ok(response);
        }

        [HttpPost("CreateUser")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> CreateNewUser([FromBody] IEnumerable<CreateUserDto> usersToBeCreated)
        {
            List<UserResponseDto> response = new List<UserResponseDto>();

            foreach (var user in usersToBeCreated)
            {
                if (UserDoesNotExist(user))
                {
                    var newUser = new User
                    {
                        Name = user.Name,
                        Email = user.Email
                    };
                    _database.Add(newUser);
                    _database.SaveChanges();

                    response.Add(ReturnCreatedUser(user));
                }
            }
            return Ok(response);
        }

        [HttpPost("HourRate")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> CreateOrUpdateHourRate([FromBody] IEnumerable<CreateHourRateDto> hourRatesToBeCreated)
        {
            List<HourRateResponseDto> response = new List<HourRateResponseDto>();

            foreach (var hourRate in hourRatesToBeCreated)
            {
                var existingHourRate = RetrieveExistingHourRate(hourRate);

                if (existingHourRate == null)
                {
                    var newHourRate = new HourRate
                    {
                        FromDate = hourRate.FromDate,
                        Rate = hourRate.Rate,
                        TaskId = hourRate.TaskId
                    };
                    _database.Add(newHourRate);
                    _database.SaveChanges();
                }
                else
                {
                    existingHourRate.FromDate = hourRate.FromDate;
                    existingHourRate.Rate = hourRate.Rate;
                    existingHourRate.TaskId = hourRate.TaskId;
                }
                response.Add(ReturnCreatedHourRate(hourRate));
            }
            return Ok(response);
        }

        private TaskResponseDto ReturnCreatedTask(CreateTaskDto task)
        {
            var taskResponseDto = _database.Task
                .Where(x => x.Name == task.Name && x.Project == task.Project)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = false,
                    Project = new ProjectDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    }
                })
                .FirstOrDefault();

            return taskResponseDto;
        }

        private CustomerDto ReturnCreatedCustomer(CreateCustomerDto customer)
        {
            var customerResponseDto = _database.Customer
                .Where(x => x.Name == customer.Name)
                .Select(x => new CustomerDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefault();

            return customerResponseDto;
        }

        private ProjectDto ReturnCreatedProject(CreateProjectDto project)
        {
            var projectResponseDto = _database.Project
                .Where(x => x.Name == project.Name && x.Customer == project.Customer)
                .Select(x => new ProjectDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Customer = new CustomerDto
                    {
                        Id = x.CustomerNavigation.Id,
                        Name = x.CustomerNavigation.Name
                    }
                })
                .FirstOrDefault();

            return projectResponseDto;
        }

        private UserResponseDto ReturnCreatedUser(CreateUserDto user)
        {
            var userResponseDto = _database.User
                .Where(x => x.Name == user.Name && x.Email == user.Email)
                .Select(x => new UserResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email
                })
                .FirstOrDefault();

            return userResponseDto;
        }

        private HourRateResponseDto ReturnCreatedHourRate(CreateHourRateDto hourRate)
        {
            var task = _database.Task
                .Where(x => x.Id == hourRate.TaskId)
                .FirstOrDefault();

            var hourRateResponseDto = _database.HourRate
                .Where(x => x.FromDate == hourRate.FromDate && x.Rate == hourRate.Rate && x.TaskId == hourRate.TaskId)
                .Select(x => new HourRateResponseDto
                {
                    Id = x.Id,
                    Rate = x.Rate,
                    FromDate = x.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Task = new TaskResponseDto
                    {
                        Description = task.Description,
                        Id = task.Id,
                        Favorite = false,
                        Locked = task.Locked,
                        Name = task.Name,
                        Project = new ProjectDto
                        {
                            Id = task.ProjectNavigation.Id,
                            Name = task.ProjectNavigation.Name,
                            Customer = new CustomerDto
                            {
                                Id = task.ProjectNavigation.CustomerNavigation.Id,
                                Name = task.ProjectNavigation.CustomerNavigation.Name
                            }
                        }
                    }
                })
                .FirstOrDefault();

            return hourRateResponseDto;
        }

        private TaskResponseDto ReturnUpdatedTask(LockTaskDto task)
        {
            var taskResponseDto = _database.Task
                .Where(x => x.Id == task.Id)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = false,
                    Project = new ProjectDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    },
                })
                .FirstOrDefault();

            return taskResponseDto;
        }

        private bool TaskDoesNotExist(CreateTaskDto task)
        {
            return _database.Task
                    .Where(x => x.Name == task.Name && x.Project == task.Project)
                    .FirstOrDefault() == null ? true : false;
        }

        private bool CustomerDoesNotExist(CreateCustomerDto customer)
        {
            return _database.Customer
                    .Where(x => x.Name == customer.Name)
                    .FirstOrDefault() == null ? true : false;
        }

        private bool ProjectDoesNotExist(CreateProjectDto project)
        {
            return _database.Project
                    .Where(x => x.Name == project.Name && x.Customer == project.Customer)
                    .FirstOrDefault() == null ? true : false;
        }

        private bool UserDoesNotExist(CreateUserDto user)
        {
            return _database.User
                    .Where(x => x.Name == user.Name && x.Email == user.Email)
                    .FirstOrDefault() == null ? true : false;
        }

        private HourRate RetrieveExistingHourRate(CreateHourRateDto hourRate)
        {
            return _database.HourRate.FirstOrDefault(x =>
                x.FromDate == hourRate.FromDate &&
                x.Rate == hourRate.Rate &&
                x.TaskId == hourRate.TaskId);
        }
    }
}
