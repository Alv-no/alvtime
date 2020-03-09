using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class TaskFavorites
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
    }
}
