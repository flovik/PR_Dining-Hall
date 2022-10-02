using DiningHall.Interfaces;
using DiningHall.Models;
using DiningHall.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<DiningHallService>();
builder.Services.AddSingleton<IDiningHallSender, DiningHallSender>();
builder.Services.AddSingleton<IDiningHallNotifier, DiningHallNotifier>();
builder.Services.AddSingleton<IRatingSystem, RatingSystem>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

Thread.Sleep(500);

//create diningHallService to start the program
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    var _ = services.GetRequiredService<DiningHallService>();
}

app.Run();