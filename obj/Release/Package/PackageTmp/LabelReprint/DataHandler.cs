using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace DecoAPI.LabelReprint
{
    public class DataHandler
    {
        /// <summary>
        /// Method to Return List of Customers
        /// </summary>
        /// <returns></returns>
        public List<string> GetCustomerList()
        {
            List<string> customers = new List<string>();
            Models.Database database = new Models.Database();

            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT [name] from [PaintLabelReprint].[dbo].[customers]";

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                customers.Add(reader.GetString(0));
            }

            database.CloseConnection();

            return customers;
        }

        /// <summary>
        /// Method to return two digit customer code by customer name
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public string GetCustomerNumber(string customer)
        {
            string customerNumber = "";

            switch (customer.ToLower())
            {
                case "mercedes":
                    customerNumber = "01";
                    break;
                case "bmw":
                    customerNumber = "02";
                    break;
                case "ez-go":
                    customerNumber = "03";
                    break;
                case "club car":
                    customerNumber = "04";
                    break;
                case "nissan":
                    customerNumber = "07";
                    break;
                case "gm":
                    customerNumber = "08";
                    break;
                case "honda/vw":
                    customerNumber = "06";
                    break;
                case "kia":
                    customerNumber = "09";
                    break;
            }

            return customerNumber;
        }


        /// <summary>
        /// Method to look up Program Code by Program Name
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public string GetProgramNumber(string program)
        {
            string result = "";

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "Select Program FROM PaintLabelReprint.dbo.Programs WHERE Description = @program";
            command.Parameters.AddWithValue("@program", program);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            if (reader.Read())
            {
                result = reader.GetString(0);
            }
            database.CloseConnection();

            return result;
        }

        /// <summary>
        /// Method to look up all Programs for a given Customer
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public List<string> GetProgramList(string customer)
        {
            List<string> programs = new List<string>();

            string customerNumber = GetCustomerNumber(customer);

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "SELECT distinct Description FROM Decostar.dbo.pt_mstr_sql as master, PaintLabelReprint.dbo.Programs as PLR " +
                            "where  left(master.pt_desc1,2) = @partDigits " +
                            "and    substring(master.pt_desc1, 3,3) Like PLR.Program";
            command.Parameters.AddWithValue("@partDigits", customerNumber);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                programs.Add(reader.GetString(0));
            }

            database.CloseConnection();

            return programs;

        }


        /// <summary>
        /// Method to look up all Styles for a given Customer
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public List<StyleInfo> GetStyleList(string customer)
        {
            List<StyleInfo> styles = new List<StyleInfo>();

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "SELECT distinct description, [plant number] FROM decos032.wwconfig.dbo.styles WHERE [plant number] " +
                                  "IN (select Plant_Number from PaintLabelReprint.dbo.style_group WHERE Customer = @customer)";
            command.Parameters.AddWithValue("@customer", customer);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                StyleInfo style = new StyleInfo();
                style.Description = reader["description"].ToString();
                style.PlantNumber = Convert.ToInt32(reader["plant number"].ToString());
                styles.Add(style);
            }
            database.CloseConnection();

            return styles;
        }


        /// <summary>
        /// Method to look up Mold Parts for a given Customer and Program
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="program"></param>
        /// <returns></returns>
        public List<PartInfo> GetPartList(string customer, string program)
        {
            List<PartInfo> parts = new List<PartInfo>();

            string customerNumber = GetCustomerNumber(customer);
            string programNumber = GetProgramNumber(program);

            string s = customerNumber + programNumber;

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "Select master_sql.pt_part, master_sql.pt_desc2, MoldRecords.dbo.MoldPartInfo.Code "
                                 + "FROM Decostar.dbo.pt_mstr_sql as master_sql "
                                 + "full outer JOIN "
                                 + "MoldRecords.dbo.MoldPartInfo "
                                 + "ON "
                                 + "master_sql.pt_part  =  MoldRecords.dbo.MoldPartInfo.Part_Number "
                                 + "WHERE master_sql.pt_desc1 like @s "
                                 + "AND		master_sql.pt_part_type in ('mold', 'moldp', 'molda', 'punc100') "
                                 + "AND		master_sql.pt_status = 'ac'";
            command.Parameters.AddWithValue("@s", s + "%");

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                PartInfo part = new PartInfo();
                part.PartNumber = reader["pt_part"].ToString();
                part.Description = reader["pt_desc2"].ToString();
                part.Side = reader["code"].ToString();
                parts.Add(part);
            }

            database.CloseConnection();

            return parts;
        }


        /// <summary>
        /// Method to return all colors associated with a given program
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public List<ColorInfo> GetColorList(string program)
        {
            List<ColorInfo> colors = new List<ColorInfo>();
            string programNumber = GetProgramNumber(program);

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();

            command.CommandText = "select  distinct syscolors.ColorDescription, syscolors.Plant_Color " +
                                 "from  Decostar.dbo.pt_mstr_sql as pt_mstr, " +
                                 "Production.dbo.Colors as syscolors " +
                                 "where right(pt_desc1,4) = syscolors.Color_Code " +
                                 "and	substring(pt_desc1,3,3) = @programNumber " +
                                 "and	pt_status = 'AC' " +
                                 "and   syscolors.ColorDescription not like '%Not Used%'";
            command.Parameters.AddWithValue("@programNumber", programNumber);

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                ColorInfo color = new ColorInfo();
                color.Description = reader["ColorDescription"].ToString();
                color.PlantNumber = Convert.ToInt32(reader["Plant_Color"].ToString());
                colors.Add(color);
            }
            database.CloseConnection();

            return colors;
        }


        public List<GMPart> GetGMParts()
        {
            List<GMPart> GMParts = new List<GMPart>();

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "select	pt_part, pt_desc2 from	Decostar.dbo.pt_mstr_sql "
                                + "where (substring(pt_desc1, 1, 2) = '08' "
                                + "and pt_part_type = 'SVFG') "
                                + "or (substring(pt_desc1, 1, 2) = '08' "
                                + "and pt_part_type = 'FG') "
                                + "or (substring(pt_desc1, 1, 2) = '08' "
                                + "and substring(pt_desc1, 4, 2) = 'SU' "
                                + "and pt_part_type = 'CMP100') "
                                + "or (substring(pt_desc1, 1, 2) = '08' "
                                + "and substring(pt_desc1, 4, 2) = 'SU' "
                                + "and pt_part_type = 'CMP101') "
                                + "order by	pt_part asc";

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                GMPart part = new GMPart();
                part.Number = reader["pt_part"].ToString();
                part.Description = reader["pt_desc2"].ToString();

                GMParts.Add(part);
            }
            database.CloseConnection();

            return GMParts;
        }

        public List<Printers.Address> GetGMAddresses()
        {
            List<Printers.Address> GMAddresses = new List<Printers.Address>();

            Models.Database database = new Models.Database();
            SqlCommand command = new SqlCommand();
            command.CommandText = "select * from RackRecords.dbo.ShippingAddresses where Customer = 'GM'";

            SqlDataReader reader = database.RunCommandReturnReader(command);

            while (reader.Read())
            {
                Printers.Address address = new Printers.Address();

                address.ID = Int32.Parse(reader["Address_ID"].ToString());
                address.Description = reader["Description"].ToString();
                address.Line1 = reader["Line_1"].ToString();
                address.Line2 = reader["Line_2"].ToString();
                address.Line3 = reader["Line_3"].ToString();
                address.Line4 = reader["Line_4"].ToString();
                address.ShipLocation = reader["Ship_Location_TXT"].ToString();
                address.Customer = reader["Customer"].ToString();
                address.TempPo = reader["PO"].ToString();

                GMAddresses.Add(address);
            }
            database.CloseConnection();

            return GMAddresses;
        }

        public void PrintGMRack(LabelReprint.GMLabel label)
        {
            Models.AbstractRack rack = new Models.AbstractRack(label.PartNumber, label.Quantity, label.ShipLocation);

            rack.PurchaseOrder = label.PurchaseOrder;

            if (rack.PurchaseOrder == "")
            {
                rack.ShipLocation = "Landaal";
                rack.ServiceRack = true;
                rack.SetCustomerPart("00000122");
                Printers.LandaalRackPrinter landaalPrinter = new Printers.LandaalRackPrinter(label.Printer, "DecoAPI", rack);

                Printers.Address address = new Printers.Address();
                address.Line1 = label.Line1;
                address.Line2 = label.Line2;
                address.Line3 = label.Line3;
                address.Line4 = label.Line4;
                landaalPrinter.address = address;

                landaalPrinter.print();

            }
            else
            {
                Printers.GMRackPrinter gmPrinter = new Printers.GMRackPrinter(label.Printer, "DecoAPI", rack);

                Printers.Address address = new Printers.Address();
                address.Line1 = label.Line1;
                address.Line2 = label.Line2;
                address.Line3 = label.Line3;
                address.Line4 = label.Line4;
                gmPrinter.address = address;

                gmPrinter.print();
            }
        }
    }    
}