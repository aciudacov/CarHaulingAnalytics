using CarHaulingAnalytics.Data.Context;
using CarHaulingAnalytics.Data.Enums;
using CarHaulingAnalytics.Data.Models;
using CarHaulingAnalytics.Data.Models.Widgets;
using EnumsNET;
using Microsoft.EntityFrameworkCore;

namespace CarHaulingAnalytics.Data;

public class WidgetDataService
{
    private readonly AnalyticContext _context;

    public WidgetDataService(AnalyticContext context)
    {
        _context = context;
    }

    public async Task<int> GetOrderCount(OverviewFilterModel model)
    {
        return await GetBaseQuery(model)
            .CountAsync();
    }

    public async Task<IEnumerable<StringCountTuple>> GetCountByPickupState(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(o => o.PickupState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                OrdersCount = n.Count()
            })
            .OrderBy(o => o.State)
            .ToListAsync();

        return result.Select(r => new StringCountTuple
        {
            Value = r.State.AsString(),
            Count = r.OrdersCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetCountByDeliveryState(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(o => o.DeliveryState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                OrdersCount = n.Count()
            })
            .OrderBy(o => o.State)
            .ToListAsync();

        return result.Select(r => new StringCountTuple
        {
            Value = r.State.AsString(),
            Count = r.OrdersCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetPopularRoutes(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(o => new{ PickupState = o.PickupState, DeliveryState = o.DeliveryState })
            .Where(w => w.Key.PickupState != States.Canada && w.Key.DeliveryState != States.Canada)
            .Select(g => new
            {
                PickupState = g.Key.PickupState,
                DeliveryState = g.Key.DeliveryState,
                OrderCount = g.Count()
            }).OrderByDescending(o => o.OrderCount)
            .Take(20)
            .ToListAsync();

        return result.Select(r => new StringCountTuple()
        {
            Value = $"{r.PickupState.AsString()} to {r.DeliveryState.AsString()}",
            Count = r.OrderCount
        });
    }

    public async Task<IEnumerable<PickupStateAveragePrice>> GetAveragePriceByPickupState(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(o => o.PickupState)
            .Where(g => g.Key != States.Canada)
            .Select(n => new
            {
                State = n.Key,
                AveragePrice = n.Average(a => a.Price)
            })
            .OrderBy(o => o.State)
            .ToListAsync();

        return result.Select(r => new PickupStateAveragePrice
        {
            State = r.State.AsString(),
            AveragePrice = decimal.Round(r.AveragePrice, 2, MidpointRounding.AwayFromZero)
        });
    }

    public async Task<IEnumerable<PickupStateAveragePrice>> GetAveragePricePerMileByPickupState(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
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
            .ToListAsync();

        return result.Select(r => new PickupStateAveragePrice
        {
            State = r.State.AsString(),
            AveragePrice = decimal.Round(r.AveragePrice, 2, MidpointRounding.AwayFromZero)
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetShipperOrderCount(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .Where(o => o.PricePerMile > 0 && o.PricePerMile < 300)
            .GroupBy(o => o.ShipperName)
            .Select(n => new
            {
                ShipperName = n.Key,
                OrderCount = n.Count()
            })
            .OrderByDescending(o => o.OrderCount)
            .Take(10)
            .ToListAsync();

        return result.Select(r => new StringCountTuple
        {
            Value = r.ShipperName,
            Count = r.OrderCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetPaymentTypesCount(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .Where(o => o.PricePerMile > 0 && o.PricePerMile < 300)
            .GroupBy(o => o.PaymentType)
            .Select(n => new
            {
                PaymentType = n.Key,
                OrderCount = n.Count()
            })
            .OrderBy(o => o.PaymentType)
            .ToListAsync();

        return result.Select(r => new StringCountTuple
        {
            Value = r.PaymentType.AsString(),
            Count = r.OrderCount
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetVehicleStatuses(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(g => g.HasInoperable)
            .Select(n => new
            {
                Inop = n.Key,
                Count = n.Count()
            }).ToListAsync();

        return result.Select(r => new StringCountTuple
        {
            Value = r.Inop ? "Inoperable" : "Operable",
            Count = r.Count
        });
    }

    public async Task<IEnumerable<StringCountTuple>> GetTrailerTypes(OverviewFilterModel model)
    {
        var result = await GetBaseQuery(model)
            .GroupBy(g => g.TrailerType)
            .Select(n => new
            {
                TrailerType = n.Key,
                Count = n.Count()
            }).ToListAsync();

        return result.Select(r => new StringCountTuple
        {
            Value = r.TrailerType.AsString(),
            Count = r.Count
        });
    }

    public async Task<(DateTime startDate, DateTime endDate)> GetLowerAndUpperDates()
    {
        var lowerDate = await _context.Orders.MinAsync(o => o.DataCollectedAt);
        var upperDate = await _context.Orders.MaxAsync(o => o.DataCollectedAt);
        return (lowerDate, upperDate);
    }
    
    private IQueryable<LoadboardOrder> GetBaseQuery(OverviewFilterModel model)
    {
        return _context.Orders
            .Where(o => !model.Date.HasValue || o.CreatedDate.Day == model.Date.Value.Day)
            .Where(o => !model.TrailerType.HasValue || o.TrailerType == model.TrailerType.Value)
            .Where(o => o.Price >= model.PriceLimits.Min() && o.Price <= model.PriceLimits.Max())
            .Where(o => o.Distance >= model.RangeLimits.Min() && o.Distance <= model.RangeLimits.Max())
            .Where(o => !model.ExcludePickup || !model.ExcludedStates.Contains(o.PickupState))
            .Where(o => !model.ExcludeDelivery || !model.ExcludedStates.Contains(o.DeliveryState));
    }
}
