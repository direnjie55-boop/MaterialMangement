using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace MaterialMangement.Services;

///<summary>
/// 库存变动消息
/// </summary>
public class StockOperationMessage
{
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal BeforeQuantity { get; set; }
    public decimal AfterQuantity { get; set; }
    public string? Operator { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

}

public interface IMessageService
{
    Task PublishStockOperationAsync(StockOperationMessage message);
    ValueTask DisposeAsync();
}
public class RabbitMqService : IMessageService, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _initialized;

    public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    private async Task EnsureInitializedAsync()
    {
        if (_initialized && _channel?.IsOpen == true) return;
        try
        {
            var rabbitSection = _configuration.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = rabbitSection["HostName"] ?? "localhost",
                Port = int.Parse(rabbitSection["Port"] ?? "5672"),
                UserName = rabbitSection["UserName"] ?? "guest",
                Password = rabbitSection["Password"] ?? "guest"
            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            var exchangeName = rabbitSection["ExchangeName"] ?? "material_exchange";
            var queueName = rabbitSection["QueueName"] ?? "stock_operation_queue";

            // 声明交换机和队列
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: "");

            _initialized = true;
            _logger.LogInformation("RabbitMQ 连接成功");

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ 连接失败，消息将被跳过。请确保 RabbitMQ 服务正在运行。");
            _initialized = false;
        }
    }

    public async Task PublishStockOperationAsync(StockOperationMessage message)
    {
        await EnsureInitializedAsync();

        if (_channel == null || !_initialized)
        {
            _logger.LogWarning("RabbitMQ 不可用，跳过消息发布: {Operation} - {MaterialName}",
                message.Operation, message.MaterialName);
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            var exchangeName = _configuration.GetSection("RabbitMQ")["ExchangeName"] ?? "material_exchange";

            await _channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: "",
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("消息已发布: {Operation} - {MaterialName} - 数量:{Quantity}",
                message.Operation, message.MaterialName, message.Quantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发布消息失败");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}

