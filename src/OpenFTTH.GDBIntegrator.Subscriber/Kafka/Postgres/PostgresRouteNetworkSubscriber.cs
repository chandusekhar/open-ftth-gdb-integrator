using System;
using System.Threading.Tasks;
using Topos.Config;
using MediatR;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Postgres
{
    public class PostgresRouteNetworkSubscriber : IRouteNetworkSubscriber
    {
        private IDisposable _consumer;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IMediator _mediator;
        private readonly ILogger<PostgresRouteNetworkSubscriber> _logger;

        public PostgresRouteNetworkSubscriber(
            IOptions<KafkaSetting> kafkaSetting,
            IMediator mediator,
            ILogger<PostgresRouteNetworkSubscriber> logger
            )
        {
            _kafkaSetting = kafkaSetting.Value;
            _mediator = mediator;
            _logger = logger;
        }

        public void Subscribe()
        {
            _consumer = Configure
                .Consumer(_kafkaSetting.PostgisRouteNetworkConsumer, c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.RouteNetwork())
                .Topics(t => t.Subscribe(_kafkaSetting.PostgisRouteNetworkTopic))
                .Positions(p => p.StoreInFileSystem(_kafkaSetting.PositionFilePath))
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages)
                    {
                        if (message.Body is RouteNodeMessage)
                        {
                            var routeNode = (RouteNodeMessage)message.Body;
                            await HandleSubscribedEvent(routeNode);
                        }
                        else if (message.Body is RouteSegmentMessage)
                        {
                            var routeSegment = (RouteSegmentMessage)message.Body;
                            await HandleSubscribedEvent(routeSegment);
                        }
                    }
                }).Start();
        }

        private async Task HandleSubscribedEvent(RouteNodeMessage routeNodeMessage)
        {
            _logger.LogDebug($"{DateTime.UtcNow.ToString("o")}: Received message {JsonConvert.SerializeObject(routeNodeMessage, Formatting.Indented)}");
            await _mediator.Send(new GeoDatabaseUpdated { UpdateMessage = routeNodeMessage });
        }

        private async Task HandleSubscribedEvent(RouteSegmentMessage routeSegmentMessage)
        {
            _logger.LogDebug($"{DateTime.UtcNow.ToString("o")}: Received message {JsonConvert.SerializeObject(routeSegmentMessage, Formatting.Indented)}");
            await _mediator.Send(new GeoDatabaseUpdated { UpdateMessage = routeSegmentMessage });
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}