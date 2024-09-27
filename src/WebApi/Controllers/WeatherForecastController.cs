namespace Phoenix.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Phoenix.WebApi.Queries;

[ApiController, Route("[controller]")]
public sealed class WeatherForecastController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast"), ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken = default)
    {
        var query = new GetWeatherForecastsQuery();

        return await mediator.Send(query, cancellationToken);
    }
}
