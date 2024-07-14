namespace CarHaulingAnalytics.Data.Models;

public class PaymentTypeOverviewModel
{
    public DateTime Date { get; set; }
    public int CashCount { get; set; }
    public int CheckCount { get; set; }
    public int CompanyCheckCount { get; set; }
    public int ComcheckCount { get; set; }
    public int TCHCount { get; set; }
    public int ACHCount { get; set; }
    public int SuperPayCount { get; set; }
    public int ZelleCount { get; set; }
    public int UshipCount { get; set; }
}

public class PaymentTypeData
{
    public string Name { get; set; } = string.Empty;
    public PaymentTypePair[] Data { get; set; } = [];
}

public class PaymentTypePair
{
    public long X { get; set; }
    public int Y { get; set; }
}