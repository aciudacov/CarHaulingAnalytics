﻿using CarHaulingAnalytics.Data.Context;
using CarHaulingAnalytics.Data.Enums;
using CarHaulingAnalytics.Data.Models;
using CarHaulingAnalytics.Data.Models.Charts.ComplexData;
using CarHaulingAnalytics.Data.Models.Charts.Tuples;
using CarHaulingAnalytics.Data.Models.FilterModels;
using CarHaulingAnalytics.Data.Models.Widgets.Tuples;
using EnumsNET;
using Microsoft.EntityFrameworkCore;

namespace CarHaulingAnalytics.Data;

public class WidgetDataService(AnalyticContext context)
{
    public async Task<int> GetOrderCount(BaseFilterModel model, CancellationToken cancellationToken)
    {
        return await GetBaseQuery(model)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<StringCountTuple>> GetCountByPickupState(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(o => o.PickupState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                OrdersCount = n.Count()
            })
            .OrderBy(o => o.State)
            .ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = r.State.AsString(),
            Count = r.OrdersCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetCountByDeliveryState(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(o => o.DeliveryState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                OrdersCount = n.Count()
            })
            .OrderBy(o => o.State)
            .ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = r.State.AsString(),
            Count = r.OrdersCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetPopularRoutes(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(o => new{ o.PickupState, o.DeliveryState })
            .Where(w => w.Key.PickupState != States.Canada && w.Key.DeliveryState != States.Canada)
            .Select(g => new
            {
                g.Key.PickupState,
                g.Key.DeliveryState,
                OrderCount = g.Count()
            }).OrderByDescending(o => o.OrderCount)
            .Take(20)
            .ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple()
        {
            Value = $"{r.PickupState.AsString()} to {r.DeliveryState.AsString()}",
            Count = r.OrderCount
        });
    }

    public async Task<IEnumerable<PickupStateAveragePrice>> GetAveragePriceByPickupState(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(o => o.PickupState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                AveragePrice = n.Average(a => a.Price)
            })
            .OrderBy(o => o.State)
            .ToListAsync(cancellationToken);

