using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g01
{
    public class Rental : EntityBase<Model>
    {

        /*-- Constructor --*/

        protected Rental() { }

        /*-- Model de base --*/

        [Key]
        public int RentalId { get; set; }
        public DateTime? RentalDate { get; set; }

        [Required]
        public virtual User User { get; set; }

        public virtual ICollection<RentalItem> Items { get; set; } = new List<RentalItem>();

        public int NumOpenItems { get => Items.Count(ri => ri.ReturnDate == null); }

        /*-- Method de base --*/

        public RentalItem RentCopy(BookCopy copy)
        {
            RentalItem rentalItem = Model.CreateRentalItem(copy, this);
            Model.SaveChanges();
            return rentalItem;
        }

        public void RemoveCopy(BookCopy copy)
        {
            Items.Remove(Items.SingleOrDefault(ri => ri.BookCopy == copy));
            Model.SaveChanges();
        }

        public void RemoveItem(RentalItem rentalItem)
        {
            Model.RentalItems.Remove(rentalItem);
            Model.SaveChanges();
        }

        public void Return(RentalItem rentalItem)
        {
            rentalItem.ReturnDate = DateTime.Now;
            Model.SaveChanges();
        }

        public void Confirm()
        {
            RentalDate = DateTime.Now;
            foreach (var ri in Items)
            {
                ri.BookCopy.RentedBy = User;
            }
            Model.SaveChanges();
        }

        public void Clear()
        {
            while (Items.Count() > 0)
            {
                RemoveItem(Items.ElementAt(0));
            }
        }
    }
}
