using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string CommentText { get; set; }
    }
}
