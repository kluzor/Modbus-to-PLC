using System;
using System.Windows;
using System.Data.Odbc;
using EasyModbus;

namespace WpfAppChamber
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public OdbcConnection OdbcConnect;

        public void DataBaseConnect()
        {
            var connectionString = @"DSN=PLC_SQL;DatabaseName=PLC_DB;UID=PLC;PWD=plc_pwd;";
            OdbcConnect = new OdbcConnection(connectionString);
            try
            {
                OdbcConnect.Open();
                MessageBox.Show("Połączono z bazą danych", "Success");
            }
            catch (OdbcException ex)
            {
                MessageBox.Show("Błąd połączenia z bazą danych" + ex.Message, "Error");
            }
        }

        public void InsertRow()
        {
            OdbcCommand insert = new OdbcCommand("INSERT INTO DBO.KODYSMT(STATUS,DATA,GODZINA,KOD,JAKOSCPROCENTOWA,OGOLNAJAKOSC,DECODE,CELLCONTRAST," +
                                                        "CELLMODULATION,REFLECTANCEMARGIN,FIXEDPATTERNDAMAGE,FORMATINFORMATIONDAMAGE,VERSIONINFORMATIONDAMAGE" +
                                                        ",AXIALNONUNIFORMITY,GRIDNONUNIFORMITY,UNUSEDERRORCORRECTIION,PRINTGROWTHHORIZONTAL,PRINTGROWTHVERTICAL," +
                                                        "NAZWAPROJEKTU) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?);", OdbcConnect);

            insert.Parameters.Add("@field1", OdbcType.Text);
            insert.Parameters.Add("@field2", OdbcType.Text);
            insert.Parameters.Add("@field3", OdbcType.Text);
            insert.Parameters.Add("@field4", OdbcType.Text);
            insert.Parameters.Add("@field5", OdbcType.Int);
            /*
            insert.Parameters["@field1"].Value = _dane.Status;             
            insert.Parameters["@field2"].Value = _dane.Data;                    
            insert.Parameters["@field3"].Value = _dane.Godzina;                    
            insert.Parameters["@field4"].Value = _dane.Kod;
            insert.Parameters["@field5"].Value = _dane.JakoscProcentowa;
            */
            insert.ExecuteNonQuery();
        }

        public int DeltaToModbus(string s)
        {
            string device;
            int new_modbus;
            int range = 0;
            char[] _chars = new char[5];

            //Pobranie i podzielenie adresów PLC na część Device i Range.
            var chars = s.ToCharArray();
            device = chars[0].ToString();

            for (int ctr = 1; ctr < chars.Length; ctr++)
            {
                _chars[ctr - 1] = chars[ctr];
            }

            string _range = new string(_chars);
            range = Int32.Parse(_range);

            //Utworzenie tablic z adresami MODBUS dla różnych Device PLC
            //Wypełnienie adresami MODBUS dla zakresów Range PLC
            int[] tabD;
            int[] tabM;
            int[] tabC;

            tabD = new int[12000];
            tabM = new int[4096];
            tabC = new int[256];

            int D1 = 4097;
            int D2 = 36865;

            int M1 = 2049;
            int M2 = 45057;

            int C1 = 3585;

            // Dla reg D
            for (int i = 0; i < 4096; i++)
            {
                tabD[i] = D1 + i;
            }
            int d = 0;
            for (int i = 4096; i < 12000; i++, d++)
            {
                tabD[i] = D2 + d;
            }

            // Dla reg M
            for (int i = 0; i < 1536; i++)
            {
                tabM[i] = M1 + i;
            }
            int m = 0;
            for (int i = 1536; i < 4096; i++, m++)
            {
                tabM[i] = M2 + m;
            }

            // Dla reg C
            for (int i = 0; i < 256; i++)
            {
                tabC[i] = C1 + i;
            }

            //Wybór i przypisanie odpowiednich adresów z PLC adresom MODBUS dla róznych device
            switch (device)
            {
                case "D":
                    new_modbus = tabD[range];
                    break;
                case "M":
                    new_modbus = tabM[range];
                    break;
                case "C":
                    new_modbus = tabC[range];
                    break;
                default:
                    new_modbus = 0;
                    break;
            }

            MessageBox.Show("C# MODBUS value: "+(new_modbus-1)+"\n" + "MODBUS value: "+ new_modbus);
            return new_modbus;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataBaseConnect();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int nowyModbus = DeltaToModbus(startPLCAdres.Text);
            try
            {
                int wynik = OdczytajDanaRejestru(ipAdres.Text, Int32.Parse(Port.Text), nowyModbus, Int32.Parse(iloscPLCAdres.Text));
                textBOX.Text = "Odczytana wartośc rejestru: " + wynik;

                Console.WriteLine(wynik);
            }
            catch (Exception)
            {
                //MessageBox.Show("Błąd połączenia z PLC: " + ex.Message, "Error");
            }
        }

        public static int OdczytajDanaRejestru(string plcIP, int port, int adresRejestru, int iloscRejestrow)
        {
            ModbusClient modbusClient = new ModbusClient(plcIP, port); //Ip-Address and Port of Modbus-TCP-Server
            modbusClient.Connect(); //Connect to Server
            //modbusClient.WriteMultipleCoils(4, new bool[] { true, true, true, true, true, true, true, true, true, true }); //Write Coils starting with Address 5
            //bool[] readCoils = modbusClient.ReadCoils(9, 10); //Read 10 Coils from Server, starting with address 10
            int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters((adresRejestru -1), iloscRejestrow); //Read 10 Holding Registers from Server, starting with Address 1

            // Console Output
            //for (int i = 0; i < readCoils.Length; i++)
            //    Console.WriteLine("Value of Coil " + (9 + i + 1) + " " + readCoils[i].ToString());

            for (int i = 0; i < readHoldingRegisters.Length; i++)
            {
                Console.WriteLine("Value of HoldingRegister: " + readHoldingRegisters[i].ToString());
            }

            modbusClient.Disconnect(); //Disconnect from Server
            return 0;
        }
    }
}