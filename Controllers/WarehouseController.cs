using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;

using DecoAPI.Models;
using DecoAPI.Employees;

namespace DecoAPI.Controllers
{
    public class WarehouseController : ApiController
    {
        /// <summary>
        /// Method to Validate a user by Badge Scan
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        [Route("warehouse/ValidateUser/{badge}")]
        [HttpGet]
        public bool ValidateUser(string badge)
        {
            Employee e = new Employee();

            if(e.ValidateEmployee(badge))
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Method to process racks scanned into the Mold Warehouse
        /// </summary>
        /// <param name="scan">WIP Label number scanned from Rack</param>
        [Route("warehouse/MoldWarehouseIn/{scan}")]
        [HttpGet]
        public ReturnObject MoldWarehouseIn(string scan)
        {
            objWIPRack thisRack = new objWIPRack();
            
            try
            {
                // Make sure it is a wip rack
                if (scan.ToUpper().Contains("|WIP|"))
                {
                    thisRack = Rack.GetWIPRack(Int64.Parse(RackFactory.StripRack(scan)));
                    sPart part = new Part().GetPartByPartNumber(thisRack.PartNumber);

                    if (thisRack.Type == WIPRackType.FINISHED)
                    {
                        if (thisRack.Quantity == 0)
                        {
                            throw new Exception("WIP Rack has been cancelled");
                        }

                        // can't allow Nissan mold in color parts
                        if (part.Description1.Substring(0, 2) == "07" && part.Description1.Substring(3, 1) == "X")
                        {
                            throw new Exception("Can't scan Nissan MIC parts");
                        }

                        // make sure it isn't in Mold Warehouse already
                        if (thisRack.Location != "MOLDWH")
                        {
                            // Do an MFG/Pro Transaction
                            Models.Transactions trn = new Models.Transactions();
                            if (thisRack.Location == "PAINT")
                            {
                                trn.CreateInventoryTransfer("WHIXF", thisRack.PartNumber, thisRack.Quantity, "100", "PAINT", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                            }
                            else if (thisRack.Location == "HOLDSORT")
                            {
                                trn.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "HOLDSORT", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                            }
                            else if (thisRack.Location == "SOUTHWH")
                            {
                                trn.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "SOUTHWH", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                            }
                            else
                            {
                                if (part.PartType == "MOLD" || part.PartType == "PUNC100" || part.PartType == "MOLDA")
                                {
                                    trn.CreateRBE("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "MOLDWH", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                                }
                                else if (part.PartType == "MOLDP")
                                {
                                    trn.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "PAINT1", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                                }
                                else
                                {
                                    throw new Exception("Not a molded part!");
                                }
                            }
                            // move its location to MOLDWH
                            Rack.ChangeWIPLastLoc(thisRack.id, "MOLDWH");
                        }
                        else
                        {
                            throw new Exception("Rack already in Mold Warehouse");
                        }
                    }
                    else
                    {
                        throw new Exception("Can only move FINISHED WIP to Mold Warehouse");
                    }
                }
                else
                {
                    throw new Exception("Invalid WIP Label Scan");
                }
                return new ReturnObject(true, "WIP Rack transferred to Mold Warehouse.");
            }
            catch (Exception e)
            {
                return new ReturnObject(false, e.Message);
            }
        }
        
        
        /// <summary>
        /// Method to Process racks scanned into the South Warehouse (ie, outside)
        /// </summary>
        /// <param name="scan"></param>
        [Route("warehouse/SouthWarehouseIn/{scan}")]
        [HttpGet]
        public ReturnObject SouthWarehouseIn(string scan)
        {            
            objWIPRack thisRack = new objWIPRack();

            try
            {
                if (scan.Contains("|WIP|"))
                {
                    thisRack = Rack.GetWIPRack(Int64.Parse(RackFactory.StripRack(scan)));
                    sPart part = new Part().GetPartByPartNumber(thisRack.PartNumber);

                    if (thisRack.Type == WIPRackType.FINISHED)
                    {
                        if (thisRack.Quantity == 0)
                            throw new Exception("WIP Rack has been cancelled");

                        //can't allow Nissan mold in color parts
                        if (part.Description1.Substring(0, 2) == "07" && part.Description1.Substring(3, 1) == "X")
                            throw new Exception("Can't scan Nissan MIC parts");

                        //make sure it isn't in South Warehouse already
                        if (thisRack.Location != "SOUTHWH")
                        {
                            //Do an MFG/Pro Transaction
                            Transactions trn = new Transactions();
                            if (thisRack.Location == "PAINT")
                            {
                                trn.CreateInventoryTransfer("WHIXF", thisRack.PartNumber, thisRack.Quantity, "100", "PAINT", "100", "SOUTHWH", (int)thisRack.id, thisRack.Quantity, "southWH.aspx");
                            }
                            else if (thisRack.Location == "MOLDWH")
                            {
                                trn.CreateInventoryTransfer("WHIXF", thisRack.PartNumber, thisRack.Quantity, "100", "MOLDWH", "100", "SOUTHWH", (int)thisRack.id, thisRack.Quantity, "southWH.aspx");
                            }
                            else if (thisRack.Location == "HOLDSORT")
                            {
                                trn.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "HOLDSORT", "100", "SOUTHWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                            }
                            else
                            {
                                if (part.PartType == "MOLD" || part.PartType == "PUNC100" || part.PartType == "SVPUN100" || part.PartType == "MOLDA")
                                {
                                    trn.CreateRBE("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "SOUTHWH", "100", "SOUTHWH", (int)thisRack.id, thisRack.Quantity, "southWH.aspx.aspx");
                                }
                                else if (part.PartType == "MOLDP")
                                {
                                    trn.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "PAINT1", "100", "SOUTHWH", (int)thisRack.id, thisRack.Quantity, "southWH.aspx.aspx");
                                }
                                else
                                {
                                    throw new Exception("Not a molded part!");
                                }
                            }
                            Rack.ChangeWIPLastLoc(thisRack.id, "SOUTHWH");
                        }
                        else
                        {
                            throw new Exception("Rack already in Mold South Mold Warehouse");
                        }
                    }
                    else
                    {
                        throw new Exception("Can only move FINISHED WIP to South Mold Warehouse");
                    }
                }
                else
                {
                    throw new Exception("Invalid WIP Label Scan");
                }
                                
                return new ReturnObject(true, "Rack Transferred to South Warehouse");
            }
            catch (Exception e)
            {
                return new ReturnObject(false, e.Message);
            }
        }

                
        /// <summary>
        /// Method to handle racks scanned out of the Mold Warehouse/Into the Paint Line/Load Deck
        /// </summary>
        /// <param name="scan"></param>
        [Route("warehouse/PaintLineIn/{scan}")]
        [HttpGet]
        public ReturnObject PaintLineIn(string scan)
        {
            objWIPRack thisRack = new objWIPRack();
            sRack thisNewRack = new sRack();
            string wip;
            Transactions transaction = new Transactions();
            try
            {
                // Check and handle |RCK| created by Joe's Container Creator app in MoldWH
                if (scan.ToUpper().Contains("|RCK|"))
                {
                    wip = RackFactory.StripRack(scan);

                    // VALIDATION
                    thisNewRack = Rack.GetRack(Int32.Parse(wip));
                    if (thisNewRack.LastLocation != "MOLDWH")
                    {
                        throw new Exception("Only racks of this type in the MoldWH can be scanned to the load deck!");
                    }
                    // Base part #5623001 not allowed to go to paint line without PDC adapters (#5626008)
                    if (thisNewRack.PartNumber == "5623001")
                    {
                        throw new Exception("This part (#5623001) cannot be scanned to the paint line.  Must be punched first.");
                    }

                    if (thisNewRack.PartNumber.StartsWith("61"))
                    {
                        transaction.CreateRBE("WHORBE", thisNewRack.PartNumber, -thisNewRack.Quantity, "100", "MOLDWH", "100", "MOLDWH", Int32.Parse(thisNewRack.id.ToString()), thisNewRack.Quantity, this.ToString());
                    }
                    else
                    {
                        transaction.CreateInventoryTransfer("WHOXX", thisNewRack.PartNumber, thisNewRack.Quantity, "100", "MOLDWH", "100", "PAINT", (int)thisNewRack.id, thisNewRack.Quantity, "MWH/warehouseout.aspx");
                    }
                    Rack.ChangeRackLastLoc(thisNewRack.id, "PAINT");
                }

                else if (scan.ToUpper().Contains("|WIP|"))
                {
                    wip = scan.ToUpper().Replace("|WIP|", "");

                    // VALIDATION
                    thisRack = Rack.GetWIPRack(Int64.Parse(wip));
                    if (thisRack.Type != WIPRackType.FINISHED)
                        throw new Exception("Label must be FINISHED to scan out");
                    if (thisRack.MoldTime >= DateTime.Now.AddHours(-72.00))
                    {
                        // If it is an IMM rack, let it through
                        if (thisRack.Applicaiton == "IMM/rack_label.aspx")
                        { }
                        else //RIM Parts must wait three days to settle before being punched/painted. 
                        {
                            throw new Exception("Mold should wait 72 hours before scan out");
                        }
                    }
                    // Base part #5623001 not allowed to go to paint line without PDC adapters (#5626008)
                    if (thisRack.PartNumber == "5623001")
                    {
                        throw new Exception("This part (#5623001) cannot be scanned to the paint line.  Must be punched first.");
                    }
                    if (thisRack.Location != "MOLDWH" && thisRack.Location != "SOUTHWH" && thisRack.Location != "PAINT")
                    {
                        sPart part = new Part().GetPartByPartNumber(thisRack.PartNumber);

                        // Do an MFG/Pro Transaction                        
                        if (thisRack.Location == "HOLDSORT")
                        {
                            transaction.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "HOLDSORT", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                        }
                        else
                        {
                            if (part.PartType == "MOLD" || part.PartType == "PUNC100" || part.PartType == "MOLDA")
                            {
                                transaction.CreateRBE("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "MOLDWH", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                            }
                            else if (part.PartType == "MOLDP")
                            {
                                transaction.CreateInventoryTransfer("WHIXX", thisRack.PartNumber, thisRack.Quantity, "100", "PAINT1", "100", "MOLDWH", (int)thisRack.id, thisRack.Quantity, this.ToString());
                            }
                            else
                            {
                                throw new Exception("Not a molded part!");
                            }
                        }
                    }

                    if (thisRack.PartNumber.StartsWith("61"))
                    {
                        transaction.CreateRBE("WHORBE", thisRack.PartNumber, -thisRack.Quantity, "100", "MOLDWH", "100", "MOLDWH", Int32.Parse(thisRack.id.ToString()), thisRack.Quantity, this.ToString());
                        Rack.ChangeWIPLastLoc(thisRack.id, "MOLDWIP");
                    }
                    else if (thisRack.Location == "SOUTHWH" || thisRack.Location == "HOLDSORT")
                    {
                        transaction.CreateInventoryTransfer("WHOXX", thisRack.PartNumber, thisRack.Quantity, "100", thisRack.Location, "100", "PAINT", (int)thisRack.id, thisRack.Quantity, "MWH/warehouseout.aspx");
                    }
                    else if (thisRack.Location == "PAINT")
                    {
                        throw new Exception("Rack already scanned to load deck!");
                    }
                    else
                    {
                        transaction.CreateInventoryTransfer("WHOXX", thisRack.PartNumber, thisRack.Quantity, "100", "MOLDWH", "100", "PAINT", (int)thisRack.id, thisRack.Quantity, "MWH/warehouseout.aspx");
                    }
                    Rack.ChangeWIPLastLoc(thisRack.id, "PAINT");
                }
                else
                {
                    throw new Exception("Invalid Rack Label Scan");
                }

                return new ReturnObject(true, "Rack successfully transferred from Mold Warehouse.");
            }
            catch (Exception e)
            {
                return new ReturnObject(false, e.Message);
            }
        }

                
        /// <summary>
        /// Method to process racks scanned into the Shipping Warehouse
        /// </summary>
        /// <param name="scan"></param>
        [Route("warehouse/ShipWarehouseIn/{scan}")]
        [HttpGet]
        public ReturnObject ShipWarehouseIn(string scan)
        {
            try
            {
                // Make sure it is a shipping rack
                if (RackFactory.IsValidRack(scan) == false)
                    throw new Exception("Invalid rack label.");

                // Check whether new rack type, and if not, mark as legacy system
                string rackType = String.Empty;
                if (scan.StartsWith("|MES|"))
                {
                    rackType = "MES";
                }
                else
                {
                    rackType = "RCK";
                }

                // Strip the scanned value
                scan = RackFactory.StripRack(scan);

                // Manipulate das Toyota String
                if (scan.StartsWith("772"))
                {
                    scan = "8" + scan.Substring(3);
                }

                // Create the Rack Object
                AbstractRack rack;
                if (rackType == "MES")
                {
                    rack = RackFactory.Make(Int32.Parse(scan), rackType);
                }
                else
                {
                    rack = RackFactory.Make(Int32.Parse(scan));
                }

                // Create the Part Object
                sPart part = new Part().GetPartByPartNumber(rack.PartNumber);

                // Make sure it is not already in the shipping warehouse
                string sLocation = rack.LastLocation;
                if (sLocation == "100~SHIP")
                    throw new Exception("Rack already scanned into Ship Warehouse");
                if (sLocation == "NOSHIP")
                    throw new Exception("Not a shippable rack");
                if (sLocation != "SHIPWIP" && sLocation != "DDCWIP")
                    throw new Exception("Rack in unexpected location");
                if (rack.Quantity == 0)
                    throw new Exception("Rack is empty (quantity 0)");

                // Second check to prevent internal DO NOT SHIP labels from being added to ship warehouse
                string sApplication = rack.Application;
                if (sApplication == "ASP.rack_rack_internal_single_aspx")
                    throw new Exception("Internal Label DO NOT SHIP!");
                if (sApplication == "ASP.rack_rack_internal_double_aspx")
                    throw new Exception("Internal Label DO NOT SHIP!");

                // Make sure unassembled L42N rockers not scanned to ship
                if (part.Description1.StartsWith("07NML") && part.PartType == "PAINT")
                    throw new Exception("Cannot scan unassembled L42N Rockers to ship warehouse.  Must go to Assembly first.");

                // Make sure the rack is not cancelled
                int status = rack.Status;
                if (status == -1)
                    throw new Exception("Rack has been cancelled");
                // Make sure the rack is not waiting to be validated
                if (status == -5)
                    throw new Exception("BMW Rack not validated");

                // Move it to the Ship Warehouse
                sLocation = "100~SHIP";
                if (rackType == "MES")
                {
                    Database database = new Database();
                    try
                    {
                        SqlCommand command = new SqlCommand();
                        command.CommandText = "update mes.dbo.m_container set mc_location = @location where mc_id = @scan";
                        command.Parameters.AddWithValue("@location", sLocation);
                        command.Parameters.AddWithValue("@scan", scan);
                        database.ExecuteNonQuery(command);
                        command.Dispose();

                        try
                        {
                            command = new SqlCommand();
                            command.CommandText = "Insert into Production.dbo.history values (@scan, GetDate(), @quantity, @status, @location, @ShipLocation, @application)";
                            command.Parameters.AddWithValue("@scan", scan);
                            command.Parameters.AddWithValue("@quantity", rack.Quantity);
                            command.Parameters.AddWithValue("@status", rack.Status);
                            command.Parameters.AddWithValue("@location", sLocation);
                            command.Parameters.AddWithValue("@ShipLocation", rack.ShipLocation);
                            command.Parameters.AddWithValue("@application", "Shipping rack in " /*+ this.Application*/);
                            database.ExecuteNonQuery(command);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else
                {
                    rack.LastLocation = sLocation;
                }

                // Perform an MFG/Pro Transaction on the rack
                // Skip Backflush for P32R
                if (part.Description1.StartsWith("07RO"))
                { }
                else
                {
                    Transactions trn = new Transactions();
                    trn.CreateRBE("SHIPWHIN-RBE", rack.PartNumber, rack.Quantity, "100", "SHIP", "100", "SHIP", rack.RackID, rack.Quantity, "SWH/warehousein.aspx");
                }

                return new ReturnObject(true, "Rack transferred to Shipping Dock.");
            }
            
            catch (Exception e)
            {
                return new ReturnObject(false, e.Message);
            }
        }

                
        /// <summary>
        /// Method to process racks scanned out of the Shipping Warehouse (Obsolete due to DO shipping App?)
        /// </summary>
        /// <param name="scan"></param>
        [Route("warehouse/ShipWarehouseOut/{scan}")]
        [HttpGet]
        public ReturnObject ShipWarehouseOut(string scan)
        {
            try
            {
                if (RackFactory.IsValidRack(scan) == false)
                    throw new Exception("Invalid rack label.");

                scan = RackFactory.StripRack(scan);

                // Manipulate das Toyota String
                if (scan.StartsWith("772"))
                {
                    scan = "8" + scan.Substring(3);
                }

                // Create the Rack Object
                AbstractRack rack = RackFactory.Make(Int32.Parse(scan));
                // Create the Part Object
                sPart oPart = new Part().GetPartByPartNumber(rack.PartNumber);

                // Make sure it is not already in the shipping warehouse
                string sLocation = rack.LastLocation;
                if (sLocation == "SHIPWIP")
                    throw new Exception("Rack not in Ship Dock");
                if (sLocation != "100~SHIP")
                    throw new Exception("Rack already scanned out");
                if (rack.Quantity == 0)
                    throw new Exception("Rack is empty (quantity 0)");

                // Make sure the rack is not cancelled
                int iStatus = rack.Status;
                if (iStatus == -1)
                    throw new Exception("Rack has been cancelled");
                if (iStatus == 1)
                    throw new Exception("Rack has been shipped.");
                // Make sure the rack is not waiting to be validated
                if (iStatus == -5)
                    throw new Exception("BMW Rack not validated");

                // Move it to the Ship Warehouse
                sLocation = "SHIPWIP";
                rack.LastLocation = sLocation;

                // Perform an MFG/Pro Transaction on the rack
                Transactions trn = new Transactions();
                trn.CreateRBE("SHIPWHOUT-RBE", rack.PartNumber, -rack.Quantity, "100", "SHIP", "100", "SHIP", rack.RackID, -rack.Quantity, "SWH/warehouseout.aspx");

                // Display success
                return new ReturnObject(true, "Rack transferred out of Shipping Dock.");
            }
            catch (Exception e)
            {
                return new ReturnObject(false, e.Message);
            }
        }

                
        /// <summary>
        /// Method to process racks scanned into the assembly warehouse
        /// </summary>
        /// <param name="scan"></param>
        [Route("warehouse/AssemblyWarehouseIn/{scan}")]
        [HttpGet]
        public ReturnObject AssembyWarehouseIn(string scan)
        {
            try
            {
                if (RackFactory.IsValidRack(scan) == false)
                    throw new Exception("Invalid rack label.");

                // Make sure it is NOT a GM Rack
                if (scan.StartsWith("1JUN129357955"))
                    throw new Exception("Cannot Scan GM Shipping Rack into Assembly");

                scan = RackFactory.StripRack(scan);
                AbstractRack rack = RackFactory.Make(Int32.Parse(scan));
                sPart part = new Part().GetPartByPartNumber(rack.PartNumber);

                if (part.Description1.Substring(0, 2) == "06"
                    || (part.Description1.Substring(0, 2) == "07" && part.Description1.Substring(2, 2) == "RO" && part.PartType == "PAINT")
                    || (part.Description1.Substring(0, 2) == "07" && part.Description1.Substring(2, 2) == "RO" && part.Description1.Substring(6, 4) == "D  I" && part.PartType == "MOLD")
                    || (part.Description1.Substring(3, 2) == "V6" && (part.Description1.Substring(6, 1) == "F" || part.Description1.Substring(6, 1) == "N"))
                    || (part.Description1.Substring(3, 2) == "VZ" && part.Description1.Substring(6, 1) == "F")
                    || (part.Description1.Substring(3, 2) == "VS" && part.Description1.Substring(6, 1) == "N")
                    || (part.Description1.Substring(0, 2) == "07" && part.Description1.Substring(6, 1) == "K")
                    || (part.Description1.Substring(0, 2) == "07" && part.Description1.Substring(6, 1) == "B")
                    || (part.Description1.Substring(0, 5) == "07NML" && part.PartType == "PAINT")
                    || (part.Description1.Substring(0, 4) == "09LF" && part.PartType == "PAINT"))
                {
                    // GOOD
                }
                else
                {
                    throw new Exception("Cannot scan that part into Assembly area");
                }

                //Check if part is already assembled
                if (part.Description1.PadRight(20).Substring(19, 1) == "A")
                {
                    throw new Exception("Part already assembled");
                }

                // Make sure it is not already in the shipping warehouse
                string sLocation = rack.LastLocation;
                if (sLocation == "100~SHIP")
                    throw new Exception("Rack scanned into Ship Warehouse");
                if (sLocation == "Assembly")
                    throw new Exception("Rack already in assembly");
                if (rack.Quantity == 0)
                    throw new Exception("Rack is empty (quantity 0)");

                // Make sure the rack is not cancelled
                int iStatus = rack.Status;
                if (iStatus == -1)
                    throw new Exception("Rack has been cancelled");
                // Make sure the rack is not waiting to be validated
                if (iStatus == -5)
                    throw new Exception("BMW Rack not validated");

                // Move it to the Assembly Location
                sLocation = "Assembly";
                rack.LastLocation = sLocation;

                // Perform an MFG/Pro Transaction on the rack
                Transactions trn = new Transactions();

                trn.CreateRBE("ASM_IN-RBE", rack.PartNumber, rack.Quantity, "100", "PREASM", "100", "PREASM", rack.RackID, rack.Quantity, this.ToString());

                return new ReturnObject(true, "Rack transferred to Assembly Location.");
            }
            catch (Exception e)
            {
                return new ReturnObject(false, e.Message);
            }
        }        
    }
}
