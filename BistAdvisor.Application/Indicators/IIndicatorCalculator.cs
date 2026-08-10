using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public interface IIndicatorCalculator
{
    IndicatorResult Calculate(Stock stock, IReadOnlyList<PriceBar> priceBars);
}