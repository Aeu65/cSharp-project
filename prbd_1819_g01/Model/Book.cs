using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g01
{
    public class Book : EntityBase<Model>
    {
        
        /*-- Contructor --*/

        protected Book() { }

        /*-- Model de base --*/

        [Key]
        public int BookId { get; set; }
        public string Isbn { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Editor { get; set; }
        public string PicturePath { get; set; }

        //public int NumAvailableCopies { get => Copies.Count(c => c.RentedBy == null); }
        public int NumAvailableCopies
        {
            get => (from c in this.Model.BookCopies
                    where c.Book.BookId == BookId &&
                    (from i in c.RentalItems where i.ReturnDate == null select i).Count() == 0
                    select c).Count();
        }

        public virtual ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        /*-- Method de base --*/

        public void AddCategory(Category category)
        {
            if (!Categories.Contains(category))
            {
                Categories.Add(category);
                Model.SaveChanges();
            }
        }

        public void AddCategories(Category[] cats)
        {
            foreach (var cat in cats) // Meowww
            {
                this.AddCategory(cat);
            }
        }

        public void RemoveCategory(Category category)
        {
            if (Categories.Contains(category))
            {
                Categories.Remove(category);
                Model.SaveChanges();
            }
        }

        public void AddCopies(int qty, DateTime date)
        {
            for (int i = 0; i < qty; i++)
            {
                var copy = Model.CreateBookCopy(this, date);
            }
        }

        public BookCopy GetAvailableCopy()
        {
            return (from c in this.Model.BookCopies
                    where c.Book.BookId == BookId &&
                    (from i in c.RentalItems where i.ReturnDate == null select i).Count() == 0
                    select c).FirstOrDefault();
        }

        public void DeleteCopy(BookCopy copy)
        {
            Model.BookCopies.Remove(copy);
            Model.SaveChanges();
        }

        public void Delete()
        {
            Model.Books.Remove(this);
            Model.SaveChanges();
        }

        /*-- Tool --*/

        public ICollection<BookCopy> GetBookCopies()
        {
            return Copies;
        }

        /*-- Ajout --*/

        public void AddCategoryByName(string name)
        {
            var cat = App.Model.Categories.SingleOrDefault(c => c.Name.Equals(name));
            Categories.Add(cat);
            Model.SaveChanges();
        }

        public void RemoveCategoryByName(string name)
        {
            var cat = App.Model.Categories.SingleOrDefault(c => c.Name.Equals(name));
            RemoveCategory(cat);
            Model.SaveChanges();
        }

        /*-- Layout --*/

        public string AbsolutePicturePath
        {
            get { return PicturePath != null ? App.IMAGE_PATH + "\\" + PicturePath : null; }
        }

        public string PrintEditor
        {
            get { return "(" + Editor + ")"; }
        }

        public string PrintCategories
        {
            get
            {
                var cat = "";
                foreach (var c in Categories)
                {
                    cat = String.Concat(cat, c.Name + " ");
                }
                return cat;
            }
        }

        public string PrintAvailavilities
        {
            get
            {
                if (NumAvailableCopies == null)
                {
                    return 0 + " copies available";
                }
                else
                {
                    return NumAvailableCopies + " copies available";
                }
            }
        }
    }
}
