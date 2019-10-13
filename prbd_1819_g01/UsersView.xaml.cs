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
    public partial class UsersView : UserControlBase
    {
        /*-- Constructor --*/

        public UsersView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init list --*/

            Users = new ObservableCollection<User>(App.Model.Users);

            App.Register(this, AppMessages.MSG_REFRESH_USERS_VIEW, () =>
            {
                Users = new ObservableCollection<User>(App.Model.Users);
            });
        }

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get => users;
            set => SetProperty<ObservableCollection<User>>(ref users, value);
        }
    }
}
