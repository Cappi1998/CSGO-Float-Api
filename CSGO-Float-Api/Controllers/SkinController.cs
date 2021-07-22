using CSGO_Float_Api.Database.Repositories;
using CSGO_Float_Api.Models;
using CSGO_Float_Api.Models.Api;
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
        public ApiRequestResponse RequestFloat(string InspectLink)
        {
            ApiRequestResponse response = new ApiRequestResponse();

            string[] ParamsArray = InspectLink.Split('S', 'A', 'D', 'M');
            if (ParamsArray.Length != 4) 
            {
                response.Sucess = false;
                response.ErrorMessage = "Inspect link format invalid.";
                return response;
            }

            Skin skin = CreateSkinModel.Create(InspectLink);
            Skin skinDB = _skinRepository.Get(skin.param_a);

            FloatRequest floatRequest = new FloatRequest { Skins = new List<Skin>() };

            if (skinDB != null) floatRequest.Skins.Add(skinDB); else floatRequest.Skins.Add(skin); Server.AddSkinToQueue(skin);

            if (floatRequest.Skins.Count == 0)
            {
                response.Sucess = false;
                response.ErrorMessage = "No inspect link found!";
                response.FailedCount = 1;
                return response;
            }
            else
            {
                _floatRequestRepository.Add(floatRequest);
                response.Sucess = true;
                response.SucessCount = 1;
                response.RequestID = floatRequest.ID;
                return response;
            }
        }

        [HttpPost]
        [Route("RequestFloats")]
        public ApiRequestResponse RequestFloats(LinkLists linkLists)
        {
            ApiRequestResponse response = new ApiRequestResponse();

            if (linkLists == null || linkLists.InspectLinks.Count == 0)
            {
                response.Sucess = false;
                response.ErrorMessage = "Invalid Format!";
                return response;
            }

            int InvalidCount = 0;
            int SucessCount = 0;

            FloatRequest floatRequest = new FloatRequest { Skins = new List<Skin>() };

            linkLists.InspectLinks.ForEach(a =>
            {
                string[] ParamsArray = a.Split('S', 'A', 'D', 'M');
                if (ParamsArray.Length != 4)
                {
                    InvalidCount++;
                    return;
                }

                Skin skin = CreateSkinModel.Create(a);

                if (skin == null)
                {
                    InvalidCount++;
                    return;
                }
                else SucessCount++;

                Skin skinDB = _skinRepository.Get(skin.param_a);

                if (skinDB != null) floatRequest.Skins.Add(skinDB); else floatRequest.Skins.Add(skin); Server.AddSkinToQueue(skin);
            });

            if(floatRequest.Skins.Count == 0)
            {
                response.Sucess = false;
                response.ErrorMessage = "White list, no inspect link found!";
                return response;
            }
            else
            {
                _floatRequestRepository.Add(floatRequest);
                response.Sucess = true;
                response.SucessCount = SucessCount;
                response.FailedCount = InvalidCount;
                response.RequestID = floatRequest.ID;
                return response;
            }
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
