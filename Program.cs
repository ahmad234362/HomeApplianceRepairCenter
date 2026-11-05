using System;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter
{
    internal static class Program
    {
        private static void Main()
        {
            RepairCenter center = new RepairCenter("Damascus Home Appliance Repair Center");
            center.ApplianceStatusChanged += ShowNotification;

            Console.WriteLine("=== 1. Registering appliances ===");
            center.AddAppliance(new Refrigerator("RF-100001", "Samsung", "Ahmad", Date("2026-10-01"), Date("2026-10-08"), 420, true));
            center.AddAppliance(new Refrigerator("RF-100002", "LG", "Salma", Date("2026-10-05"), Date("2026-10-12"), 600, false));
            center.AddAppliance(new WashingMachine("WM-200001", "Bosch", "Omar", Date("2026-10-03"), Date("2026-10-10"), 8m, true, true));
            center.AddAppliance(new WashingMachine("WM-200002", "Ariston", "Lina", Date("2026-10-12"), Date("2026-10-19"), 6m, false, false));
            center.AddAppliance(new AirConditioner("AC-300001", "Gree", "Hadi", Date("2026-10-10"), Date("2026-10-17"), 18000, true, true, true));
            center.AddAppliance(new AirConditioner("AC-300002", "Carrier", "Rima", Date("2026-10-15"), Date("2026-10-22"), 12000, false, false, false));

            // Labor hours and parts cost are entered after the diagnosis.
            SetWork(center, "RF-100001", 3m, 80m);
            SetWork(center, "RF-100002", 5m, 150m);
            SetWork(center, "WM-200001", 4m, 70m);
            SetWork(center, "WM-200002", 2m, 20m);
            SetWork(center, "AC-300001", 2m, 45m);
            SetWork(center, "AC-300002", 1m, 10m);
            center.DisplayAllDevices();

            Console.WriteLine();
            Console.WriteLine("=== 2. Maintenance workflow (events are raised) ===");
            center.FindBySerial("RF-100001").StartRepair();
            center.FindBySerial("RF-100002").StartRepair();
            center.FindBySerial("WM-200001").StartRepair();
            center.FindBySerial("WM-200002").StartRepair();
            center.FindBySerial("AC-300001").StartRepair();
            center.FindBySerial("RF-100001").CompleteRepair();
            center.FindBySerial("WM-200001").CompleteRepair();
            center.FindBySerial("WM-200002").CompleteRepair();

            Console.WriteLine();
            Console.WriteLine("=== 3. Checking delayed deliveries on 2026-10-20 ===");
            int delayed = center.CheckDelays(Date("2026-10-20"));
            Console.WriteLine("Delay events raised: " + delayed);
            Console.WriteLine("Checking again raises: " + center.CheckDelays(Date("2026-10-20")) + " (each delay is reported once)");

            Console.WriteLine();
            Console.WriteLine("=== 4. Polymorphic processing of all appliances ===");
            center.DisplayAllDevices();

            Console.WriteLine();
            Console.WriteLine("=== 5. Totals ===");
            Console.WriteLine("Total cost of all appliances : {0:F2}", center.CalculateTotalCost());
            Console.WriteLine("Cost of repaired appliances  : {0:F2}", center.CalculateTotalCost(RepairStatus.Repaired));
            Console.WriteLine("Cost of appliances in repair : {0:F2}", center.CalculateTotalCost(RepairStatus.UnderRepair));
            Console.WriteLine("Cost of waiting appliances   : {0:F2}", center.CalculateTotalCost(RepairStatus.Received));
            Console.WriteLine("Static counters -> under repair: {0}, repaired: {1}, revenue: {2:F2}",
                Appliance.UnderRepairCount, Appliance.RepairedCount, Appliance.TotalRevenue);

            Console.WriteLine();
            Console.WriteLine("=== 6. Validation and encapsulation checks ===");
            Try("Invalid serial number", delegate { new Refrigerator("XX123", "LG", "Test", Date("2026-10-01"), Date("2026-10-02"), 300, false); });
            Try("Due date before received date", delegate { new Refrigerator("RF-100009", "LG", "Test", Date("2026-10-05"), Date("2026-10-01"), 300, false); });
            Try("Duplicate serial number", delegate { center.AddAppliance(new Refrigerator("RF-100001", "LG", "Test", Date("2026-10-01"), Date("2026-10-02"), 300, false)); });
            Try("Changing parts cost after repair", delegate { center.FindBySerial("RF-100001").PartsCost = 10m; });
            Try("Negative labor hours", delegate { center.FindBySerial("RF-100002").LaborHours = -1m; });
            Try("Completing a repair twice", delegate { center.FindBySerial("RF-100001").CompleteRepair(); });
            Try("Completing a repair never started", delegate { center.FindBySerial("AC-300002").CompleteRepair(); });
            Console.WriteLine("Appliances registered after the failed attempts: " + center.Count);
        }

        private static void ShowNotification(object sender, ApplianceStatusChangedEventArgs e)
        {
            Console.WriteLine("  [{0}] {1}", e.Kind, e.Message);
        }

        private static DateTime Date(string text)
        {
            DateTime result;
            if (!InputValidator.TryParseDate(text, out result))
                throw new FormatException("Invalid date: " + text);
            return result;
        }

        private static void SetWork(RepairCenter center, string serial, decimal hours, decimal partsCost)
        {
            Appliance appliance = center.FindBySerial(serial);
            appliance.LaborHours = hours;
            appliance.PartsCost = partsCost;
        }

        private static void Try(string title, Action action)
        {
            try
            {
                action();
                Console.WriteLine("  {0}: no error (unexpected)", title);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                int newLine = message.IndexOf('\n');
                if (newLine >= 0) message = message.Substring(0, newLine).TrimEnd();
                Console.WriteLine("  {0} -> {1}: {2}", title, ex.GetType().Name, message);
            }
        }
    }
}
