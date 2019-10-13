using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRBD_Framework;
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
    public partial class LoginView : WindowBase
    {
        /*-- Constructor --*/

        public LoginView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init command --*/

            Login = new RelayCommand(LoginAction, () =>
            {
                return username != null && password != null && !HasErrors;
            });
            Cancel = new RelayCommand(() => Close());
        }

        /*-- Command --*/

        public ICommand Login { get; set; }
        public ICommand Cancel { get; set; }

        /*-- Data --*/

        private string username;
        public string Username {
            get => username;
            set => SetProperty<string>(ref username, value, () => Validate());
        }

        private string password;
        public string Password {
            get => password;
            set => SetProperty<string>(ref password, value, () => Validate());
        }

        /*-- Méthod --*/

            /*-- For action --*/
        private void LoginAction()
        {
            if(Validate())
            {
                var user = App.Model.Users.FirstOrDefault(u => u.UserName == Username);
                App.CurrentUser = user;
                ShowMainView();
                Close();
            }
        }
            
            /*-- For validate --*/

        public override bool Validate()
        {
            ClearErrors();
            var user = App.Model.Users.FirstOrDefault(u => u.UserName== Username);

            if (string.IsNullOrEmpty(Username))
            {
                AddError("Username", Properties.Resources.Error_Required);
            }
            else
            {
                if (Username.Length < 3)
                {
                    AddError("Username", Properties.Resources.Error_LengthGreaterEqual3);
                }
                else
                {
                    if (user == null)
                    {
                        AddError("Username", Properties.Resources.Error_DoesNotExist);
                    }
                }
            }

            if (string.IsNullOrEmpty(Password))
            {
                AddError("Password", Properties.Resources.Error_Required);
            }
            else
            {
                if (Password.Length < 3)
                {
                    AddError("Password", Properties.Resources.Error_LengthGreaterEqual3);
                } else if (user != null && user.Password != Password) {
                    AddError("Password", Properties.Resources.Error_WrongPassword);
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

            /*-- For showMainView --*/

        private static void ShowMainView()
        {
            var mainView = new MainView();
            mainView.Show();
            Application.Current.MainWindow = mainView;
        }
    }
}
