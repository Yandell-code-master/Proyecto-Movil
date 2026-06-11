using System.ComponentModel.DataAnnotations;

namespace API_Security.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Date { get; set; }
        public string Roll { get; set; }
        public bool Status { get; set; }
    }
}
