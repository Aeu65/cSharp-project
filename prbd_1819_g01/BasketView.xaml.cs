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
    public partial class BasketView : UserControlBase
    {
        /*-- Constructor --*/

        public BasketView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init list --*/

            UserSelected = App.CurrentUser;
            Users = new ObservableCollection<User>(App.Model.Users);
            InitBasket();

            /*-- Init command --*/

            Delete = new RelayCommand<RentalItem>(rentalItem => {
                DeleteFromBasketAction(rentalItem);
                ApplyFilterAction();
            });
            Clear = new RelayCommand(ClearBasketAction, CanClearBasket);
            Confirm = new RelayCommand(ConfirmBasketAction, CanConfirmBasket);

            /*-- Init listener --*/

            App.Register(this, AppMessages.MSG_REFRESH_BASKET_VIEW, () =>
            {
                ApplyFilterAction();
            });
        }

        /*-- Command --*/

        public ICommand Confirm { get; set; }
        public ICommand Clear { get; set; }
        public ICommand Delete { get; set; }

        /*-- Observable list --*/

        private ObservableCollection<RentalItem> basket;
        public ObservableCollection<RentalItem> Basket
        {
            get => basket;
            set => SetProperty<ObservableCollection<RentalItem>>(ref basket, value);
        }

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get => users;
            set => SetProperty<ObservableCollection<User>>(ref users, value);
        }

        /*-- Selected User --*/

        private User userSelected;
        public User UserSelected
        {
            get => userSelected;
            set => SetProperty<User>(ref userSelected, value, ApplyFilterActionAndRefreshBooksView);
        }

        /*-- Properties user connected --*/

        public bool SelectUserDisplay { get => App.CurrentUser.IsAdmin(); }
        public int HeightIfAdmin { get => App.CurrentUser.IsAdmin() ? 30 : 0; }

        /*-- Méthod --*/

            /*-- For filter --*/

        private void ApplyFilterAction()
        {
            if(BasketIsCreated())
            {
                Basket = new ObservableCollection<RentalItem>();
            }
            else
            {
                Basket = new ObservableCollection<RentalItem>(UserSelected.Basket.Items);
            }
        }

        private void ApplyFilterActionAndRefreshBooksView()
        {
            ApplyFilterAction();
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW_USER, UserSelected);
        }

        /*-- For action --*/

        private void DeleteFromBasketAction(RentalItem rentalItem)
        {
            UserSelected.RemoveFromBasket(rentalItem);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
        }

        private void ClearBasketAction()
        {
            UserSelected.ClearBasket();
            ApplyFilterAction();
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
        }

        private void ConfirmBasketAction()
        {
            UserSelected.ConfirmBasket();
            ApplyFilterAction();
            App.NotifyColleagues(AppMessages.MSG_REFRESH_RENTALS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_DETAILS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_USERS_VIEW);
        }

            /*-- For logic --*/

        private bool CanClearBasket()
        {
            return !BasketIsCreated() &&
                UserSelected.Basket.Items.Count() > 0;
        }

        private bool CanConfirmBasket()
        {
            return UserSelected.Basket != null && UserSelected.Basket.Items.Count() > 0;
        }

        /*-- Tool --*/

        private bool BasketIsCreated()
        {
            return UserSelected.Basket == null;
        }

        private void InitBasket()
        {
            if(UserSelected.Basket != null)
            {
                Basket = new ObservableCollection<RentalItem>(UserSelected.Basket.Items);
            }
            else
            {
                Basket = new ObservableCollection<RentalItem>();
            }
        }
    }
}
