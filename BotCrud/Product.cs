using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotCrud
{
    [Table("products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("product_id")]
        public int Id { get; set; }

        [Column("product_name")]
      
        public  string Name { get; set; }
        
        [Column("price")]
        public decimal Price { get; set; }

        [Column("img")]
        public string Img { get; set; } = "No photo";

        [Column("expire_date")]
        public DateTime ExpireDate { get; set; }

        [Column("category_name")]
        public Category CategoryName { get; set; }
    }

    public enum Category : byte
    {
        Vegetables,
        Fruits,
        Drinks,
        MilkyProducts
    }
}
