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
    public class RentalItem : EntityBase<Model>
    {

        /*-- Constructor --*/

        protected RentalItem() { }

        /*-- Model de base --*/

        [Key]
        public int RentalItemId { get; set; }
        public DateTime? ReturnDate { get; set; }

        [Required]
        public virtual Rental Rental { get; set; }

        [Required]
        public virtual BookCopy BookCopy { get; set; }

        /*-- Method de base --*/

        public void DoReturn()
        {
            ReturnDate = DateTime.Now;
            BookCopy.RentedBy = null;
            Model.SaveChanges();
        }

        public void CancelReturn()
        {
            ReturnDate = null;
            BookCopy.RentedBy = Rental.User;
            Model.SaveChanges();
        }

        /*-- Ajout --*/

        public void Remove()
        {
            BookCopy.RentedBy = null;
            Rental.RemoveItem(this);
        }

        /*-- Layout --*/

        public string IconPath
        {
            get { return ReturnDate != null ? App.ICON_PATH + "\\" + "002.png"
                    : App.ICON_PATH + "\\" + "001.png"; }
        }
    }
}
