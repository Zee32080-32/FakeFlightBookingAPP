using FakeFlightBookingAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace FakeFlightBookingApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; } // Add this property

        protected override void OnStartup(StartupEventArgs e)
        {
            // Build configuration from appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build(); // Build the configuration

            var serviceCollection = new ServiceCollection();

            // Configure services
            serviceCollection.AddHttpClient<FlightOffersSearchService>();
            serviceCollection.AddHttpClient<AirportLookupService>();
            serviceCollection.Configure<AmadeusOptions>(Configuration.GetSection("Amadeus")); // Now you can access Configuration

            ServiceProvider = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);
        }
    }
}
