using Microsoft.Win32;
using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
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
    public partial class BookDetailView : UserControlBase
    {
        /*-- Constructor --*/

        public BookDetailView(Book book, bool isNew)
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init detail --*/

            Book = book;
            IsNew = isNew;
            InitDetail();

            /*-- Init list --*/

            Categories = new ObservableCollection<Category>(App.Model.Categories);
            InitInnerClass();

            /*-- Init command --*/

            Save = new RelayCommand(SaveAction, CanSaveAndCatModified);
            Cancel = new RelayCommand(CancelAction, CanSaveAndCatModified);
            Delete = new RelayCommand(DeleteAction, () => !IsNew);
            LoadImage = new RelayCommand(LoadImageAction);
            ClearImage = new RelayCommand(ClearImageAction, () => PicturePath != null);
            Add = new RelayCommand(AddCopiesAction, CanAddCopies);
            Modified = new RelayCommand(ModifiedAction);

            /*-- Init listener --*/
            App.Register(this, AppMessages.MSG_REFRESH_DETAILS_VIEW, () =>
            {
                BookCopies = InitBookCopies();
                Categories = new ObservableCollection<Category>(App.Model.Categories);
                InitInnerClass();
            });
        }

        /*-- Inner class --*/

        public class CategoryView
        {
            public CategoryView() { }
            public string Name { get; set; }
            public bool IsChecked { get; set; }
            public bool IsEnabled { get => App.CurrentUser.IsAdmin(); }
        }

        public void InitInnerClass()
        {
            CategoriesView = new ObservableCollection<CategoryView>();

            foreach (var c in Categories)
            {
                var catView = new CategoryView()
                { Name = c.Name, IsChecked = c.HasBook(Book) };

                CategoriesView.Add(catView);
            }
        }

        public void SaveUpdateCategories()
        {
            for (int k = 0; k < Categories.Count(); k++)
            {
                if (CategoriesView[k].IsChecked)
                {
                    Book.AddCategory(Categories[k]);
                }
                else
                {
                    Book.RemoveCategory(Categories[k]);
                }
            }
        }

        /*-- Command --*/

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Delete { get; set; }
        public ICommand LoadImage { get; set; }
        public ICommand ClearImage { get; set; }
        public ICommand Add { get; set; }
        public ICommand Modified { get; set; }

        /*-- Observable list --*/

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get => categories;
            set => SetProperty<ObservableCollection<Category>>(ref categories, value);
        }

        private ObservableCollection<BookCopy> bookCopies;
        public ObservableCollection<BookCopy> BookCopies
        {
            get => bookCopies;
            set => SetProperty<ObservableCollection<BookCopy>>(ref bookCopies, value);
        }

        private ObservableCollection<CategoryView> categoriesView;
        public ObservableCollection<CategoryView> CategoriesView
        {
            get => categoriesView;
            set => SetProperty<ObservableCollection<CategoryView>>(ref categoriesView, value);
        }

        /*-- Data init --*/

        public Book Book { get; set; }
        private ImageHelper imageHelper { get; set; }

        private bool isNew;
        public bool IsNew
        {
            get { return isNew; }
            set
            {
                isNew = value;
                RaisePropertyChanged(nameof(IsNew));
            }
        }

        /*-- IsModified --*/

            public bool CategoriesIsModified { get; set; }

        /*-- Binding --*/

        private string title;
        public string Title
        {
            get { return Book.Title; }
            set
            {
                Book.Title = value;
                SetProperty<string>(ref title, value, () => Validate());
                App.NotifyColleagues(AppMessages.MSG_TITLE_CHANGED, string.IsNullOrEmpty(value) ? "<new book>" : value);
            }
        }

        private string isbn;
        public string Isbn
        {
            get { return Book.Isbn; }
            set
            {
                Book.Isbn = value;
                SetProperty<string>(ref isbn, value, () => Validate());
            }
        }

        private string author;
        public string Author
        {
            get { return Book.Author; }
            set
            {
                Book.Author = value;
                SetProperty<string>(ref author, value, () => Validate());
            }
        }

        private string editor;
        public string Editor
        {
            get { return Book.Editor; }
            set
            {
                Book.Editor = value;
                SetProperty<string>(ref editor, value, () => Validate());
            }
        }

        public string PicturePath
        {
            get { return Book.AbsolutePicturePath; }
            set
            {
                Book.PicturePath = value;
                RaisePropertyChanged(nameof(PicturePath));
            }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                RaisePropertyChanged(nameof(Date));
            }
        }

        private int qty;
        public int Qty
        {
            get { return qty; }
            set
            {
                qty = value;
                RaisePropertyChanged(nameof(Qty));
            }
        }

        /*-- Properties user connected --*/

        public bool Enabled { get => App.CurrentUser.IsAdmin(); }
        public bool IsReadOnly { get => !App.CurrentUser.IsAdmin(); }

        /*-- Méthods --*/

        /*-- For action --*/

        private void SaveAction()
        {
            if (IsNew)
            {
                App.Model.Books.Add(Book);
                IsNew = false;
            }
            imageHelper.Confirm(Book.Isbn);
            PicturePath = imageHelper.CurrentFile;

            SaveUpdateCategories();
            CategoriesIsModified = false;

            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_REFRESH_CATEGORIES_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BASKET_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_RENTALS_VIEW);
        }

        private void CancelAction()
        {
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
            }
            if (IsNew)
            {
                Isbn = null;
                Title = null;
                Author = null;
                Editor = null;
                PicturePath = imageHelper.CurrentFile;
                RaisePropertyChanged(nameof(Book));
                InitInnerClass();
            }
            else
            {
                var change = (from c in App.Model.ChangeTracker.Entries<Book>()
                              where c.Entity == Book
                              select c).FirstOrDefault();
                if (change != null)
                {
                    change.Reload();
                    RaisePropertyChanged(nameof(Isbn));
                    RaisePropertyChanged(nameof(Title));
                    Title = Title;
                    RaisePropertyChanged(nameof(Author));
                    RaisePropertyChanged(nameof(Editor));
                    RaisePropertyChanged(nameof(PicturePath));
                    InitInnerClass();
                }
            }
            CategoriesIsModified = false;
        }

        private void DeleteAction()
        {
            this.CancelAction();
            if (File.Exists(PicturePath))
            {
                File.Delete(PicturePath);
            }
            Book.Delete();
            App.NotifyColleagues(AppMessages.MSG_REFRESH_CATEGORIES_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BASKET_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_RENTALS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_CLOSE_TAB, this);

        }

        private void LoadImageAction()
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == true)
            {
                imageHelper.Load(fd.FileName);
                PicturePath = imageHelper.CurrentFile;
            }
        }

        private void ClearImageAction()
        {
            imageHelper.Clear();
            PicturePath = imageHelper.CurrentFile;
        }

        private void AddCopiesAction()
        {
            if(Qty > 0)
            {
                Book.AddCopies(Qty, Date);
                BookCopies = InitBookCopies();
                Qty = 0;
                App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            }
        }

        private void ModifiedAction()
        {
            if (!IsNew)
            {
                CategoriesIsModified = true;
            }
        }

            /*-- For logic --*/

        private bool CanSaveAndCatModified()
        {
            return CategoriesIsModified || CanSaveOrCancelAction();
        }

        private bool CanSaveOrCancelAction()
        {
            if (IsNew)
            {
                return !string.IsNullOrEmpty(Title) && !HasErrors;
            }
            var change = (from c in App.Model.ChangeTracker.Entries<Book>()
                          where c.Entity == Book
                          select c).FirstOrDefault();
            return (change != null && change.State != EntityState.Unchanged);
        }

        private bool CanAddCopies()
        {
            return Qty > 0 && !IsNew;
        }

        /*-- For validate --*/

        public override bool Validate()
        {
            ClearErrors();

            /*-- Validation Isbn --*/

            if (string.IsNullOrEmpty(Isbn))
            {
                AddError("Isbn", Properties.Resources.Error_Required);
            }
            else
            {
                if (Isbn.Length < 3)
                {
                    AddError("Isbn", Properties.Resources.Error_LengthGreaterEqual3);
                }
                else if (Isbn.Length > 15)
                {
                    AddError("Isbn", Properties.Resources.Error_ToLong);
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(Isbn, "^[0-9]+$"))
                {
                    AddError("Isbn", Properties.Resources.Error_NumberOnly);
                }
                else if(App.Model.IsbnNotAvailable(Book.BookId, Isbn))
                {
                    AddError("Isbn", Properties.Resources.Error_NotAvailable);
                }
                
            }

            /*-- Validation Title --*/

            if (string.IsNullOrEmpty(Title))
            {
                AddError("Title", Properties.Resources.Error_Required);
            }
            else if (Title.Length > 30)
            {
                AddError("Title", Properties.Resources.Error_ToLong);
            }

            /*-- Validation Author --*/

            if (string.IsNullOrEmpty(Author))
            {
                AddError("Author", Properties.Resources.Error_Required);
            }
            else if (Author.Length > 30)
            {
                AddError("Author", Properties.Resources.Error_ToLong);
            }

            /*-- Validation Editor --*/

            if (string.IsNullOrEmpty(Editor))
            {
                AddError("Editor", Properties.Resources.Error_Required);
            }
            else if (Editor.Length > 30)
            {
                AddError("Editor", Properties.Resources.Error_ToLong);
            }

            RaiseErrors();
            return !HasErrors;
        }

        /*-- Tool --*/

        private ObservableCollection<BookCopy> InitBookCopies()
        {
            var filtered = Book.GetBookCopies();
            return new ObservableCollection<BookCopy>(filtered);
        }

        private void InitDetail()
        {
            imageHelper = new ImageHelper(App.IMAGE_PATH, Book.PicturePath);
            BookCopies = InitBookCopies();
            Qty = 0;
            CategoriesIsModified = false;
            Date = DateTime.Now;
        }
    }
}
