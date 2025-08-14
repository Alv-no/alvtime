namespace AlvTimeWebApi.Requests
{
    public class TaskFavoriteRequest
    {
        public int Id { get; set; }
        public bool Favorite { get; set; }
        public bool EnableComments { get; set; }
    }
}