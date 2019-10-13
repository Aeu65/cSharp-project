using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g01
{
    public class Category : EntityBase<Model>
    {

        /*-- Constructor --*/

        protected Category() { }

        /*-- Model de base --*/

        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        /*-- Method de base --*/

        public bool HasBook(Book book)
        {
            return Books.Contains(book);
        }

        public void AddBook(Book book)
        {
            Books.Add(book);
            Model.SaveChanges();
        }

        public void RemoveBook(Book book)
        {
            Books.Remove(book);
            Model.SaveChanges();
        }

        public void Delete()
        {
            Model.Categories.Remove(this);
            Model.SaveChanges();
        }
    }
}
