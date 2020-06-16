using AlvTime.Business.Projects;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.Projects.ProjectStorage
{
    public static class ProjectQueryableExtension
    {
        public static IQueryable<Project> Filter(this IQueryable<Project> query, ProjectQuerySearch criterias)
        {
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
