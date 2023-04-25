using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;

namespace MqttPlayground
{
    public class MqttPublisher : BackgroundService
    {
        private readonly Stopwatch _stopwatch;
        private readonly ILogger<MqttPublisher> _logger;

        public MqttPublisher(ILogger<MqttPublisher> logger, Stopwatch stopwatch)
        {
            _logger = logger;
            _stopwatch = stopwatch;
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
                    .WithClientId("PlaygroundPublisher")
                    .WithTcpServer("localhost", 1883)
                    .WithCleanSession(true)
                    .Build();

                    await client.ConnectAsync(options, cancellationToken);
                    _logger.LogInformation("mqtt pub client is connected.");

                    while (true)
                    {
                        // Generate payload and build message 
                        var payload = GeneratePayload();
                        var applicationMessage = GenerateMqttApplicationMessage(payload);

                        // Start stopwatch 
                        _stopwatch.Reset();
                        _stopwatch.Start();

                        // Publish message
                        await client.PublishAsync(applicationMessage, cancellationToken);
                        _logger.LogInformation($"Message is published to topic: {applicationMessage.Topic}. Payload: {Encoding.UTF8.GetString(applicationMessage.PayloadSegment)}");

                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
        }


        private string GeneratePayload()
        {
            // Generate Payload
            Random random = new Random();
            string randomNumber = random.Next(0, 100).ToString();

            return randomNumber;
        }

        private MqttApplicationMessage GenerateMqttApplicationMessage(string payload)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("playground/number")
                .WithPayload(payload)
                .Build();

            return applicationMessage;
        }
    }
}