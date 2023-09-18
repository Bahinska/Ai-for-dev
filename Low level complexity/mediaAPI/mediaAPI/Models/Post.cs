namespace mediaAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int Author { get; set; }
        public int Likes { get; set; }

        public Post()
        {
        }
    }
}
