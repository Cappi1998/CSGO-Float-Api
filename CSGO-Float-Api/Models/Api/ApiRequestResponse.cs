namespace CSGO_Float_Api.Models.Api
{
    public class ApiRequestResponse
    {
        public bool Sucess { get; set; }
        public int RequestID { get; set; }
        public int SucessCount { get; set; }
        public int FailedCount { get; set; }
        public string ErrorMessage { get; set; }
    }
}
