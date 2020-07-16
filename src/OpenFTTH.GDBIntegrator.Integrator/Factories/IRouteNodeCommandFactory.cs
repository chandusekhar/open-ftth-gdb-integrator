using MediatR;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public interface IRouteNodeCommandFactory
    {
        Task<IRequest> Create(RouteNode routeNode);
    }
}
