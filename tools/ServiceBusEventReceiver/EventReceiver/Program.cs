using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

// 1. Define friendly command-line switch mappings
// This allows you to use -t instead of --ServiceBus:TopicName
var switchMappings = new Dictionary<string, string>()
{
    { "-t", "ServiceBus:TopicName" },
    { "--topic", "ServiceBus:TopicName" },
    { "-s", "ServiceBus:SubscriptionName" },
    { "--subscription", "ServiceBus:SubscriptionName" }
};

// 2. Build the configuration hierarchy
// The order matters: AddCommandLine is added last so it takes highest priority.
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args, switchMappings)
    .Build();

// 3. Extract the final values
string? connectionString = config["ServiceBus:ConnectionString"];
string? topicName = config["ServiceBus:TopicName"];
string? subscriptionName = config["ServiceBus:SubscriptionName"];

if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(topicName) || string.IsNullOrWhiteSpace(subscriptionName))
{
    Console.WriteLine("Error: Connection string, topic, and subscription must be configured.");
    Console.WriteLine("Usage: dotnet run -- -t <TopicName> -s <SubscriptionName>");
    return;
}

Console.WriteLine($"Starting receiver for Topic: '{topicName}' | Subscription: '{subscriptionName}'...");

// 4. Initialize Service Bus
await using var client = new ServiceBusClient(connectionString);
await using var receiver = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

receiver.ProcessMessageAsync += MessageHandler;
receiver.ProcessErrorAsync += ErrorHandler;

await receiver.StartProcessingAsync();

Console.WriteLine("Listening for messages... Press any key to end.");
Console.ReadKey();

await receiver.StopProcessingAsync();

// Handlers
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"\n[Message Received at {DateTime.Now:HH:mm:ss}]");
    Console.WriteLine($"Message ID: {args.Message.MessageId}");
    Console.WriteLine($"Body: {body}");

    // Complete the message so it is removed from the emulator's database
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[Error] {args.Exception.Message}");
    Console.ResetColor();
    return Task.CompletedTask;
}