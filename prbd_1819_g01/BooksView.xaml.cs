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
    public partial class BooksView : UserControlBase
    {
        /*-- Constructor --*/

        public BooksView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init CurrentUser --*/
            
            UserSelected = App.CurrentUser;

            /*-- Init list --*/

            Books = new ObservableCollection<Book>(App.Model.Books);
            Categories = new ObservableCollection<Category>(App.Model.Categories);
            Categories.Insert(0, App.Model.CreateCategoryAll());
            CategoryIndex = 0;

            /*-- Init command --*/

            NewBook = new RelayCommand(() => {
                App.NotifyColleagues(AppMessages.MSG_NEW_BOOK);
            });
            ClearFilter = new RelayCommand(() => {
                Filter = "";
                CategoryIndex = 0;
            });
            DisplayBookDetails = new RelayCommand<Book>(book => {
                App.NotifyColleagues(AppMessages.MSG_DISPLAY_BOOK, book);
            });
            Add = new RelayCommand<Book>(book => {
                AddAction(book);
            },book => CanAddAction(book));

            /*-- Init listener --*/

            App.Register(this, AppMessages.MSG_REFRESH_BOOKS_VIEW, () =>
            {
                ApplyFilterAction();
                RefreshCategories();
            });
           
            App.Register<User>(this, AppMessages.MSG_REFRESH_BOOKS_VIEW_USER, userBasket =>
            {
                UserSelected = userBasket;
                NameOwnerBasket = userBasket.FullName;
            });
        }

        /*-- Command --*/

        public ICommand NewBook { get; set; }
        public ICommand ClearFilter { get; set; }
        public ICommand DisplayBookDetails { get; set; }
        public ICommand Add { get; set; }

        /*-- Observable list --*/

        private ObservableCollection<Book> books;
        public ObservableCollection<Book> Books
        {
            get => books;
            set => SetProperty<ObservableCollection<Book>>(ref books, value);
        }

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get => categories;
            set => SetProperty<ObservableCollection<Category>>(ref categories, value);
        }

        /*-- Current User --*/

        private User userSelected;
        public User UserSelected { get; set; }

        /*-- Selected item category --*/

        private Category categorySelected;
        public Category CategorySelected
        {
            get => categorySelected;
            set => SetProperty<Category>(ref categorySelected, value, ApplyFilterAction);
        }

        private int categoryIndex;
        public int CategoryIndex
        {
            get { return categoryIndex; }
            set
            {
                categoryIndex = value;
                RaisePropertyChanged(nameof(categoryIndex));
            }
        }

        private Category categoryClicked;
        public Category CategoryClicked
        {
            get => categoryClicked;
            set => SetProperty<Category>(ref categoryClicked, value, ClickedCategoryAction);
        }

        /*-- Binding textBox --*/

        private string filter;
        public string Filter
        {
            get => filter;
            set => SetProperty<string>(ref filter, value, ApplyFilterAction);
        }

        /*-- Binding label NameOwnerBasket --*/

        private string nameOwnerBasket;
        public string NameOwnerBasket
        {
            get { return App.CurrentUser.Equals(UserSelected) ? "Add to your basket" : "Add to " + nameOwnerBasket + " basket"; }
            set => SetProperty<string>(ref nameOwnerBasket, value);
        }

        public bool ButtonNewBookDisplay { get => App.CurrentUser.IsAdmin(); }

        /*-- Méthods --*/

            /*-- For filter && selectedCategory --*/

        private void ApplyFilterAction()
        {
            var Category = CategorySelected != null ? CategorySelected.Name : "All";
            if (Category.Equals("All"))
            {
                if (!string.IsNullOrEmpty(Filter))
                {
                    var filtered = App.Model.FindBooksByText(Filter);
                    Books = new ObservableCollection<Book>(filtered);
                }
                else
                {
                    Books = new ObservableCollection<Book>(App.Model.Books);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Filter))
                {
                    var filtered = from b in App.Model.FindBooksByText(Filter)
                                   where b.Categories.Any(c => c.Name.Equals(Category))
                                   select b;
                    Books = new ObservableCollection<Book>(filtered);
                }
                else
                {
                    var filtered = from b in App.Model.Books
                                   where (b.Categories.Any(c => c.Name.Equals(Category)))
                                   select b;
                    Books = new ObservableCollection<Book>(filtered);
                }
            }
        }

        private void ClickedCategoryAction()
        {
            if(CategoryClicked != null)
            {
                CategorySelected = CategoryClicked;
            }
        }

            /*-- For action --*/

        private void AddAction(Book book)
        {
            UserSelected.AddToBasket(book);
            ApplyFilterAction();
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BASKET_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_USERS_VIEW);
        }

            /*-- For logic --*/

        private bool CanAddAction(Book book)
        {
            return book != null && 
                book.NumAvailableCopies > 0 &&
                1 > UserSelected.NbBookInBasket(book);
        }

        /*-- Tool --*/

        private void RefreshCategories()
        {
            Categories = new ObservableCollection<Category>(App.Model.Categories);
            Categories.Insert(0, App.Model.CreateCategoryAll());
            CategoryIndex = 0;
        }
    }
}
