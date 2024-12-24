using FakeFlightBookingApp.Model;
using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace FakeFlightBookingApp.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Page
    {
        private readonly HttpClient _httpClient;

        public LoginView()
        {
            InitializeComponent();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7186/api/users/")
            };

        }

        private async void Signin_Button_Click(object sender, RoutedEventArgs e)
        {
            string username = UserName_textBox.Text;
            string password = Password_PasswordBox.Password;

            // Create a new LoginRequest object
            var loginRequest = new FakeFlightBookingAPI.Models.LoginRequest
            {
                UserName = username,
                Password = password
            };

            // Send the login request to the API
            await LoginUser(loginRequest);

        }

        private async Task LoginUser(FakeFlightBookingAPI.Models.LoginRequest loginRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("CustomerLogin", loginRequest);

                // Log raw response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // Try to deserialize the customer data
                    var customer = await response.Content.ReadFromJsonAsync<CustomerDTO>();

                    if (customer != null)
                    {
                        ClearUserData();

                        // Store the user details in Application.Current.Properties
                        Application.Current.Properties["UserId"] = customer.Id;
                        Application.Current.Properties["UserName"] = customer.UserName;
                        Application.Current.Properties["FirstName"] = customer.FirstName;
                        Application.Current.Properties["LastName"] = customer.LastName;
                        Application.Current.Properties["Email"] = customer.Email;

                        LogCurrentProperties();
                        // Show success message
                        MessageBox.Show("Login successful.");
                        NavigateToMainPage();
                    }
                    else
                    {
                        MessageBox.Show("Error: Invalid response data.");
                        LogCurrentProperties();
                    }
                }
                else
                {
                    Console.WriteLine($"Response: {response.StatusCode}, Content: {responseContent}");
                    MessageBox.Show("Invalid username or password.");
                    LogCurrentProperties();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                MessageBox.Show("An error occurred while logging in. Please try again.");
                LogCurrentProperties();
            }
        }

        private void Reset_Password_Button_Click(object sender, RoutedEventArgs e)
        {

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

        public void ClearUserData()
        {
            Application.Current.Properties.Clear(); // This will clear all properties
        }

        private void LogCurrentProperties()
        {
            foreach (var key in Application.Current.Properties.Keys)
            {
                Console.WriteLine($"{key}: {Application.Current.Properties[key]}");
            }
        }
    }
}
