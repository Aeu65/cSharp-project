using PRBD_Framework;
using System;
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
    public partial class MainView : WindowBase
    {
        /*-- Constructor --*/

        public MainView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init CurrentUser --*/

            PrintUserLogged = App.CurrentUser.PrintLog();

            /*-- Init command --*/

            Logout = new RelayCommand(() => LogoutAction());

            /*-- Init listener --*/

            App.Register(this, AppMessages.MSG_NEW_BOOK, () => {
                var book = App.Model.Books.Create();
                //App.Model.Books.Add(book);
                AddTab(book, true);
            });
            App.Register<Book>(this, AppMessages.MSG_DISPLAY_BOOK, book => {
                if (book != null)
                {
                    var tab = (from TabItem t in View_tabControl.Items where (string)t.Header == book.Title select t).FirstOrDefault();
                    if (tab == null)
                        AddTab(book, false);
                    else
                        Dispatcher.InvokeAsync(() => tab.Focus());
                }
            });
            App.Register<string>(this, AppMessages.MSG_TITLE_CHANGED, (s) => {
                (View_tabControl.SelectedItem as TabItem).Header = s;
            });
            App.Register<UserControlBase>(this, AppMessages.MSG_CLOSE_TAB, ctl => {
                var tab = (from TabItem t in View_tabControl.Items where t.Content == ctl select t).SingleOrDefault();
                ctl.Dispose();
                View_tabControl.Items.Remove(tab);
                View_tabControl.SelectedIndex = 0;
            });
        }

        /*-- Command --*/

        public ICommand Logout { get; set; }

        /*-- Data binding --*/

        private string printUserLogged;
        public string PrintUserLogged
        {
            get => printUserLogged;
            set => SetProperty<string>(ref printUserLogged, value);
        }

        /*-- Properties user connected --*/

        public bool UsersDisplay { get => App.CurrentUser.IsAdmin() ? true : false; }

        /*-- Méthod --*/

            /*-- For addTab --*/

        private void AddTab(Book book, bool isNew)
        {
            var ctl = new BookDetailView(book, isNew);
            var tab = new TabItem()
            {
                Header = isNew ? "<new book>" : book.Title,
                Content = ctl
            };
            tab.MouseDown += (o, e) => {
                if (e.ChangedButton == MouseButton.Middle &&
                    e.ButtonState == MouseButtonState.Pressed)
                {
                    View_tabControl.Items.Remove(o);
                    View_tabControl.SelectedIndex = 0;
                    (tab.Content as UserControlBase).Dispose();
                }
            };
            tab.PreviewKeyDown += (o, e) => {
                if (e.Key == Key.W && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    View_tabControl.Items.Remove(o);
                    View_tabControl.SelectedIndex = 0;
                    (tab.Content as UserControlBase).Dispose();
                }
            };
            View_tabControl.Items.Add(tab);
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

            /*-- For logout --*/ 

        private void LogoutAction()
        {
            App.CurrentUser = null;
            ShowLoginView();
            Close();
        }

        private void ShowLoginView()
        {
            var loginView = new LoginView();
            loginView.Show();
            Application.Current.MainWindow = loginView;
        }
    }
}
