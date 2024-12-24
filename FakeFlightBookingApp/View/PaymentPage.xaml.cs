using FakeFlightBookingApp.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using SharedModels;
using System.Globalization;

namespace FakeFlightBookingApp.View
{
    public partial class PaymentPage : Page
    {
        public FlightOfferDTO FlightOffer { get; set; }

        public PaymentPage(FlightOfferDTO flightOffer)
        {
            InitializeComponent();
            FlightOffer = flightOffer;

            // Bind flight details to UI
            FlightDetailsTextBlock.Text = $"Flight Number: {FlightOffer.FlightNumber}";
            TotalPriceTextBlock.Text = $"${FlightOffer.Price}";
        }

        private async void ProceedToPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // Replace with your API's base address and endpoint
            string apiUrl = "https://localhost:7186/api/payment/create-checkout-session";

            // Prepare the request data
            var requestData = new
            {
                Amount = FlightOffer.Price.ToString()
            };

            // Use HttpClient to call the API
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    // Make a POST request to the API
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JObject.Parse(responseContent);

                        // Extract the sessionId (Stripe Checkout URL)
                        string checkoutUrl = jsonResponse["session"].ToString();

                        // Ensure the WebView2 is initialized before navigating
                        if (PaymentWebView.CoreWebView2 != null)
                        {
                            // Navigate to the Stripe Checkout URL on the UI thread
                            PaymentWebView.CoreWebView2.Navigate(checkoutUrl);
                        }
                        else
                        {
                            // Initialize WebView2 if not already initialized
                            await PaymentWebView.EnsureCoreWebView2Async(null);

                            // Navigate to the Stripe Checkout URL on the UI thread
                            PaymentWebView.Dispatcher.Invoke(() =>
                            {
                                PaymentWebView.CoreWebView2.Navigate(checkoutUrl);
                            });
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private async void PaymentWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Check if the URL contains the word "success" to confirm a successful payment
            string currentUrl = PaymentWebView.CoreWebView2.Source;

            if (currentUrl.Contains("success", StringComparison.OrdinalIgnoreCase))
            {
                // 1. Retrieve the UserID (this should come from authentication logic)
                int userId = GetAuthenticatedUserId();

                // 2. Prepare the booking data with all required fields
                var bookedFlight = new
                {
                    CustomerId = userId,
                    FlightNumber = FlightOffer.FlightNumber,
                    AirlineName = FlightOffer.AirlineName?.Trim(), // Ensure no extra spaces
                    Origin = FlightOffer.Origin?.Trim(),
                    Destination = FlightOffer.Destination?.Trim(),
                    DepartureDateTime = DateTime.ParseExact(FlightOffer.DepartureTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    ArrivalDateTime = DateTime.ParseExact(FlightOffer.ArrivalTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    Price = decimal.Parse(FlightOffer.Price, CultureInfo.InvariantCulture),
                    NumberOfTickets = FlightOffer.NumberOfTickets,
                    ClassType = FlightOffer.ClassType?.Trim(),
                    BookingDate = DateTime.UtcNow.ToString("o") // ISO 8601 format

                };
                MessageBox.Show($"DepartureTime: {FlightOffer.DepartureTime}, ArrivalTime: {FlightOffer.ArrivalTime}");

                MessageBox.Show(Newtonsoft.Json.JsonConvert.SerializeObject(bookedFlight));

                // 3. Call your API to save the booking
                string apiUrl = "https://localhost:7186/api/BookedFlight/AddBookedFlight";

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        var jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(bookedFlight);
                        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Booking successful!");

                            // Navigate to the main page after a short delay
                            NavigateToMainPage();
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Failed to save booking: {errorContent}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }

                // Optional: Add a delay before closing the payment page
                await Task.Delay(500);
                ClosePaymentPage();
            }
        }


        private int GetAuthenticatedUserId()
        {
            // Check if UserId exists in Application.Current.Properties
            if (Application.Current.Properties.Contains("UserId"))
            {
                return (int)Application.Current.Properties["UserId"];
            }
            else
            {
                MessageBox.Show("User not authenticated. Please log in.");
                return -1; // Return an invalid value or handle appropriately
            }

        }

        private void NavigateToMainPage()
        {
            // Ensure you're on the UI thread before navigating
            if (NavigationService != null)
            {
                // Navigate to the MainPageView
                NavigationService.Navigate(new MainPageView());
            }
        }

        private void ClosePaymentPage()
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }
    }
}
