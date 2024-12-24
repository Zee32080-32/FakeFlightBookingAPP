using Microsoft.EntityFrameworkCore;
using FakeFlightBookingAPI.Models;
using FakeFlightBookingAPI.Data;
using FakeFlightBookingAPI.Services;
using System.Configuration;
using Microsoft.Extensions.Options;
using Stripe;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));
builder.Services.AddSingleton<EmailService>();

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddSingleton<PaymentService>();

/*
builder.Services.AddSingleton<StripeClient>(sp =>
{
    var stripeSettings = sp.GetRequiredService<IOptions<StripeOptions>>().Value;
    return new StripeClient(stripeSettings.SecretKey);
});
*/

//builder.Services.AddScoped<PaymentService>();


/*
//configures AmadeusOptions from appsettings.json
builder.Services.Configure<AmadeusOptions>(builder.Configuration.GetSection("Amadeus"));
//register the FlightOffersSearchService for dependency injection
builder.Services.AddHttpClient<FlightOffersSearchService>();
builder.Services.AddHttpClient<AirportLookupService>();
*/

// Configures AmadeusOptions from appsettings.json
builder.Services.Configure<AmadeusOptions>(builder.Configuration.GetSection("Amadeus"));

// Register the AirportLookupService for dependency injection
builder.Services.AddHttpClient<AirportLookupService>()
    .ConfigureHttpClient((sp, client) =>
    {
        // Inject AmadeusOptions to configure the base URL and other settings
        var amadeusOptions = sp.GetRequiredService<IOptions<AmadeusOptions>>().Value;
        client.BaseAddress = new Uri(amadeusOptions.BaseUrl);
    });

// Register other services
builder.Services.AddHttpClient<FlightOffersSearchService>();

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

app.Run();
