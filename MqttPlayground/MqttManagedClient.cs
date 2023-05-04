using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Diagnostics;

namespace MqttPlayground
{
    public class MqttManagedClient : BackgroundService
    {
        private readonly Stopwatch _stopwatch;
        private readonly ILogger<MqttPublisher> _logger;

        public MqttManagedClient(ILogger<MqttPublisher> logger, Stopwatch stopwatch)
        {
            _logger = logger;
            _stopwatch = stopwatch;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactroy = new MqttFactory();

            using (var managedClient = mqttFactroy.CreateManagedMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost", 1883)
                    .WithWillTopic("ManagedTopic/status")
                    .WithWillPayload("offline")
                    .Build();

                var managedMqttClientOpctions = new ManagedMqttClientOptionsBuilder()
                    .WithClientOptions(mqttClientOptions)
                    .Build();

                await managedClient.StartAsync(managedMqttClientOpctions);
                await managedClient.EnqueueAsync("ManagedTopic/status", "online");
                await managedClient.EnqueueAsync("ManagedTopic", "ManagedPayload");
                _logger.LogInformation("mqtt pub client is connected.");

                SpinWait.SpinUntil(() => managedClient.PendingApplicationMessagesCount == 0, 1000);

                _logger.LogInformation($"Pending messages = {managedClient.PendingApplicationMessagesCount}");

                await managedClient.StopAsync();
            }
        }
    }
}
