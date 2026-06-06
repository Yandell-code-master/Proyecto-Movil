using API_BigFOOD.DTOs;
using API_BigFOOD.Models;

namespace API_BigFOOD.Services
{
    public interface IAuthorizationServices
    {
        AuthorizationResponse DevolverToken(LoginDTO authorization);
    }
}
