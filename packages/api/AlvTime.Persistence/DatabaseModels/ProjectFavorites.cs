using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence;

public partial class ProjectFavorites
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; }

    public int ProjectId { get; set; }
    public virtual Project Project { get; set; }

    public int Index { get; set; }
}