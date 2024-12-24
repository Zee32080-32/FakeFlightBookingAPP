using FakeFlightBookingApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FakeFlightBookingApp.View
{
    /// <summary>
    /// Interaction logic for FlightTicketPage.xaml
    /// </summary>
    public partial class FlightTicketPage : Page
    {
        public FlightTicketPage(IEnumerable<FlightOfferDTO> flightOffers, string searchQuery)
        {
            InitializeComponent();

            // Display the search query details
            SearchQueryTextBlock.Text = searchQuery;

            // Set the ItemsSource of the ListView to the received flight offers
            FlightOffersListView.ItemsSource = flightOffers;
        }

        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFlight = ((Button)sender).DataContext as FlightOfferDTO;

            if (selectedFlight != null)
            {
                // Navigate to the payment page, passing the selected flight
                var paymentPage = new PaymentPage(selectedFlight);
                NavigationService?.Navigate(paymentPage);
            }
        }
    }
}
