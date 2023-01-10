using order_api.Managers;
using order_api.Models;

var builder = WebApplication.CreateBuilder(args);

var AllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins,
                      builder =>
                      {
                      builder.WithOrigins(new string[]  { "http://localhost:3001", "http://localhost:3000", "http://localhost:12345" } )
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                      });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add a single instance of the OrderManager to the API
builder.Services.AddSingleton<IOrderManager, OrderManager>();
builder.Services.AddSingleton<IOrderWebSocketManager, OrderWebSocketManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseCors(AllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
