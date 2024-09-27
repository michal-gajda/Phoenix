namespace Phoenix.WebApi.Queries;

using MediatR;

public sealed record class GetWeatherForecastsQuery : IRequest<IEnumerable<WeatherForecast>>
{
}
