using AlvTime.Business.Projects;
using AlvTime.Persistence.DataBaseModels;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public static class ProjectQueryableExtension
    {
        public static IQueryable<Project> Filter(this IQueryable<Project> query, ProjectQuerySearch criterias)
        {
            if (criterias.Id != null)
            {
                query = query.Where(project => project.Id == criterias.Id);
            }
            if (criterias.Name != null)
            {
                query = query.Where(project => project.Name == criterias.Name);
            }
            if (criterias.Customer != null)
            {
                query = query.Where(project => project.Customer == criterias.Customer);
            }

            return query;
        }
    }
}
