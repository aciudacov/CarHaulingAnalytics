using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models;

public class LoadboardOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long OrderId { get; set; }

    public States PickupState { get; set; }

    [MaxLength(30)]
    public string PickupCity { get; set; } = string.Empty;

    [MaxLength(15)]
    public string PickupLatitude { get; set; } = string.Empty;

    [MaxLength(15)]
    public string PickupLongitude { get; set; } = string.Empty;

    public States DeliveryState { get; set; }

    [MaxLength(30)]
    public string DeliveryCity { get; set; } = string.Empty;

    [MaxLength(15)]
    public string DeliveryLatitude { get; set; } = string.Empty;

    [MaxLength(15)]
    public string DeliveryLongitude { get; set; } = string.Empty;

    public bool HasInoperable { get; set; }

    public int VehicleCount { get; set; }

    public TrailerTypes TrailerType { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime DataCollectedAt { get; set; }

    public decimal Price { get; set; }

    public decimal PricePerMile { get; set; }

    public PaymentTypes PaymentType { get; set; }

    public PaymentTimes PaymentTime { get; set; }

    public PaymentTerms PaymentTerm { get; set; }

    public decimal Distance { get; set; }

    [MaxLength(50)]
    public string ShipperName { get; set; } = string.Empty;
}
