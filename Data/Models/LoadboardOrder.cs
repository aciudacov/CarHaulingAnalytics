using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CarHaulingAnalytics.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarHaulingAnalytics.Data.Models;

[Index(nameof(OrderId), nameof(OrderGuid))]
public class LoadboardOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? OrderId { get; set; }

    [MaxLength(36)]
    public string? OrderGuid { get; set; }

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

    [MaxLength(75)]
    public string ShipperName { get; set; } = string.Empty;

    public SourcePlatform SourcePlatform { get; set; }
}
