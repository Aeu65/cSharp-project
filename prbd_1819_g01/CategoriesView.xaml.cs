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
    public partial class CategoriesView : UserControlBase
    {

        /*-- Constructor --*/

        public CategoriesView()
        {
            /*-- Init --*/

            InitializeComponent();
            DataContext = this;

            /*-- Init list --*/

            Categories = new ObservableCollection<Category>(App.Model.Categories);

            /*-- Init command --*/

            Add = new RelayCommand(AddAction, CanAddAction);
            Update = new RelayCommand(UpdateAction, CanUpdateOrCancelAction);
            Cancel = new RelayCommand(CancelAction, CanUpdateOrCancelAction);
            Delete = new RelayCommand(DeleteAction, CanDeleteAction);

            /*-- Init listener --*/

            App.Register(this, AppMessages.MSG_REFRESH_CATEGORIES_VIEW, () =>
            {
                Categories = new ObservableCollection<Category>(App.Model.Categories);
            });
        }

        /*-- Command --*/

        public ICommand Add { get; set; }
        public ICommand Update { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Delete { get; set; }

        /*-- Observable list --*/

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get => categories; 
            set => SetProperty<ObservableCollection<Category>>(ref categories, value);
        }

        /*-- Selected item --*/

        private Category categorySelected;
        public Category CategorySelected
        {
            get => categorySelected;
            set => SetProperty<Category>(ref categorySelected, value, UpdateTextCategory);
        }

        /*-- Binding textBox --*/

        private string textCategory;
        public string TextCategory
        {
            get => textCategory;
            set
            {
                textCategory = value;
                RaisePropertyChanged(nameof(TextCategory));
            }
        }

        /*-- Properties user connected --*/

        public bool ActionsCategoriesDisplay { get => App.CurrentUser.IsAdmin(); }

        /*-- Méthods --*/

            /*-- For binding--*/

        private void UpdateTextCategory()
        {
            if (CategorySelected != null)
            {
                TextCategory = CategorySelected.Name;
            }
            else
            {
                TextCategory = "";
            }
        }

            /*-- For action --*/

        private void AddAction()
        {
            var cat = App.Model.CreateCategory(TextCategory);
            Categories.Add(cat);
            CategorySelected = cat;
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_DETAILS_VIEW);
        }

        private void UpdateAction()
        {
            App.Model.UpdateCategory(CategorySelected, TextCategory);
            Categories = new ObservableCollection<Category>(App.Model.Categories);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_DETAILS_VIEW);
        }

        private void CancelAction()
        {
            TextCategory = CategorySelected.Name;
        }

        private void DeleteAction()
        {
            App.Model.RemoveCategory(CategorySelected);
            Categories.Remove(CategorySelected);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_BOOKS_VIEW);
            App.NotifyColleagues(AppMessages.MSG_REFRESH_DETAILS_VIEW);
        }

            /*-- For logic--*/

        private bool CanAddAction()
        {
            return TextCategoryIsNotNull() &&
                   TextCategoryIsNotEmpty() &&
                   AlreadyNotExists();
        }

        private bool CanUpdateOrCancelAction()
        {
            return ElemIsSelected() &&
                   TextCategoryIsNotNull() &&
                   AlreadyNotExists();
        }

        private bool CanDeleteAction()
        {
            return ElemIsSelected() &&
                   ElemEqualsTextBox();
        }

        /*-- Tool --*/

        private bool ElemIsSelected()
        {
            return CategorySelected != null;
        }

        private bool TextCategoryIsNotNull()
        {
            return TextCategory != null;
        }

        private bool TextCategoryIsNotEmpty()
        {
            return !TextCategory.Equals("");
        }

        private bool ElemEqualsTextBox()
        {
            return CategorySelected.Name.Equals(TextCategory);
        }

        private bool AlreadyNotExists()
        {
            return !Categories.Any
                (c => c.Name.ToUpper().Equals(TextCategory.ToUpper()));
        }
    }
}
