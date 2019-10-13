using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using prbd_1819_g01.Properties;

namespace prbd_1819_g01
{
    public enum AppMessages
    {
        MSG_NEW_BOOK,
        MSG_DISPLAY_BOOK,
        MSG_TITLE_CHANGED,
        MSG_CLOSE_TAB,
        MSG_REFRESH_BOOKS_VIEW,
        MSG_REFRESH_CATEGORIES_VIEW,
        MSG_REFRESH_BASKET_VIEW,
        MSG_REFRESH_RENTALS_VIEW,
        MSG_REFRESH_BOOKS_VIEW_USER,
        MSG_REFRESH_USERS_VIEW,
        MSG_REFRESH_DETAILS_VIEW,
    }

    public partial class App : ApplicationBase
    {

        /*-- Constructor --*/

        public App()
        {
#if MSSQL
            var type = DbType.MsSQL;
#else
        var type = DbType.MySQL;
#endif
            Model = Model.CreateModel(type);
            Model.CreateTestData(type);

        Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Culture);
        }

        /*-- Model --*/

        public static Model Model { get; private set; }

        /*-- User connected --*/

        public static User CurrentUser { get; set; }

        /*-- Image path --*/

        public static readonly string IMAGE_PATH = 
            Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../images");
        public static readonly string ICON_PATH =
            Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../icons");
    }
}
