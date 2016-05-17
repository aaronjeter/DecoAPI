using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Text;

namespace DecoAPI.Printers
{
    public class PrintHandler
    {
        int port = 9100;
        string ipAddress;
        public string dpi;
        string lastError;        

        public PrintHandler(string PrinterId)
        {            
            ipAddress = GetIPAddress(PrinterId);
            dpi = GetDPI(PrinterId);
        }

        public PrintHandler(LabelReprint.Label label)
        {
            ipAddress = GetIPAddress(label.PrinterID);
            FormLabel(label);            
        }
        
        public string GetIPAddress(string printer_id)
        {
            string ip = "";
            
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT IP_ADDRESS from Print_Service.dbo.PrinterPaths where PRINTER_ID = @printer_id";
            command.Parameters.AddWithValue("@printer_id", printer_id);

            Models.Database database = new Models.Database();

            SqlDataReader reader = database.RunCommandReturnReader(command);
            reader.Read();
            
            ip = reader.GetString(0);            

            database.CloseConnection(); 
            return ip;
        }

        public string GetDPI(string printer_id)
        {
            string dpi = "";

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT DPI from Print_Service.dbo.PrinterPaths where PRINTER_ID = @printer_id";
            command.Parameters.AddWithValue("@printer_id", printer_id);

            Models.Database database = new Models.Database();

            SqlDataReader reader = database.RunCommandReturnReader(command);
            reader.Read();

            dpi = reader["DPI"].ToString();

            database.CloseConnection();
            return dpi;
        }

        public bool GetRibbon(string printer_id)
        {
            bool ribbon = false;

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT Ribbon FROM Print_Service.dbo.PrinterPaths where PRINTER_ID = @printer_id";
            command.Parameters.AddWithValue("@printer_id", printer_id);
            Models.Database database = new Models.Database();

            SqlDataReader reader = database.RunCommandReturnReader(command);
            reader.Read();

            ribbon = (bool) reader["Ribbon"];

            database.CloseConnection();
            return ribbon;
        }

