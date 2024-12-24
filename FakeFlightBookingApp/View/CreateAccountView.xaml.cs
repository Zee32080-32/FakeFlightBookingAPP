//using Microsoft.VisualBasic.ApplicationServices;
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
using SharedModels;
using System.Net.Http; // Include for HttpClient
using Newtonsoft.Json;
using System.Net.Http.Json; // Include for Json serialization


namespace FakeFlightBookingApp.View
{
    public partial class CreateAccountView : Page
    {
        private readonly HttpClient _httpClient;

        public CreateAccountView()
        {
            InitializeComponent();
            _httpClient = new HttpClient 
            {
                BaseAddress = new Uri("https://localhost:7186/api/users/") 
            }; 

        }

        private async void createAccount_Button_Click(object sender, RoutedEventArgs e)
        {
            string firstName = firstName_textBox.Text;
            string lastName = lastName_TextBox.Text;
            string userName = userName_TextBox.Text;
            string email = email_TextBox.Text;
            string password = password_PasswordBox.Password;
            string phoneNumber = phoneNumber_TextBox.Text;

            // Create a new customer with all required parameters
            var newCustomer = new SharedModels.Customer(firstName, lastName, userName, email, password, phoneNumber);


            var response = await _httpClient.PostAsJsonAsync("initiate-registration", newCustomer);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Verification email sent. Please check your email.");
                verificationCode_Label.Visibility = Visibility.Visible;
                verificationCode_TextBox.Visibility = Visibility.Visible;
                verifyEmail_Button.Visibility = Visibility.Visible;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error creating account: {errorMessage}");
            }
        }
        private async void verifyEmail_Button_Click(object sender, RoutedEventArgs e)
        {
            var verificationCode = verificationCode_TextBox.Text;

            var model = new FakeFlightBookingAPI.Models.EmailVerification
            {
                Email = email_TextBox.Text,
                Code = verificationCode
            };

            var response = await _httpClient.PostAsJsonAsync("verify-email-and-create-account", model);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Email successfully verified, and account created.");
                // Navigate to the login page or continue further
            }
            else
            {
                MessageBox.Show("Invalid verification code.");
            }
        }

        private async Task<HttpResponseMessage> CreateCustomerAsync(Customer newCustomer)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7186/api/users/"); 

                // Serialize customer object to JSON
                var json = JsonConvert.SerializeObject(newCustomer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send POST request
                var response = await client.PostAsync("", content);
                return response;
            }
        }
    }
}
