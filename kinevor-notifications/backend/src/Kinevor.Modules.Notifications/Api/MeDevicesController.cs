using Kinevor.Modules.Notifications.Application;
using Kinevor.Platform.Api;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinevor.Modules.Notifications.Api;

/// <summary>Member push device endpoints.</summary>
[ApiController, Route("api/v1/me/devices"), Authorize(Policy = Policies.MemberOrAbove)]
public class MeDevicesController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Lists the authenticated member's registered devices.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeviceTokenDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeviceTokenDto>>>> List(CancellationToken ct)
    {
        var result = await sender.Send(new GetMyDeviceTokensQuery(MemberId), ct);
        return Ok(ApiResponse<IReadOnlyList<DeviceTokenDto>>.Ok(result));
    }

    /// <summary>Registers or re-assigns a push device token.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DeviceTokenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeviceTokenDto>>> Register(
        [FromBody] RegisterDeviceTokenRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new RegisterDeviceTokenCommand(MemberId, request.Token, request.Platform), ct);
        return Ok(ApiResponse<DeviceTokenDto>.Ok(result, "Device registered."));
    }

    /// <summary>Unregisters one of the authenticated member's devices.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Unregister(Guid id, CancellationToken ct)
    {
        await sender.Send(new UnregisterDeviceTokenCommand(MemberId, id), ct);
        return Ok(ApiResponse.Ok("Device removed."));
    }

    private Guid MemberId =>
        currentUser.MemberId ?? throw new ForbiddenException("No linked member profile.");
}

/// <summary>Push device registration request body.</summary>
public record RegisterDeviceTokenRequest(string Token, string Platform);
