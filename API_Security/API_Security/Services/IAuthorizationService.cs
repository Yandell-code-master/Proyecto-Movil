namespace API_Security.Services
{
    public interface IAuthorizationService
    {
        public string DevolverToken(string email);
    }
}
