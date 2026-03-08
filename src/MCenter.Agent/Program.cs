using MCenter.Agent.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register Services
builder.Services.AddSingleton<UiPathExecutor>();
builder.Services.AddHostedService<AgentWorker>();

var host = builder.Build();
host.Run();