        return result.Select(r => new PickupStateAveragePrice
        {
            State = r.State.AsString(),
            AveragePrice = decimal.Round(r.AveragePrice, 2, MidpointRounding.AwayFromZero)
        });
    }

    public async Task<IEnumerable<PickupStateAveragePrice>> GetAveragePricePerMileByPickupState(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .Where(o => o.PricePerMile > 0 && o.PricePerMile < 300)
            .GroupBy(o => o.PickupState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                AveragePrice = n.Sum(s => s.VehicleCount == 1 ? s.Price : s.Price / s.VehicleCount) / n.Sum(s => s.VehicleCount == 1 ? s.Distance : s.Distance * s.VehicleCount)
                //AveragePrice = n.Average(a => a.VehicleCount == 1 ? a.PricePerMile : a.PricePerMile / a.VehicleCount)
            })
            .OrderBy(o => o.State)
            .ToListAsync(cancellationToken);

        return result.Select(r => new PickupStateAveragePrice
        {
            State = r.State.AsString(),
            AveragePrice = decimal.Round(r.AveragePrice, 2, MidpointRounding.AwayFromZero)
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetShipperOrderCount(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .Where(o => o.PricePerMile > 0 && o.PricePerMile < 300)
            .GroupBy(o => o.ShipperName)
            .Select(n => new
            {
                ShipperName = n.Key,
                OrderCount = n.Count()
            })
            .OrderByDescending(o => o.OrderCount)
            .Take(20)
            .ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = r.ShipperName,
            Count = r.OrderCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetPaymentTypesCount(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .Where(o => o.PricePerMile > 0 && o.PricePerMile < 300)
            .GroupBy(o => o.PaymentType)
            .Select(n => new
            {
                PaymentType = n.Key,
                OrderCount = n.Count()
            })
            .OrderBy(o => o.PaymentType)
            .ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = r.PaymentType.AsString(),
            Count = r.OrderCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetVehicleStatuses(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(g => g.HasInoperable)
            .Select(n => new
            {
                Inop = n.Key,
                Count = n.Count()
            }).ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = r.Inop ? "Inoperable" : "Operable",
            Count = r.Count
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetTrailerTypes(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(g => g.TrailerType)
            .Select(n => new
            {
                TrailerType = n.Key,
                Count = n.Count()
            }).ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = r.TrailerType.AsString(),
            Count = r.Count
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetVehicleCounts(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetOverviewQuery(model, query)
            .GroupBy(g => g.VehicleCount)
            .Select(n => new
            {
                VehicleCount = n.Key,
                Count = n.Count()
            }).ToListAsync(cancellationToken);

        return result.Select(r => new StringCountTuple
        {
            Value = $"{r.VehicleCount} vehicle(s)",
            Count = r.Count
        });
    }

    public async Task<List<DateAverageTuple>> GetAveragePriceTrend(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(n => new
            {
                Date = n.Key,
                Average = n.Average(s => s.Price)
            }).ToListAsync(cancellationToken);

        return result.Select(r => new DateAverageTuple
        {
            Date = r.Date,
            Average = r.Average,
        }).ToList();
    }

    public async Task<List<DateAverageTuple>> GetAveragePricePerMileTrend(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(n => new
            {
                Date = n.Key,
                Average = n.Average(s => s.PricePerMile)
            }).ToListAsync(cancellationToken);

        return result.Select(r => new DateAverageTuple
        {
            Date = r.Date,
            Average = r.Average,
        }).ToList();
    }

    public async Task<IEnumerable<DateAverageTuple>> GetAverageDistanceTrend(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(n => new
            {
                Date = n.Key,
                Average = n.Average(s => s.Distance)
            }).ToListAsync(cancellationToken);

        return result.Select(r => new DateAverageTuple
        {
            Date = r.Date,
            Average = decimal.Round(r.Average, 2, MidpointRounding.AwayFromZero)
        });
    }

    public async Task<SnapshotModel> GetMainTrendData(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var result = await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(n => new DateSnapshot
            {
                Date = n.Key,
                AverageRange = n.Average(s => s.Distance),
                AveragePricePerMile = n.Average(s => s.PricePerMile),
                AveragePrice = n.Average(s => s.Price),
                OrdersCount = n.Count()
            }).ToListAsync(cancellationToken);
        var rangeData = new SnapshotDataset
        {
            Name = "Range trend",
            Data = result.Select(r => r.AverageRange).ToArray(),
            Type = "area",
            Unit = "mi",
            ValueDecimals = 0
        };
        var ppmData = new SnapshotDataset
        {
            Name = "Price per mile trend",
            Data = result.Select(r => r.AveragePricePerMile).ToArray(),
            Type = "area",
            Unit = "$",
            ValueDecimals = 2
        };
        var priceData = new SnapshotDataset
        {
            Name = "Price trend",
            Data = result.Select(r => r.AveragePrice).ToArray(),
            Type = "area",
            Unit = "$",
            ValueDecimals = 2
        };
        var orderData = new SnapshotDataset
        {
            Name = "Order count trend",
            Data = result.Select(r => (decimal)r.OrdersCount).ToArray(),
            Type = "area",
            Unit = "orders",
            ValueDecimals = 0
        };
        var chartData = new SnapshotModel
        {
            Xdata = result.Select(r => ((DateTimeOffset)r.Date).ToUnixTimeMilliseconds()).ToArray(),
            Datasets = [rangeData, ppmData, priceData, orderData]
        };
        return chartData;
    }

    public async Task<List<OperabilityOverviewModel>> GetOperabilityTrend(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        return await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(g => new OperabilityOverviewModel
            {
                Date = g.Key,
                OperableCount = g.Count(e => !e.HasInoperable),
                InoperableCount = g.Count(e => e.HasInoperable)
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<TrailerTypesOverviewModel>> GetTrailersTrend(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        return await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(g => new TrailerTypesOverviewModel
            {
                Date = g.Key,
                EnclosedCount = g.Count(e => e.TrailerType == TrailerTypes.Enclosed),
                OpenCount = g.Count(e => e.TrailerType == TrailerTypes.Open),
                DriveawayCount = g.Count(e => e.TrailerType == TrailerTypes.Driveaway)
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<PaymentTypeOverviewModel>> GetPaymentsTrend(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        return await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(s => new PaymentTypeOverviewModel
            {
                Date = s.Key,
                CashCount = s.Count(e => e.PaymentType == PaymentTypes.Cash),
                CheckCount = s.Count(e => e.PaymentType == PaymentTypes.Check),
                CompanyCheckCount = s.Count(e => e.PaymentType == PaymentTypes.CompanyCheck),
                ComcheckCount = s.Count(e => e.PaymentType == PaymentTypes.Comcheck),
                TCHCount = s.Count(e => e.PaymentType == PaymentTypes.TCH),
                ACHCount = s.Count(e => e.PaymentType == PaymentTypes.ACH),
                SuperPayCount = s.Count(e => e.PaymentType == PaymentTypes.SuperPay),
                ZelleCount = s.Count(e => e.PaymentType == PaymentTypes.Zelle),
                UshipCount = s.Count(e => e.PaymentType == PaymentTypes.Uship),
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<VehicleCountOverviewModel>> GetVehicleCountTrend(AnalyzeFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        return await GetAnalyzeQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date)
            .Select(s => new VehicleCountOverviewModel
            {
                Date = s.Key,
                One = s.Count(e => e.VehicleCount == 1),
                Two = s.Count(e => e.VehicleCount == 2),
                Three = s.Count(e => e.VehicleCount == 3),
                Four = s.Count(e => e.VehicleCount == 4),
                Five = s.Count(e => e.VehicleCount == 5),
                Six = s.Count(e => e.VehicleCount == 6),
                Seven = s.Count(e => e.VehicleCount == 7),
                Eight = s.Count(e => e.VehicleCount == 8),
                Nine = s.Count(e => e.VehicleCount == 9)
            }).ToListAsync(cancellationToken);
    }

    public async Task<(List<LongIntTuple> result1, List<LongIntTuple> result2)> GetOrderCountComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var midQuery = GetCompareQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date);

        var result1 = await midQuery
            .Select(s => new LongIntTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Count = s.Count(c => c.PickupState == model.This!.Value)
            }).ToListAsync(cancellationToken);

        var result2 = await midQuery
            .Select(s => new LongIntTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Count = s.Count(c => model.Rest ? c.PickupState != model.This!.Value && c.PickupState != States.Canada : c.PickupState == model.That!.Value)
            }).ToListAsync(cancellationToken);

        return (result1, result2);
    }

    public async Task<(List<LongDecimalTuple> result1, List<LongDecimalTuple> result2)> GetRangeComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var midQuery = GetCompareQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date);

        var result1 = await midQuery
            .Select(s => new LongDecimalTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Average = s.Where(c => c.PickupState == model.This!.Value).Average(a => a.Distance)
            }).ToListAsync(cancellationToken);

        var result2 = await midQuery
            .Select(s => new LongDecimalTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Average = s.Where(c => model.Rest ? c.PickupState != model.This!.Value && c.PickupState != States.Canada : c.PickupState == model.That!.Value).Average(a => a.Distance)
            }).ToListAsync(cancellationToken);

        return (result1, result2);
    }

    public async Task<(List<LongDecimalTuple> result1, List<LongDecimalTuple> result2)> GetPriceComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var midQuery = GetCompareQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date);

        var result1 = await midQuery
            .Select(s => new LongDecimalTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Average = s.Where(c => c.PickupState == model.This!.Value).Average(a => a.Price)
            }).ToListAsync(cancellationToken);

        var result2 = await midQuery
            .Select(s => new LongDecimalTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Average = s.Where(c => model.Rest ? c.PickupState != model.This!.Value && c.PickupState != States.Canada : c.PickupState == model.That!.Value).Average(a => a.Price)
            }).ToListAsync(cancellationToken);

        return (result1, result2);
    }

    public async Task<(List<LongDecimalTuple> result1, List<LongDecimalTuple> result2)> GetPricePerMileComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var query = GetBaseQuery(model);
        var midQuery = GetCompareQuery(model, query)
            .GroupBy(g => g.CreatedDate.Date);

        var result1 = await midQuery
            .Select(s => new LongDecimalTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Average = s.Where(c => c.PickupState == model.This!.Value).Average(a => a.PricePerMile)
            }).ToListAsync(cancellationToken);

        var result2 = await midQuery
            .Select(s => new LongDecimalTuple
            {
                Timestamp = ((DateTimeOffset)s.Key).ToUnixTimeMilliseconds(),
                Average = s.Where(c => model.Rest ? c.PickupState != model.This!.Value && c.PickupState != States.Canada : c.PickupState == model.That!.Value).Average(a => a.PricePerMile)
            }).ToListAsync(cancellationToken);

        return (result1, result2);
    }

    public async Task<(DateTime startDate, DateTime endDate)> GetLowerAndUpperDates(CancellationToken cancellationToken)
    {
        var lowerDate = await context.Orders.MinAsync(o => o.DataCollectedAt, cancellationToken);
        var upperDate = await context.Orders.MaxAsync(o => o.DataCollectedAt, cancellationToken);
        return (lowerDate, upperDate);
    }

    public DbSet<LoadboardOrder> GetQuery()
    {
        return context.Orders;
    }
    
    private IQueryable<LoadboardOrder> GetBaseQuery(BaseFilterModel model)
    {
        return context.Orders
            .Where(o => !model.FromDate.HasValue || o.CreatedDate >= model.FromDate.Value)
            .Where(o => !model.ToDate.HasValue || o.CreatedDate <= model.ToDate.Value)
            .Where(o => !model.TrailerType.HasValue || o.TrailerType == model.TrailerType.Value)
            .Where(o => o.Price >= model.LowerPriceLimit && o.Price <= model.UpperPriceLimit)
            .Where(o => o.Distance >= model.LowerRangeLimit && o.Distance <= model.UpperRangeLimit)
            .Where(o => model.SelectedPlatforms.Count == 0 || model.SelectedPlatforms.Contains(o.SourcePlatform));
    }

    private IQueryable<LoadboardOrder> GetOverviewQuery(OverviewFilterModel model, IQueryable<LoadboardOrder> query)
    {
        return query
            .Where(o => model.ExcludedStates != null &&
                        (!model.ExcludePickup || !model.ExcludedStates.Contains(o.PickupState)))
            .Where(o => model.ExcludedStates != null &&
                        (!model.ExcludeDelivery || !model.ExcludedStates.Contains(o.DeliveryState)));
    }

    private IQueryable<LoadboardOrder> GetAnalyzeQuery(AnalyzeFilterModel model, IQueryable<LoadboardOrder> query)
    {
        return query.Where(o =>
            !model.SelectedState.HasValue ||
            (model.SelectedState.HasValue && o.DeliveryState == model.SelectedState.Value));
    }

    private IQueryable<LoadboardOrder> GetCompareQuery(CompareFilterModel model, IQueryable<LoadboardOrder> query)
    {
        return query.Where(q => model.Rest || (q.PickupState == model.This || q.PickupState == model.That));
    }
}
