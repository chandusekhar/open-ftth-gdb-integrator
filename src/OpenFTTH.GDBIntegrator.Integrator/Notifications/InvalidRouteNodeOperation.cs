using System;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.GDBIntegrator.Integrator.Notifications
{
    public class InvalidRouteNodeOperation : INotification
    {
        public RouteNode RouteNode { get; set; }
        public string Message { get; set; }
    }

    public class InvalidRouteNodeOperationHandler : INotificationHandler<InvalidRouteNodeOperation>
    {
        private readonly IGeoDatabase _geoDatabase;
        private readonly ILogger<InvalidRouteNodeOperationHandler> _logger;

        public InvalidRouteNodeOperationHandler(IGeoDatabase geoDatabase, ILogger<InvalidRouteNodeOperationHandler> logger)
        {
            _geoDatabase = geoDatabase;
            _logger = logger;
        }

        public async Task Handle(InvalidRouteNodeOperation request, CancellationToken token)
        {
            _logger.LogWarning($"Deleteting {nameof(RouteNode)} with mrid '{request.RouteNode.Mrid}'. Because: {request.Message}");
            await _geoDatabase.DeleteRouteNode(request.RouteNode.Mrid);
        }
    }
}
