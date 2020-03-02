using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class User
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
    }
}
