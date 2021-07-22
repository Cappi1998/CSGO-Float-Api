using System.ComponentModel.DataAnnotations;

namespace CSGO_Float_Api.Models
{
    public class Skin
    {
        public ulong param_s { get; set; }
        public ulong param_m { get; set; }
        [Key]
        public ulong param_a { get; set; }//AssetID
        public ulong param_d { get; set; }
        public float Float { get; set; }
        public int Pattern { get; set; }
    }
}
