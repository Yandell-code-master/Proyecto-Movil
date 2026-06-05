namespace AplicacionMovil.Services;

public class MockAuthService
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        await Task.Delay(800);
        return username == "admin" && password == "admin123";
    }
}
