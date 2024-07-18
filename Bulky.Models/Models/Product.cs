using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Bulky.Models
{
    public  class Product
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(1, 1000)]
        public double Price { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public string ImageUrl { get; set; }
    }
}
