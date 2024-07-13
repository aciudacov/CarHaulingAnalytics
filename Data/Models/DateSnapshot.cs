namespace CarHaulingAnalytics.Data.Models;

public class DateSnapshot
{
    public DateTime Date { get; set; }
    public decimal AverageRange { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal AveragePricePerMile { get; set; }
    public int OrdersCount { get; set; }
}

public class SnapshotDataset
{
    public string Name { get; set; } = string.Empty;
    public decimal[] Data { get; set; } = [];
    public string Type { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int ValueDecimals { get; set; }
}

public class SnapshotModel
{
    public long[] Xdata { get; set; } = [];
    public SnapshotDataset[] Datasets { get; set; } = [];
}