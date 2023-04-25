using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;

namespace MqttPlayground
{
    public class MqttSubscriber : BackgroundService
    {
        private readonly Stopwatch _stopwatch;
        private readonly ILogger<MqttSubscriber> _logger;

        public MqttSubscriber(ILogger<MqttSubscriber> logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Publisher running at: {time}", DateTimeOffset.Now);
                var factory = new MqttFactory();
                using (var client = factory.CreateMqttClient())
                {
                    var options = new MqttClientOptionsBuilder()
                    .WithClientId("PlaygroundSubscriber")
                    .WithTcpServer("localhost", 1883)
                    .WithCleanSession(true)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))
                    .Build();

                    // Specify what to do with message 
                    client.ApplicationMessageReceivedAsync += e =>
                    {
                        _stopwatch.Stop();
                        _logger.LogInformation($"Received application message from topic: {e.ApplicationMessage.Topic}.ElapsedTime: {_stopwatch.ElapsedMilliseconds}");
                        _logger.LogInformation("Payload: " + Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
                        return Task.CompletedTask;
                    };

                    // Connect to server 
                    await client.ConnectAsync(options, cancellationToken);
                    _logger.LogInformation("mqtt sub client is connected");

                    // Subscribe to Topic 
                    var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic("playground/number");
                        })
                    .Build();


                    var response = await client.SubscribeAsync(mqttSubscribeOptions, cancellationToken);
                    _logger.LogInformation("MQTT client subscribed to topic.");
                    _logger.LogInformation($"{response}");

                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
            }
        }
    }
}
