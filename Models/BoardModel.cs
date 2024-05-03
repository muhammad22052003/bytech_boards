namespace bytech_boards.Models
{
    public class BoardModel : IModel
    {
        public string Id {  get; set; }

        public string lastUpdateId {  get; set; }

        public string Name { get; set; }

        public string PathToFile { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
