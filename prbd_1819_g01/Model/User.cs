using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g01
{
    public enum Role { Member, Manager, Admin }

    public class User : EntityBase<Model>
    {

        /*-- Constructor --*/

        protected User() { }

        /*-- Model de base --*/

        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public Role Role { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

        /*-- Method de base --*/

        [NotMapped]
        public Rental Basket /*{ get; set; }*/
        {
            get { return Rentals.FirstOrDefault(r => r.RentalDate == null); }
            //get
            //{
            //    var basket = (from r in Model.Rentals
            //                  where r.User.UserId == UserId && r.RentalDate == null
            //                  select r).FirstOrDefault();
            //    return basket;
            //}
        }

        public Rental CreateBasket()
        {
            var basket = Basket;
            if (basket == null)
            {
                basket = Model.CreateRental(this);
            }
            return basket;
        }

        public RentalItem AddToBasket(Book book)
        {
            CreateBasket();
            BookCopy copyAvailable = book.GetAvailableCopy();
            if (copyAvailable != null)
            {
                RentalItem ri = Basket.RentCopy(copyAvailable);
                return ri;
            }
            return null;
        }

        public void RemoveFromBasket(Book book)
        {
            if (Basket != null)
            {
                Basket.RemoveItem(Basket.Items.SingleOrDefault(ri => ri.BookCopy.Book == book));
                Model.SaveChanges();
            }

        }

        public void RemoveFromBasket(RentalItem Item)
        {
            Basket.RemoveItem(Item);
        }

        public void ClearBasket()
        {
            Basket.Clear();
            Model.SaveChanges();
        }

        public void ConfirmBasket()
        {
            if (Basket.Items.Any())
            {
                Basket.Confirm();
            }
        }

        public void Return(BookCopy copy)
        {
            copy.RentalItems.SingleOrDefault(ri => ri.BookCopy == copy).DoReturn();
            Model.SaveChanges();
        }

        /*-- Ajout --*/

        public int NbBookInBasket(Book book)
        {
            if (Basket == null)
            {
                return 0;
            }
            else
            {
                return Basket.Items.Count(ri => ri.BookCopy.Book == book);
            }
        }
        
        public bool IsAdmin()
        {
            return Role.ToString().Equals("Admin");
        }

        public bool IsManager()
        {
            return Role.ToString().Equals("Manager");
        }

        public bool IsMember()
        {
            return Role.ToString().Equals("Member");
        }

        /*-- Layout --*/

        public string PrintLog()
        {
            return "Logged : " + UserName + " (" + Role + ")";
        }

        public string PrintEmail
        {
            get { return "Email : " + Email; }
        }

        public string PrintRole
        {
            get { return "Role : " + Role; }
        }

        public string PrintRentals
        {
            get { return "Number rentals : " + (Rentals.Count(r => r.RentalDate != null)); }
        }

        public string PrintBasket
        {
            get { return "Basket : " + (Basket != null ? "Ouvert" : "Fermé"); }
        }
    }
}
