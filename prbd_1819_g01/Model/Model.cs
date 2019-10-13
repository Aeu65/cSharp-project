using PRBD_Framework;
using MySql.Data.EntityFramework;
using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

namespace prbd_1819_g01 {
    public enum DbType { MsSQL, MySQL }
    public enum EFDatabaseInitMode { CreateIfNotExists, DropCreateIfChanges, DropCreateAlways }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlModel : Model
    {
        public MySqlModel(EFDatabaseInitMode initMode) : base("name=library-mysql")
        {
            switch (initMode)
            {
                case EFDatabaseInitMode.CreateIfNotExists:
                    Database.SetInitializer<MySqlModel>(new CreateDatabaseIfNotExists<MySqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateIfChanges:
                    Database.SetInitializer<MySqlModel>(new DropCreateDatabaseIfModelChanges<MySqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateAlways:
                    Database.SetInitializer<MySqlModel>(new DropCreateDatabaseAlways<MySqlModel>());
                    break;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // see: https://blog.craigtp.co.uk/Post/2017/04/05/Entity_Framework_with_MySQL_-_Booleans,_Bits_and_%22String_was_not_recognized_as_a_valid_boolean%22_errors.
            modelBuilder.Properties<bool>().Configure(c => c.HasColumnType("bit"));
        }

        public override void Reseed(string tableName)
        {
            Database.ExecuteSqlCommand($"ALTER TABLE {tableName} AUTO_INCREMENT=1");
        }
    }

    public class MsSqlModel : Model
    {
        public MsSqlModel(EFDatabaseInitMode initMode) : base("name=library-mssql")
        {
            switch (initMode)
            {
                case EFDatabaseInitMode.CreateIfNotExists:
                    Database.SetInitializer<MsSqlModel>(new CreateDatabaseIfNotExists<MsSqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateIfChanges:
                    Database.SetInitializer<MsSqlModel>(new DropCreateDatabaseIfModelChanges<MsSqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateAlways:
                    Database.SetInitializer<MsSqlModel>(new DropCreateDatabaseAlways<MsSqlModel>());
                    break;
            }
        }

        public override void Reseed(string tableName)
        {
            Database.ExecuteSqlCommand($"DBCC CHECKIDENT('{tableName}', RESEED, 0)");
        }
    }

    public abstract class Model : DbContext
    {
        protected Model(string name) : base(name) { }

        public static Model CreateModel(DbType type, EFDatabaseInitMode initMode = EFDatabaseInitMode.DropCreateIfChanges)
        {
            Console.WriteLine($"Creating model for {type}\n");
            switch (type)
            {
                case DbType.MsSQL:
                    return new MsSqlModel(initMode);
                case DbType.MySQL:
                    return new MySqlModel(initMode);
                default:
                    throw new ApplicationException("Undefined database type");
            }
        }

        public abstract void Reseed(string tableName);

        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalItem> RentalItems { get; set; }
        public DbSet<User> Users { get; set; }

        public void ClearDatabase()
        {
#if MSSQL
            Categories.RemoveRange(Categories);
            RentalItems.RemoveRange(RentalItems);
            BookCopies.RemoveRange(BookCopies);
            Books.RemoveRange(Books);
            Rentals.RemoveRange(Rentals);
            Users.RemoveRange(Users);
#else
            Categories.RemoveRange(Categories);
            BookCopies.RemoveRange(BookCopies
                .Include(nameof(BookCopy.RentalItems))
            );
            Books.RemoveRange(Books);
            Users.RemoveRange(Users
                .Include(nameof(User.Rentals))
            );
#endif
            SaveChanges();

            Reseed(nameof(Users));
            Reseed(nameof(Books));
            Reseed(nameof(BookCopies));
            Reseed(nameof(Categories));
            Reseed(nameof(Rentals));
            Reseed(nameof(RentalItems));
        }

        public void CreateTestData(DbType type)
        {
            TestDatas test = new TestDatas(type);
            test.Run();
        }

        /*-- Constructor --*/

        public User CreateUser(
            string username,
            string password,
            string fullName,
            string email,
            DateTime? birthdate = null,
            Role role = Role.Member)
        {
            User u = Users.Create();
            u.UserName = username;
            u.Password = password;
            u.FullName = fullName;
            u.Email = email;
            u.BirthDate = birthdate;
            u.Role = role;
            Users.Add(u);
            SaveChanges();
            return u;
        }

        public Book CreateBook(
            string isbn,
            string author,
            string title,
            string editor,
            int numCopies,
            string picturePath = null)
        {
            Book b = Books.Create();
            b.Isbn = isbn;
            b.Author = author;
            b.Title = title;
            b.Editor = editor;
            b.PicturePath = picturePath;
            Books.Add(b);
            b.AddCopies(numCopies, DateTime.Now);
            SaveChanges();
            return b;
        }

        public Category CreateCategory(
            string name)
        {
            Category c = Categories.Create();
            c.Name = name;
            Categories.Add(c);
            SaveChanges();
            return c;
        }

        public Category CreateCategoryAll()
        {
            Category c = Categories.Create();
            c.Name = "All";
            return c;
        }

        public Rental CreateRental(
            User user)
        {
            Rental r = Rentals.Create();
            r.User = user;
            Rentals.Add(r);
            SaveChanges();
            return r;
        }

        public RentalItem CreateRentalItem(
            BookCopy copy,
            Rental rental)
        {
            RentalItem ri = RentalItems.Create();
            ri.BookCopy = copy;
            ri.Rental = rental;
            RentalItems.Add(ri);
            SaveChanges();
            return ri;
        }

        public BookCopy CreateBookCopy(
            Book book,
            DateTime aquisitiondate)
        {
            BookCopy bc = BookCopies.Create();
            bc.Book = book;
            bc.AquisitionDate = aquisitiondate;
            BookCopies.Add(bc);
            SaveChanges();
            return bc;
        }

        /*-- Method --*/

        public List<Book> FindBooksByText(string key)
        {
            var data = from b in Books
                       where b.Isbn.Contains(key)
                            || b.Author.Contains(key)
                            || b.Title.Contains(key)
                            || b.Editor.Contains(key)
                       select b;
            return data.ToList();
        }

        public bool IsbnNotAvailable(int id, string isbn)
        {
            int bookId = FindBookIdByIsbn(isbn);

            if(bookId == -1)
            {
                return false;
            }
            else
            {
                return !id.Equals(FindBookIdByIsbn(isbn));
            }

        }

        public int FindBookIdByIsbn(string isbn)
        {
            var book = App.Model.Books.FirstOrDefault(b => b.Isbn.Equals(isbn));
            return book != null ? book.BookId : -1;
        }

        public List<RentalItem> GetActiveRentalItems()
        {
            var data = from ri in RentalItems
                       where ri.ReturnDate == null
                       select ri;
            return data.ToList();
        }

        public List<Rental> GetActiveRental()
        {
            var data = from ri in Rentals
                       where ri.RentalDate != null
                       select ri;
            return data.ToList();
        }

        public List<Rental> GetActiveRentalFromUser(User user)
        {
            var data = from ri in Rentals
                       where ri.RentalDate != null
                       where ri.User.UserName.Equals(user.UserName)
                       select ri;
            return data.ToList();
        }

        public void UpdateCategory(Category oldCategory, string newCategory)
        {
            var cat = Categories.FirstOrDefault(c => c.Name == oldCategory.Name);
            cat.Name = newCategory;
            SaveChanges();
            //var data = (from c in Categories
            //            where c.Name == oldCategory.Name
            //            select c).FirstOrDefault();
            //data.Name = newCategory;
            //SaveChanges();
        }

        public void RemoveCategory(Category category)
        {
            Categories.Remove(category);
            SaveChanges();
        }

        public void RemoveRental(Rental rental)
        {
            Rentals.Remove(rental);
            SaveChanges();
        }

        public void RemoveRentalEmpty()
        {
            Rental rentalEmpty = Rentals.FirstOrDefault(r => r.Items.Count() <= 0 && r.RentalDate != null);
            if(rentalEmpty != null)
            {
                Rentals.Remove(rentalEmpty);
                SaveChanges();
            }
        }
    }
}
