using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace prbd_1819_g01
{
    public partial class RentalsView : UserControlBase
    {
        /*-- Constructor --*/

        public RentalsView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init list --*/

            IniRentalsIfAdminOrMember();
            RentalItems = null;

            /*-- Init command --*/

            ReturnOrCancel = new RelayCommand<RentalItem>(rentalItem =>
            {
                ReturnOrCancelAction(rentalItem);
                UpdateBookCopiesList();
            });
            Delete = new RelayCommand<RentalItem>(rentalItem =>
            {
                DeleteFromRentalAction(rentalItem);
                UpdateBookCopiesList();
            });

            /*-- Init listener --*/

            App.Register(this, AppMessages.MSG_REFRESH_RENTALS_VIEW, () =>
            {
                App.Model.RemoveRentalEmpty();
                IniRentalsIfAdminOrMember();
                UpdateBookCopiesList();
            });
        }

        /*-- Command --*/

        public ICommand ReturnOrCancel { get; set; }
        public ICommand Delete { get; set; }

        /*-- Observable list --*/

        private ObservableCollection<Rental> rentals;
        public ObservableCollection<Rental> Rentals
        {
            get => rentals;
            set => SetProperty<ObservableCollection<Rental>>(ref rentals, value);
        }

        private ObservableCollection<RentalItem> rentalItems;
        public ObservableCollection<RentalItem> RentalItems
        {
            get => rentalItems;
            set => SetProperty<ObservableCollection<RentalItem>>(ref rentalItems, value);
        }

        /*-- Selected rental --*/

        private Rental rentalSelected;
        public Rental RentalSelected
        {
            get => rentalSelected;
            set => SetProperty<Rental>(ref rentalSelected, value, UpdateBookCopiesList);
        }

        private string iconPathCorb;
        public string IconPathCorb
        {
            get { return App.ICON_PATH + "\\" + "000.png"; }
            set
            {
                iconPathCorb = value;
                RaisePropertyChanged(nameof(IconPathCorb));
            }
        }

        /*-- Properties user connected --*/

        public string ColumnToHide { get => App.CurrentUser.IsAdmin() ? "" : "2"; }

        /*-- Méthod --*/

            /*-- For filter --*/

        private void UpdateBookCopiesList()
        {
            if (RentalSelected == null)
            {
                RentalItems = null;
            }
            else
            {
                RentalItems = new ObservableCollection<RentalItem>(RentalSelected.Items);
            }
        }

            /*-- For action --*/

        private void ReturnOrCancelAction(RentalItem ri)
        {
            if (ri.ReturnDate == null)
            {
                ri.DoReturn();
                IniRentalsIfAdminOrMember();
            }
            else
            {
                ri.CancelReturn();
                IniRentalsIfAdminOrMember();
            }
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_DETAILS_VIEW);
        }

        private void DeleteFromRentalAction(RentalItem ri)
        {
            if (ri == null) { }
            else
            {
                ri.Remove();
                IniRentalsIfAdminOrMember();
                App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
                App.NotifyColleagues(AppMessages.MSG_REFRESH_DETAILS_VIEW);
            }
            if (!RentalSelected.Items.Any())
            {
                App.Model.RemoveRental(RentalSelected);
                IniRentalsIfAdminOrMember();
                App.NotifyColleagues(AppMessages.MSG_REFRESH_USERS_VIEW);
            }
        }

        /*-- Tool --*/

        private void IniRentalsIfAdminOrMember()
        {
            Rentals = App.CurrentUser.IsAdmin() ? new ObservableCollection<Rental>(App.Model.GetActiveRental())
                : new ObservableCollection<Rental>(App.Model.GetActiveRentalFromUser(App.CurrentUser));
        }
    }
}
