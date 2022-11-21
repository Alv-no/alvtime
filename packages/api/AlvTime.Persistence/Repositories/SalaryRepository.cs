using AlvTime.Business.Users;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Models;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using User = AlvTime.Persistence.DatabaseModels.User;

namespace AlvTime.Persistence.Repositories
{
    public class SalaryRepository : ISalaryRepository
    {
        private readonly AlvTime_dbContext _context;

        public SalaryRepository(AlvTime_dbContext context)
        {
            _context = context;
        }



        public async Task<IEnumerable<UserResponseDto>> GetSalaryForUser(UserQuerySearch criteria)
        {
            throw new NotImplementedException();
            //return await _context.User.AsQueryable()
            //    .Filter(criteria)
            //    .Select(u => new UserResponseDto
            //    {
            //        Email = u.Email,
            //        Id = u.Id,
            //        Name = u.Name,
            //        StartDate = u.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            //        EndDate = u.EndDate != null
            //            ? ((DateTime)u.EndDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            //            : null
            //    }).ToListAsync();
        }

    }
}