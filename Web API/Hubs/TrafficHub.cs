using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Econolite_API.Hubs;

[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
public sealed class TrafficHub : Hub;