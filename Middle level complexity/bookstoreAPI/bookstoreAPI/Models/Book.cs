using System.ComponentModel.DataAnnotations.Schema;

namespace bookstoreAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required Author Author { get; set; }
        public required Genre Genre { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
