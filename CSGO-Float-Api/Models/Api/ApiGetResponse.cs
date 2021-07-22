using System.Collections.Generic;

namespace CSGO_Float_Api.Models.Api
{
    public class ApiGetResponse
    {
        public bool Sucess { get; set; }
        public string ErrorMessage { get; set; }
        public List<SkinResponse> ResponseList { get; set; }
    }

    public class SkinResponse
    {
        public ulong AssetID { get; set; }
        public float Float { get; set; }
        public uint Pattern { get; set; }
        public string errorMessage { get; set; }
    }
}
