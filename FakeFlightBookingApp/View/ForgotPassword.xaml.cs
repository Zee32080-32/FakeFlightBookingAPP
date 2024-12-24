using FakeFlightBookingAPI.Models;
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
using MessageBox = System.Windows.MessageBox;

namespace FakeFlightBookingApp.View
{
    /// <summary>
    /// Interaction logic for ForgotPassword.xaml
    /// </summary>
    public partial class ForgotPassword : Page
    {
        private readonly HttpClient _httpClient;

        public ForgotPassword()
        {
            InitializeComponent();

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7186/api/users/")
            };
        }

        private async void SendResetCode_Button_Click(object sender, RoutedEventArgs e)
        {
            var email = SendResetCode_TextBox.Text.Trim();
            var code = "";
            var NewPassword = "";

            var forgotPassword = new SharedModels.ForgotPassword_Poco
            {
                Email = email,
                newPassword = NewPassword,
                code = code
            };

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email address.");
                return;
            }



            var response = await _httpClient.PostAsJsonAsync("Send-Reset-Password-Email", forgotPassword);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("If this email is registered, you will receive a password reset link.");
                // Show the fields for the verification code and new password
                CodeVerify_Label.Visibility = Visibility.Visible;
                CodeVerify_TextBox.Visibility = Visibility.Visible;

                ChangePassword_Label.Visibility = Visibility.Visible;
                ChangePassword_TextBox.Visibility = Visibility.Visible;
                
                RetypePassword_Label.Visibility = Visibility.Visible;
                RetypePassword_TextBox.Visibility = Visibility.Visible;

                SaveNewPassword_Button.Visibility = Visibility.Visible;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error: {errorMessage}");
            }

        }

        private async void CodeVerify_Button_Click(object sender, RoutedEventArgs e)
        {
            var code = CodeVerify_TextBox.Text.Trim();
            var newPassword = ChangePassword_TextBox.Text.Trim();
            var repeatPassword = RetypePassword_TextBox.Text.Trim();

            // Validate input fields
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(repeatPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (newPassword != repeatPassword)
            {
                MessageBox.Show("New passwords do not match.");
                return;
            }

            // Create the ForgotPassword_Poco object
            var forgotPasswordData = new SharedModels.ForgotPassword_Poco
            {
                Email = SendResetCode_TextBox.Text.Trim(), // Use the same email
                code = code,
                newPassword = newPassword
            };

            // Send request to the API to reset the password
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7186/api/users/verify-code-and-change-password", forgotPasswordData);

            // Handle the response
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Password reset successfully.");
                // Optionally, clear fields or navigate to login page
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error: {errorMessage}");
            }
        }


        private async void SaveNewPassword_Button_Click(object sender, RoutedEventArgs e)
        {
            var code = CodeVerify_TextBox.Text.Trim();
            var newPassword = ChangePassword_TextBox.Text.Trim();
            var repeatPassword = RetypePassword_TextBox.Text.Trim();

            // Validate input fields
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(repeatPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (newPassword != repeatPassword)
            {
                MessageBox.Show("New passwords do not match.");
                return;
            }

            // Create the ForgotPassword_Poco object
            var forgotPasswordData = new SharedModels.ForgotPassword_Poco
            {
                Email = SendResetCode_TextBox.Text.Trim(), // Use the same email
                code = code,
                newPassword = newPassword
            };

            // Send request to the API to reset the password
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7186/api/users/verify-code-and-change-password", forgotPasswordData);

            // Handle the response
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Password reset successfully.");
                // Optionally, clear fields or navigate to login page
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error: {errorMessage}");
            }
        }
    }

}
