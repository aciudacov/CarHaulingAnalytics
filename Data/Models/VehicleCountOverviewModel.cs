namespace CarHaulingAnalytics.Data.Models;

public class VehicleCountOverviewModel
{
    public DateTime Date { get; set; }
    public int One { get; set; }
    public int Two { get; set; }
    public int Three { get; set; }
    public int Four { get; set; }
    public int Five { get; set; }
    public int Six { get; set; }
    public int Seven { get; set; }
    public int Eight { get; set; }
    public int Nine { get; set; }
}

public class VehicleCountData
{
    public string Name { get; set; } = string.Empty;
    public VehicleCountPair[] Data { get; set; } = [];
}

public class VehicleCountPair
{
    public long X { get; set; }
    public int Y { get; set; }
}