        public bool SendToPrinter(string ipl, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                if (SendToPrinter(ipl) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Sends IPL to IP address specified
        /// </summary>
        /// <param name="ipl"></param>
        /// <returns></returns>
        public bool SendToPrinter(string ipl)
        {
            byte[] sendBytes;
            TcpClient client = null;
            NetworkStream stream = null;            

            try
            {
                client = new TcpClient();
                client.Connect(ipAddress, port);
                stream = client.GetStream();
                sendBytes = ASCIIEncoding.ASCII.GetBytes(ipl);

                if (stream.CanWrite) //try 
                {
                    stream.Write(sendBytes, 0, sendBytes.Length);
                    return true;
                }
                else
                {
                    if (stream.CanWrite) //and try again
                    {
                        stream.Write(sendBytes, 0, sendBytes.Length);
                        return true;
                    }
                    else
                    {
                        lastError = "Can't Write to Stream";
                        return false;
                    }

                }
            }
            catch (Exception e)
            {
                lastError = e.Message.ToString();
                return false;
            }
            finally
            {
                if (stream != null) { stream.Close(); }
                if (client != null) { client.Close(); }
            }
        }


        public void FormLabel(LabelReprint.Label label)
        {
            Employees.Employee e = new Employees.Employee();
            label.UserName = e.GetEmployeeName(label.Badge);
            label.Initials = e.GetEmployeeInitials(label.Badge);
            label.Ribbon = GetRibbon(label.PrinterID);

            for (int i = 0; i < label.Quantity; i++)
            {
                if (label.LabelType == "Paint")
                {                    
                    PrintPaintLabel(label);
                }
                else if (label.LabelType == "Mold")
                {
                    PrintMoldLabel(label);
                    label.shotCount = label.shotCount + 1; //Increment shot counter
                }
            }
            InsertLog(label);
        }

        public bool PrintPaintLabel(LabelReprint.Label label)
        {
            Models.Database database = new Models.Database();

            string plantColor = label.Color.PlantNumber.ToString();
            string styleNum = label.Style.PlantNumber.ToString();

            #region Paint label Barcode stored procedure
            string _BC_UID = string.Empty;

            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "Production.dbo.sp_Create_Paint_Exit_Label_RM";

            command.Parameters.AddWithValue("@carrier", 999);
            command.Parameters.AddWithValue("@carrier_pos", 1);
            command.Parameters.AddWithValue("@round", 1);
            command.Parameters.AddWithValue("@color", plantColor);
            command.Parameters.AddWithValue("@style", styleNum);
            command.Parameters.AddWithValue("@status", 0);
            command.Parameters.AddWithValue("@articleNo", "");
            command.Parameters.AddWithValue("@reqPassLabel", 0);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            reader.Read();
            _BC_UID = reader["pnt_Barcode"].ToString();
            database.CloseConnection();
            //_BC_UID = database.RunSingleResultQuery(command);
            #endregion

            string date = DateTime.Now.ToShortDateString();
            string time = DateTime.Now.ToShortTimeString();
            string styleDescription = string.Empty;
            string colorDescription = string.Empty;
            string ipl = string.Empty;

            #region Color / Style descriptions

            string styleQuery = " select description " +
                             " from   decosql.WWConfig.dbo.Styles " +
                             " where  [Plant Number] = '" + styleNum + "'";

            styleDescription = database.RunSingleResultQuery(styleQuery, Models.Database.DBType.DecoSQL);

            string colorQuery = " select description " +
                             " from   decosql.WWConfig.dbo.[System Colors] " +
                             " where  [Plant Number] = '" + plantColor + "'";

            colorDescription = database.RunSingleResultQuery(colorQuery, Models.Database.DBType.DecoSQL);

            #endregion

            
            dpi = GetDPI(label.PrinterID);
            ipAddress = GetIPAddress(label.PrinterID);

            #region IPL
            if (dpi == "200")
            {
                ipl += "<STX>R<ETX>";
                ipl += "<STX><ESC>C<SI>h<SI>I1<ETX>";
                ipl += "<STX><ESC>P;F*<ETX>";
                if (label.Ribbon)
                    ipl += "<STX><SI>g1,567<ETX>";
                else
                    ipl += "<STX><SI>g0,420<ETX>";
                ipl += "<STX>H0;o140,175;f3;c30;h1;w1;d3,;<ETX>";
                ipl += "<STX>H1;o125,20;f3;c30;h1;w1;d3," + date + ";<ETX>";
                ipl += "<STX>H2;o125,140;f3;c30;h1;w1;d3," + time + ";<ETX>";
                ipl += "<STX>H3;o110,20;f3;c30;h1;w1;d3," + styleDescription + ";<ETX>";
                ipl += "<STX>B4;o85,30;f3;c6,0;h50;w2;r0;i0;d3," + _BC_UID + ";<ETX>";
                ipl += "<STX>H5;o30,20;f3;c30;h1;w1;d3," + colorDescription + ";<ETX>";
                ipl += "<STX>H6;o30,265;f3;c31;h1;w1;d3,C:" + "999" + ";<ETX>";
                ipl += "<STX>H7;o125,265;f3;c30;h1;w1;d3," + label.UserName + ";<ETX>";
                ipl += "<STX>D0<ETX>";
                ipl += "<STX>R<ETX>";
                ipl += "<STX><ESC>E*,1<CAN><ETX>";
                ipl += "<STX><RS>1<US>1<ETB><FF><ETX>";
            }
            else if (dpi == "400")
            {
                ipl += "<STX>R<ETX>";
                ipl += "<STX><ESC>C<SI>h<SI>I1<ETX>";
                ipl += "<STX><ESC>P;F*<ETX>";
                if (label.Ribbon)
                    ipl += "<STX><SI>g1,366<ETX>";
                else
                    ipl += "<STX><SI>g0,420<ETX>";
                ipl += "<STX>H0;o280,350;f3;c30;h1;w1;d3,;<ETX>";
                ipl += "<STX>H1;o250,40;f3;c30;h1;w1;d3," + date + ";<ETX>";
                ipl += "<STX>H2;o250,280;f3;c30;h1;w1;d3," + time + ";<ETX>";
                ipl += "<STX>H3;o220,40;f3;c30;h1;w1;d3," + styleDescription + ";<ETX>";
                ipl += "<STX>B4;o170,60;f3;c6,0;h117;w4;r0;i0;d3," + _BC_UID + ";<ETX>";
                ipl += "<STX>H5;o60,40;f3;c30;h1;w1;d3," + colorDescription + ";<ETX>";
                ipl += "<STX>H6;o60,530;f3;c31;h1;w1;d3,C:" + "999" + ";<ETX>";
                ipl += "<STX>H7;o250,450;f3;c30;h1;w1;d3," + label.UserName + ";<ETX>";
                ipl += "<STX>D0<ETX>";
                ipl += "<STX>R<ETX>";
                ipl += "<STX><ESC>E*,1<CAN><ETX>";
                ipl += "<STX><RS>1<US>1<ETB><FF><ETX>";
            }
            #endregion

            if (SendToPrinter(ipl.ToString()) == false)
            {
                if (SendToPrinter(ipl.ToString()) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public bool PrintMoldLabel(LabelReprint.Label label)
        {
            string serial = GetMoldSerial(label.Part.PartNumber);
            AddMoldPart(label.Part.PartNumber, serial);

            string uniqueID = label.Part.PartNumber + serial;
            dpi = GetDPI(label.PrinterID);
            ipAddress = GetIPAddress(label.PrinterID);

            string ipl = "";

            // Build the Printer String
            #region IPL
            #region Head
            ipl += "<STX>R<ETX>";
            ipl += "<STX><ESC>k<ETX>";
            ipl += "<STX><ESC>C0<ETX>";
            ipl += "<STX><ESC>P;<ETX>";
            ipl += "<STX>E*;F*;<ETX>";
            #endregion
            #region Body
            if (dpi == "400")
            {
                ipl += "<STX>H0;o1,1;r0;f0;c0;d3, ;b0;h1;w1;<ETX>";
                ipl += "<STX>H1;o155,125;f3;c30;h1;w1;d3," + label.Part.Description + "<ETX>";
                ipl += "<STX>B2;o110,125;f3;c6,3;h80;w4;r2;i0;d3," + uniqueID + "<ETX>";
                ipl += "<STX>H3;o35,125;f3;c30;h1;w1;d3," + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                ipl += "   " + label.Initials.ToUpper() + "   " + label.shotCount;
                ipl += "<ETX>";
                ipl += "<STX>H5;o90,20;f3;c34;h1;w1;d3," + label.Part.Side + "<ETX>";
            }
            else if (dpi == "200")
            {
                ipl += "<STX>H0;o1,1;r0;f0;c0;d3, ;b0;h1;w1;<ETX>";
                ipl += "<STX>H1;o77,62;f3;c30;h1;w1;d3," + label.Part.Description + "<ETX>";
                ipl += "<STX>B2;o55,62;f3;c6,3;h40;w2;r2;i0;d3," + uniqueID + "<ETX>";
                ipl += "<STX>H3;o17,62;f3;c30;h1;w1;d3," + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                ipl += "   " + label.Initials.ToUpper() + "   " + label.shotCount;
                ipl += "<ETX>";
                ipl += "<STX>H5;o45,10;f3;c34;h1;w1;d3," + label.Part.Side + "<ETX>";
            }
            else
            {
                throw new Exception("DPI NOT SET");
            }
            #endregion
            #region Tail
            ipl += "<STX>R<ETX><STX>R<ETX><STX><ESC>k<ETX><STX><ESC>C0";
            if (label.Ribbon)
                ipl += "<SI>g1,366";
            else
                ipl += "<SI>g0,420";
            ipl += "<SI>T1<SI>R0<SI>r0<SI>D0<SI>c0<SI>t0<SI>l0<SI>F2<SI>f0<SI>L1000<SI>I5<SI>i0<SI>W609;<SI>S40<SI>d0<ETX>";
            ipl += "<STX><ESC>E*<CAN><ETX>";
            ipl += "<STX><RS>1<ETX><STX><US>1<ETX><STX><ETB><ETX>";
            #endregion

            #endregion
            #region ZPL
            //oString.Append("^XA");
            //oString.Append("^LH01,01");
            //oString.Append("^FO10,25^AD,10^FD" + part.Description + "^FS");
            //oString.Append("^FO10,45^BCN,40,N,N,N^FD" + sUniqueID + "^FS");
            //oString.Append("^FO75,90^AD^FD,10^FD" + DateTime.Now.ToShortDateString() + "^FS");
            //oString.Append("^FO230,90^AD^FD,10^FD" + DateTime.Now.ToShortTimeString() + "^FS");
            //oString.Append("^FO0,90^AD^FD,10^FD" + part.Side + "^FS");
            //oString.Append("^XZ");

            #endregion

            if (SendToPrinter(ipl.ToString()) == false)
            {
                if (SendToPrinter(ipl.ToString()) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public string GetMoldSerial(string partNumber)
        {
            string serial = "";
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "Production.dbo.sp_getOdometerSerial6";
            command.Parameters.AddWithValue("@id", 5);
            command.Parameters.AddWithValue("@nextOne", serial);
            serial = database.RunSingleResultQuery(command, Models.Database.DBType.DecoSQL02);

            return serial;
        }

        public void AddMoldPart(string partNumber, string serial)
        {
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "Production.dbo.sp_CreatePart";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@serial_number", partNumber + serial);
            command.Parameters.AddWithValue("@part_number", partNumber);
            command.Parameters.AddWithValue("@machine_id", "999"); 
            command.Parameters.AddWithValue("@transaction_code", 100); 
            command.Parameters.AddWithValue("@operation_number", "10"); 
            command.Parameters.AddWithValue("@cCounter", "1");

            database.RunCommand(command);
        }

        public void InsertLog(LabelReprint.Label label)
        {
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();

            command.CommandText = "insert into PaintLabelReprint.dbo.reprint_log (labeltype, customer, program, style, color, part, username, printer_id, timestamp, quantity, reason) values(@labeltype, @customer, @program, @style, @color, @part, @username, @printer_id, @timestamp, @quantity, @reason)";
            command.Parameters.AddWithValue("@labeltype", label.LabelType);
            command.Parameters.AddWithValue("@customer", label.Customer);
            command.Parameters.AddWithValue("@program", label.Program);
            if (label.LabelType == "Paint")
            {
                command.Parameters.AddWithValue("@style", label.Style.Description);
                command.Parameters.AddWithValue("@color", label.Color.Description);
                command.Parameters.AddWithValue("@part", "-");
            }
            else if (label.LabelType == "Mold")
            {
                command.Parameters.AddWithValue("@style", "-");
                command.Parameters.AddWithValue("@color", "-");
                command.Parameters.AddWithValue("@part", label.Part.Description);
            }
            command.Parameters.AddWithValue("@username", label.UserName);
            command.Parameters.AddWithValue("@printer_id", label.PrinterID);
            command.Parameters.AddWithValue("@timestamp", DateTime.Now);
            command.Parameters.AddWithValue("@quantity", label.Quantity);
            command.Parameters.AddWithValue("@reason", label.Reason);

            database.RunCommand(command);
        }
    }
}