using CSGO_Float_Api.Database.Repositories;
using CSGO_Float_Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CSGO_Float_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SkinController : ControllerBase
    {
        private readonly ISkinRepository _skinRepository;
        private readonly IFloatRequestRepository _floatRequestRepository;
        public SkinController(ISkinRepository skinRepository, IFloatRequestRepository floatRequestRepository)
        {
            _skinRepository = skinRepository;
            _floatRequestRepository = floatRequestRepository;
        }

        [HttpPost]
        [Route("RequestFloat")]
        public IActionResult RequestFloat(string InspectLink)
        {
            string[] ParamsArray = InspectLink.Split('S', 'A', 'D', 'M');
            if (ParamsArray.Length != 4) return BadRequest("Inspect link format invalid.");

            Skin skin = CreateSkinModel.Create(InspectLink);
            Skin skinDB = _skinRepository.Get(skin.param_a);

            FloatRequest floatRequest = new FloatRequest { Skins = new List<Skin>() };

            if (skinDB != null) floatRequest.Skins.Add(skinDB); else floatRequest.Skins.Add(skin); Server.AddSkinToQueue(skin);

            _floatRequestRepository.Add(floatRequest);
            return Ok(floatRequest.ID);
        }

        [HttpPost]
        [Route("RequestFloats")]
        public IActionResult RequestFloats(List<string> ListInspectLink)
        {
            if (ListInspectLink == null || ListInspectLink.Count == 0) return BadRequest("Format invalid.");

            int InvalidCount = 0;
            int SucessCount = 0;

            FloatRequest floatRequest = new FloatRequest { Skins = new List<Skin>() };

            ListInspectLink.ForEach(a => 
            {
                string[] ParamsArray = a.Split('S', 'A', 'D', 'M');
                if (ParamsArray.Length != 4)
                {
                    InvalidCount++;
                    return;
                } 

                Skin skin = CreateSkinModel.Create(a);
                Skin skinDB = _skinRepository.Get(skin.param_a);

                if (skinDB != null) floatRequest.Skins.Add(skinDB); else floatRequest.Skins.Add(skin); Server.AddSkinToQueue(skin);
            });

            _floatRequestRepository.Add(floatRequest);
            return Ok(floatRequest.ID);
        }

        [HttpGet]
        [Route("GetResponse")]
        public IActionResult GetResponse(int RequestID)
        {
            FloatRequest request = _floatRequestRepository.Get(RequestID);

            if (request == null) return NotFound($"Error - No order with id {RequestID} was found.");

            List<string> Response = new List<string>();
            request.Skins.ForEach(a =>
            {
                if (a.Float != 0)
                {
                    Response.Add($"{a.param_a}:{a.Float}");
                }
                else
                {
                    Response.Add($"{a.param_a}:Error - Skin not yet verified.");
                }

            });
            return Ok(Response);
        }
    }
}
