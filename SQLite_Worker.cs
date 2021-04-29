using SQLite;

namespace PI_TesterUniwersalnyKabli_V1.Database
{
    public class SQLite_Worker
    {
        readonly MainPage mainPage;

        public SQLiteConnection sQLiteConnection;

        public SQLite_Worker(MainPage mainPage)
        {
            this.mainPage = mainPage;

            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            sQLiteConnection = new SQLiteConnection("SQL_CablesDatabase.s3db");
            sQLiteConnection.CreateTable<SQL_CableTable>();
            sQLiteConnection.CreateTable<SQL_PrintersTable>();

           // ResetToFactory();
        }

        public TableQuery<SQL_CableTable> GetAllCables()
        {
            TableQuery<SQL_CableTable> query = sQLiteConnection.Table<SQL_CableTable>();

            return query;
        }

        public TableQuery<SQL_PrintersTable> GetAllPrinters()
        {
            //List<SQL_PrintersTable> L_Printers = new List<SQL_PrintersTable>();

            TableQuery<SQL_PrintersTable> query = sQLiteConnection.Table<SQL_PrintersTable>();

           // L_Printers = query.ToList();

            return query;
        }

        public SQL_CableTable GetCableByPrimaryKey(int key)
        {
            return sQLiteConnection.Get<SQL_CableTable>(key);
        }

        public SQL_PrintersTable GetPrinterByPrimaryKey(int key)
        {
           return sQLiteConnection.Get<SQL_PrintersTable>(key);
        }

        public void AddNewPrinterToDataBase(string printer, string opis)
        {
            var s = sQLiteConnection.Insert(new SQL_PrintersTable()
            {
                Printer = printer,
                Opis = opis
            });
        }

        public void AddNewCableToDataBase(string printer, string name, string symbol)
        {
            var s = sQLiteConnection.Insert(new SQL_CableTable()
            {
                Printer = printer,
                Name = name,
                Symbol = symbol,
                //Pins_A = 6,
                //PinsConnections = "1;2;3;4;5;6"
            });
        }

        public void UpdateCableInDataBase(int primary, string name, string symbol, string printer)
        {
            SQL_CableTable cableToUpdate = GetCableByPrimaryKey(primary);

            cableToUpdate.Name = name;
            cableToUpdate.Symbol = symbol;
            cableToUpdate.Printer = printer;

            sQLiteConnection.Update(cableToUpdate);
        }

        public void UpdateCableConnectionsInDataBase(int primary, string printer, int pinsA, int pinsB, string conn)
        {
            SQL_CableTable cableToUpdate = GetCableByPrimaryKey(primary);

            cableToUpdate.Printer = printer;

            cableToUpdate.Pins_A = pinsA;
            cableToUpdate.Pins_B = pinsB;
            cableToUpdate.PinsConnections = conn;

            sQLiteConnection.Update(cableToUpdate);
        }

        public void UpdatePrinterInDataBase(int primary, string printer, string opis)
        {
            SQL_PrintersTable printerToUpdate = GetPrinterByPrimaryKey(primary);

            printerToUpdate.Printer = printer;
            printerToUpdate.Opis = opis;

            sQLiteConnection.Update(printerToUpdate);
        }

        public int DeleteCableFromDataBase(int printerPrimaryKey)
        {
            return sQLiteConnection.Delete<SQL_CableTable>(printerPrimaryKey);
        }

        public int DeletePrinterFromDataBase(int printerPrimaryKey)
        {
           return sQLiteConnection.Delete<SQL_PrintersTable>(printerPrimaryKey);
        }

        public void ResetToFactory()
        {
            sQLiteConnection.DeleteAll<SQL_CableTable>();

            CreateTableResult result = sQLiteConnection.CreateTable<SQL_CableTable>();

            var s = sQLiteConnection.Insert(new SQL_CableTable()
            {
                Name = "cable 1m xyz",
                Symbol = "P000003-000",
               // Pins_A = 6,
                //PinsConnections = "1;2;3;4;5;6",
                Printer = "xxxx"
            });

            s = sQLiteConnection.Insert(new SQL_CableTable()
            {
                Name = "Kabel zasilający xyz",
                Symbol = "P000002-000",
                Pins_A = 5,
                PinsConnections = "B4;B3;B1;B2;B5",
                Printer = "yyyy"
            });

            s = sQLiteConnection.Insert(new SQL_CableTable()
            {
                Name = "Kabel xyz",
                Symbol = "P000001-000",
                Pins_A = 4,
                PinsConnections = ";;;",
                Printer = "yyyy"
            });

            s = sQLiteConnection.Insert(new SQL_CableTable()
            {
                Name = "Kabel TESTOWY",
                Symbol = "P0000000-000",
                Pins_A = 6,
                PinsConnections = "B1,B3;B2;B3,B1;B4;B5;B6",
                Printer = "xxxx"
            });
        }
    }
}
