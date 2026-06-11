namespace API_Security.Services
{
    public interface ITokenService
    {
        public string DevolverToken(string email);
    }
}
