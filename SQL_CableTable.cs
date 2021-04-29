using SQLite;

namespace PI_TesterUniwersalnyKabli_V1.Database
{
    public class SQL_CableTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        // Opis Cabla
        public string Printer { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Pins_A { get; set; }     
        public int Pins_B { get; set; }
        public string PinsConnections { get; set; }    
        public string Side_A_ConnectorType { get; set; }
        public string Side_B_ConnectorType { get; set; }

        public int TestedTimes { get; set; }
        public int Passed { get; set; }
        public int Fails { get; set; }
    }
}
