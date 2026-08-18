using LateralCms.Api.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;

namespace LateralCms.Api.Controllers;

public abstract class LateralCmsController : ControllerBase
{
    public override OkObjectResult Ok(object? value)
    {
        var response = WrapResponse((int)HttpStatusCode.OK, value);
        return base.Ok(response);
    }

    public override AcceptedResult Accepted(object? value)
    {
        var response = WrapResponse((int)HttpStatusCode.Accepted, value);
        return base.Accepted(response);
    }

    public override NotFoundObjectResult NotFound(object? value)
    {
        var response = WrapResponse((int)HttpStatusCode.NotFound, value);
        return base.NotFound(response);
    }

    public override BadRequestObjectResult BadRequest(object? value)
    {
        var response = WrapResponse((int)HttpStatusCode.BadRequest, value);
        return base.BadRequest(response);
    }

    public override UnprocessableEntityObjectResult UnprocessableEntity(object? value)
    {
        var response = WrapResponse((int)HttpStatusCode.UnprocessableEntity, value);
        return base.UnprocessableEntity(response);
    }

    public override ObjectResult StatusCode(int statusCode, object? value)
    {
        var response = WrapResponse(statusCode, value);
        return base.StatusCode(statusCode, response);
    }

    private static ResponseWrapper WrapResponse(int statusCode, object? value = null)
    {
        var success = statusCode >= 200 && statusCode < 300;

        return new ResponseWrapper
        {
            Success = success,
            HttpStatusCode = statusCode,
            HttpStatus = ReasonPhrases.GetReasonPhrase(statusCode),
            Message = success ? null : value?.ToString() ?? string.Empty,
            Data = success ? value : null,
        };
    }
}
