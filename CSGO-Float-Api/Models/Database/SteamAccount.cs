using System.ComponentModel.DataAnnotations;

namespace CSGO_Float_Api.Models
{
    public class SteamAccount
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
        public string Shared_secret { get; set; }
        public string LoginKey { get; set; }
        public string SentryFileBase64 { get; set; }
    }
}