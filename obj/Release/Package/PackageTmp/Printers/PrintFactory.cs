/****************************************************************************
 * PrintFactory
 * 
 * This is a Factory used to determine what kind of object
 *  needs to be created in order to make the correct label
 *  for the given application. It takes in a Print.Label type
 *  and then uses that to create the correct child of the
 *  Print.Printer abstract class, then passing that object
 *  back to the object that called it.
 * 
 * PrintFactory.Select(...) is a static method that returns
 *  the correct type of Printer.
 * - eType: an enume of type Print.Label
 * - sID: the name of the given printer being used
 * - sApplication: the name of the app calling this function
 * 
 * To expand the type of print objects, a class that extends
 *  Paint.Printer must be created, then a new Print.Label must
 *  be created that matches that type of print job. The 
 *  Factory then must be able to create an object of that
 *  type in its switch statement, and then that new class will
 *  become available for printing purposes.
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace DecoAPI.Printers
{
    public class PrintFactory
    {
        /// <summary>
        /// Call to print a WIP or Rack label
        /// </summary>
        /// <param name="rack">Rack/Rack object you wish to print</param>
        /// <param name="type"></param>
        /// <param name="printer"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        public static Printer Select(Models.AbstractRack rack, Label type, string printer, string application)
        {
            switch (type)
            {
                case Label.WIP: return new WIPPrinter(printer, application, rack);
                case Label.RAH: return new RahauRackPrinter(printer, application, rack);
                case Label.RCK:
                    // EZGO
                    if (rack.Type == Models.RackType.EZGoRack)
                        return new EZGoRackPrinter(printer, application, rack);
                    // Club Car
                    if (rack.Type == Models.RackType.CCRack)
                        return new CCRackPrinter(printer, application, rack);
                    // MSIG
                    if (rack.Type == Models.RackType.MercedesRack || rack.Type == Models.RackType.BMWRack)
                        return new MSIGRackPrinter(printer, application, rack);
                    // BMW E70/E71
                    if (rack.Type == Models.RackType.BMWE7Rack)
                        return new BMWE7RackPrinter(printer, application, rack);
                    // Toyota
                    if (rack.Type == Models.RackType.ToyotaRack)
                        return new MSIGRackPrinter(printer, application, rack);
                    // GMT900
                    if (rack.Type == Models.RackType.GMT900Rack)
                        return new MSIGRackPrinter(printer, application, rack);
                    // GM
                    if (rack.Type == Models.RackType.GMRack)
                        return new GMRackPrinter(printer, application, rack);
                    // Nissan
                    if (rack.Type == Models.RackType.NissanRack)
                        return new NissanRackPrinter(printer, application, rack);
                    // VW
                    if (rack.Type == Models.RackType.VWRack)
                        return new VWRackPrinter(printer, application, rack);
                    if (rack.Type == Models.RackType.GMRackDDC)
                        return new GMRackDDCPrinter(printer, application, rack);
                    if (rack.Type == Models.RackType.KIARack)
                        return new KIARackPrinter(printer, application, rack);
                    break;
                default: throw new Exception("Error: Uknown label type");
            }
            throw new Exception("Print command invalid");
        }        
    }

    public enum Label
    {
        WIP = 0,
        RCK = 1,
        RAH = 2
    };
}
