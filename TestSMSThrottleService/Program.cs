using TestSMSThrottleService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

QuotaService quotaService = new QuotaService();
TcpServerService tcpConnectionService = new TcpServerService(quotaService);
builder.Services.AddSingleton<QuotaService>(quotaService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Test performance of Quota Service using multiple client threads connecting via TCP sockets
for(int i=0; i< 10; i++) {
    ClientEmulatorService client1 = new();
    //client1.RunUsingThreadsViaDirectMethodCall(quotaService);     // 7539k requests/s
    //client1.RunUsingThreadsViaTCP();                              // 122K requests/s
    client1.RunUsingTasksViaTCP();                                  // 133k requests/s
}


app.Run();

