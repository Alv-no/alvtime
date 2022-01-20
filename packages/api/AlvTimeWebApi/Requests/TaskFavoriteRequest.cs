namespace AlvTimeWebApi.Requests
{
    public class TaskFavoriteRequest
    {
        public int Id { get; set; }
        public bool Favorite { get; set; }
        public bool? Locked { get; set; }
        public string Name { get; set; }
        public decimal? CompensationRate { get; set; }
    }
}