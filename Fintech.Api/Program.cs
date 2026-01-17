using Fintech.Interfaces;
using Fintech.Middlewares;
using Fintech.Persistence;

using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configs
MongoClassMaps.Register();
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration.GetConnectionString("Mongo")));
builder.Services.AddScoped<MongoContext>();
builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoContext>());
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

app.UseMiddleware<IdempotencyMiddleware>();
app.MapControllers();
app.Run();