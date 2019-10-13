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
    public class BookCopy : EntityBase<Model>
    {

        /*-- Constructor --*/

        protected BookCopy() { }

        /*-- Model de base --*/

        [Key]
        public int BookCopyId { get; set; }
        public DateTime AquisitionDate { get; set; }
        public User RentedBy { get; set; }

        [Required]
        public virtual Book Book { get; set; }

        public virtual ICollection<RentalItem> RentalItems { get; set; } = new List<RentalItem>();

        /*-- Ajout --*/

        public string PrintRentedBy { get => RentedBy != null ? RentedBy.UserName : null; }
    }
}
