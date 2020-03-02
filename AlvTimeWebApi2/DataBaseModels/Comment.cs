using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Comment
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string CommentText { get; set; }
    }
}
