using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSGO_Float_Api.Models
{
    public class FloatRequest
    {
        [Key]
        public int ID { get; set; }
        public List<Skin> Skins { get; set; }
    }
}
