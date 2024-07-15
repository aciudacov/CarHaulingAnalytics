namespace CarHaulingAnalytics.Data.Models.Charts.ComplexData;

public class LongDecimalModel
{
    public long X { get; set; }
    public decimal Y { get; set; }
}

public class LongDecimalOverviewModel
{
    public string Name { get; set; } = string.Empty;
    public LongDecimalModel[] Data { get; set; } = [];
}
