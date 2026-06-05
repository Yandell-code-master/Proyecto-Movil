namespace AplicacionMovil;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        var savedUsername = Preferences.Get("RememberedUsername", string.Empty);
        var savedPassword = Preferences.Get("RememberedPassword", string.Empty);

        if (!string.IsNullOrEmpty(savedUsername))
        {
            txtUsername.Text = savedUsername;
            txtPassword.Text = savedPassword;
            chkRemember.IsChecked = true;
        }
    }

    private void OnUsernameFocused(object? sender, FocusEventArgs e)
    {
        underlineUsername.Color = (Color)Application.Current!.Resources["Primary"];
    }

    private void OnUsernameUnfocused(object? sender, FocusEventArgs e)
    {
        underlineUsername.Color = (Color)Application.Current!.Resources["OutlineVariant"];
    }

    private void OnPasswordFocused(object? sender, FocusEventArgs e)
    {
        underlinePassword.Color = (Color)Application.Current!.Resources["Primary"];
    }

    private void OnPasswordUnfocused(object? sender, FocusEventArgs e)
    {
        underlinePassword.Color = (Color)Application.Current!.Resources["OutlineVariant"];
    }

    private void OnTogglePasswordVisibility(object? sender, TappedEventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;
        lblTogglePassword.Text = txtPassword.IsPassword ? "\U0001F441" : "\U0001F441\u200D\U0001F5E8";
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var username = txtUsername.Text?.Trim();
        var password = txtPassword.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlertAsync("Validation Error", "Please enter both username and password.", "OK");
            return;
        }

        btnLogin.IsEnabled = false;
        btnLogin.Text = "LOGGING IN...";

        try
        {
            var success = await MockLoginAsync(username, password);

            if (success)
            {
                if (chkRemember.IsChecked)
                {
                    Preferences.Set("RememberedUsername", username);
                    Preferences.Set("RememberedPassword", password);
                }
                else
                {
                    Preferences.Remove("RememberedUsername");
                    Preferences.Remove("RememberedPassword");
                }

                await DisplayAlertAsync("Welcome", $"Successfully logged in as {username}", "OK");
            }
            else
            {
                await DisplayAlertAsync("Login Failed", "Invalid username or password. Try admin / admin123", "OK");
            }
        }
        finally
        {
            btnLogin.IsEnabled = true;
            btnLogin.Text = "LOG IN";
        }
    }

    private async Task<bool> MockLoginAsync(string username, string password)
    {
        await Task.Delay(800);
        return username == "admin" && password == "admin123";
    }

    private async void OnForgotAccessTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Reset Access", "Please contact your administrator to reset your access credentials.", "OK");
    }
}
