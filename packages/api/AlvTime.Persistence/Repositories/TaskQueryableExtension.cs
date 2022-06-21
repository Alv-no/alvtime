using AlvTime.Business.Tasks;
using System.Linq;
using Task = AlvTime.Persistence.DatabaseModels.Task;

namespace AlvTime.Persistence.Repositories
{
    public static class TaskQueryableExtensions
    {
        public static IQueryable<Task> Filter(this IQueryable<Task> query, TaskQuerySearch criterias)
        {
            if (criterias.Locked != null)
            {
                query = query.Where(task => task.Locked == criterias.Locked);
            }
            if (criterias.Name != null)
            {
                query = query.Where(task => task.Name == criterias.Name);
            }
            if (criterias.Project != null)
            {
                query = query.Where(task => task.Project == criterias.Project);
            }
            if (criterias.Id != null)
            {
                query = query.Where(task => task.Id == criterias.Id);
            }

            return query;
        }
    }
}
