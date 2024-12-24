using CommunityToolkit.Mvvm.Input;
using FakeFlightBookingAPI.Models;
using FakeFlightBookingAPI.Services;
using FakeFlightBookingApp.Model;
using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FakeFlightBookingApp.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly FlightOffersSearchService _flightOffersSearchService;
        private readonly AirportLookupService _airportLookupService;

        // Properties for User Input
        private string _flightFrom;
        private string _flightTo;
        private DateTime? _departureDate;
        private DateTime? _returnDate;
        private int _numberOfTickets;
        private string _travelClass;
        private string _statusMessage;

        private ObservableCollection<Airport> _availableAirportsFrom = new ObservableCollection<Airport>();
        private ObservableCollection<Airport> _availableAirportsTo = new ObservableCollection<Airport>();

        private bool _isSearchButtonEnabled = true;
        public bool IsSearchButtonEnabled
        {
            get => _isSearchButtonEnabled;
            set
            {
                if (_isSearchButtonEnabled != value)
                {
                    _isSearchButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }



        // Property to hold the search results
        public ObservableCollection<FlightOffer> FlightOffers { get; private set; } = new ObservableCollection<FlightOffer>();

        public MainPageViewModel(FlightOffersSearchService flightOffersSearchService, AirportLookupService airportLookupService)
        {
            _flightOffersSearchService = flightOffersSearchService;
            _airportLookupService = airportLookupService;
            SearchFlightsCommand = new RelayCommand(async () => await Search());
            //SearchFlightsCommand = new RelayCommand(async () => await SearchFlightsFromNearbyAirports());


        }


        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged(); }
        }


        public string FlightFrom
        {
            get { return _flightFrom; }
            set { _flightFrom = value; OnPropertyChanged(); }
        }

        public string FlightTo
        {
            get { return _flightTo; }
            set { _flightTo = value; OnPropertyChanged(); }
        }

        public DateTime? DepartureDate
        {
            get { return _departureDate; }
            set { _departureDate = value; OnPropertyChanged(); }
        }

        public DateTime? ReturnDate
        {
            get { return _returnDate; }
            set { _returnDate = value; OnPropertyChanged(); }
        }

        public int NumberOfTickets
        {
            get { return _numberOfTickets; }
            set { _numberOfTickets = value; OnPropertyChanged(); }
        }

        public string TravelClass
        {
            get { return _travelClass; }
            set { _travelClass = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Airport> AvailableAirportsFrom
        {
            get { return _availableAirportsFrom; }
        }

        public ObservableCollection<Airport> AvailableAirportsTo
        {
            get { return _availableAirportsTo; }
        }

        public ICommand SearchFlightsCommand { get; }

        private async Task SearchAirports(string keyword, bool isFromAirport)
        {
            // Retrieve airports based on user input
            var airports = await _airportLookupService.SearchAirportsAsync(keyword);

            // Clear old results
            if (isFromAirport)
                _availableAirportsFrom.Clear();
            else
                _availableAirportsTo.Clear();

            // Add the retrieved airports to the corresponding collection
            foreach (var airport in airports)
            {
                if (isFromAirport)
                {
                    _availableAirportsFrom.Add(airport);
                    Console.WriteLine(airport.Name);

                }
                else 
                {
                    _availableAirportsTo.Add(airport);
                }
            }
        }

        // Method to search flights from nearby airports
        private async Task SearchFlightsFromNearbyAirports()
        {
            IsSearchButtonEnabled = false; // Disable the button
            StatusMessage = "Searching for flights...";

            try
            {
                // Validate user input
                if (string.IsNullOrWhiteSpace(FlightFrom) || string.IsNullOrWhiteSpace(FlightTo))
                {
                    ShowError("Origin and destination cannot be empty.");
                    return;
                }


                // Search for nearby airports for the origin
                await SearchAirports(FlightFrom, true);
                await SearchAirports(FlightTo, false);

                // Make sure at least one airport is available for both origin and destination
                if (_availableAirportsFrom.Count == 0 || _availableAirportsTo.Count == 0)
                {
                    ShowError("No nearby airports found for the given locations.");
                    return;
                }

                // Create a list to store flight offers
                FlightOffers.Clear();

                // Loop through all available origin airports and search for flights
                foreach (var fromAirport in _availableAirportsFrom)
                {
                    foreach (var toAirport in _availableAirportsTo)
                    {
                        var criteria = new FlightSearchCriteria
                        {
                            From = fromAirport.IataCode,
                            To = toAirport.IataCode,
                            DepartureDate = DepartureDate?.ToString("yyyy-MM-dd"), // Ensure the date is in correct format
                            ReturnDate = ReturnDate?.ToString("yyyy-MM-dd"),
                            NumberOfTickets = NumberOfTickets,
                            TravelClass = TravelClass
                        };

                        // Call the search function in your service
                        var response = await _flightOffersSearchService.SearchFlightsAsync(criteria);

                        if (response.IsSuccessStatusCode)
                        {
                            var flightOffers = await response.Content.ReadFromJsonAsync<List<FlightOffer>>();
                            foreach (var offer in flightOffers)
                            {
                                FlightOffers.Add(offer); // Add each flight offer to the collection
                                StatusMessage = "Flights found!";

                            }
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync(); // Optionally read the response content
                            ShowError($"Error searching flights: {errorMessage}");
                            StatusMessage = errorMessage;
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                ShowError($"Request error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                ShowError($"Unexpected error: {ex.Message}");
            }
            finally 
            {
                IsSearchButtonEnabled = true; // Disable the button

            }

        }

        private async Task Search()
        {
            var criteria = new FlightSearchCriteria
            {
                From = FlightFrom,
                To = FlightTo,
                DepartureDate = DepartureDate?.ToString("yyyy-MM-dd"), // Ensure the date is in correct format
                ReturnDate = ReturnDate?.ToString("yyyy-MM-dd"),
                NumberOfTickets = NumberOfTickets,
                TravelClass = TravelClass
            };

            var response = await _flightOffersSearchService.SearchFlightsAsync(criteria);

            if (response.IsSuccessStatusCode)
            {
                var flightOffers = await response.Content.ReadFromJsonAsync<List<FlightOffer>>();
                foreach (var offer in flightOffers)
                {
                    FlightOffers.Add(offer); // Add each flight offer to the collection
                    StatusMessage = "Flights found!";

                }
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync(); // Optionally read the response content
                ShowError($"Error searching flights: {errorMessage}");
                StatusMessage = errorMessage;
            }
        }



        private void ShowError(string message)
        {
            // Implement your error display logic here, e.g., using MessageBox or a UI element
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
