using FakeFlightBookingApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FakeFlightBookingApp.View
{
    public partial class MainPageView : Page
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7186") };
        public ObservableCollection<FlightOfferDTO> FlightOffersCollection { get; set; } = new ObservableCollection<FlightOfferDTO>();

        public MainPageView()
        {
            InitializeComponent();
            //FlightOffers.ItemsSource = FlightOffersCollection;
        }

        private async void SearchFlights_Button_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Content = "Searching...";
            var flightOffersCollection = new List<FlightOfferDTO>();

            // Gather input values
            string from = FlightFrom_TextBox.Text;
            string to = FlightTo_TextBox.Text;
            string departureDate = DepartureDate_DatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            string returnDate = ArrivalDate_DatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            int.TryParse(NumberOfTicket_TextBox.Text, out int numberOfTickets);
            string travelClass = (TravelClass_ComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

 

            // Build search query display string
            string searchQuery = $"Flights from {from} to {to} " +
                                 $"| Leave on: {departureDate} " +
                                 (string.IsNullOrEmpty(returnDate) ? "" : $"| Return on: {returnDate} ") +
                                 $"| Tickets: {numberOfTickets} | Class: {travelClass}";

            // Build the request URL
            string url = $"/api/Flights/search?From={from}&To={to}&DepartureDate={departureDate}&ReturnDate={returnDate}&NumberOfTickets={numberOfTickets}&TravelClass={travelClass}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var flightOfferResponse = await response.Content.ReadFromJsonAsync<SharedModels.FlightOffersResponse>();

                    if (flightOfferResponse?.Data != null)
                    {
                        foreach (var offer in flightOfferResponse.Data)
                        {
                            flightOffersCollection.Add(new FlightOfferDTO
                            {
                                FlightNumber = offer.Itineraries?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Number,
                                Price = offer.Price.Total,
                                DepartureTime = offer.Itineraries?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Departure?.At.ToString("g"),
                                ArrivalTime = offer.Itineraries?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Arrival?.At.ToString("g"),
                                Duration = offer.Itineraries?.FirstOrDefault()?.Duration,
                                NumberOfTickets = numberOfTickets,

                                Origin = offer.Itineraries?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Departure?.IataCode, // Ensure this is set
                                Destination = offer.Itineraries?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Arrival?.IataCode, // Ensure this is set
                                AirlineName = offer.ValidatingAirlineCodes?.FirstOrDefault(), // Ensure this is set
                                ClassType = travelClass // Ensure this is passed correctly
                            });
                        }

                        StatusLabel.Content = "Search completed.";

                        // Navigate to the new FlightTicketPage with the results and query
                        var flightTicketPage = new FlightTicketPage(flightOffersCollection, searchQuery);
                        NavigationService?.Navigate(flightTicketPage);
                    }
                    else
                    {
                        StatusLabel.Content = "No flights found.";
                    }
                }
                else
                {
                    StatusLabel.Content = $"Error: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Exception: {ex.Message}";
            }
        }

        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the selected flight item from the DataContext (assuming the button's parent has the flight data bound)
            var selectedFlight = ((Button)sender).DataContext as FlightOfferDTO;
            if (selectedFlight != null)
            {
                // Navigate to PaymentPage, passing the selected flight as a parameter
                var paymentPage = new PaymentPage(selectedFlight);
                NavigationService?.Navigate(paymentPage);
            }
        }
    }
}
