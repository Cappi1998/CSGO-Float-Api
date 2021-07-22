using CSGO_Float_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSGO_Float_Api
{
    public class CreateSkinModel
    {
        public static Skin Create(string InspectURL)
        {
            
            if (InspectURL.Contains("A"))
            {
                string[] ParamsArray = InspectURL.Split('S', 'A', 'D', 'M');

                if (ParamsArray.Length != 4)
                {
                    return null;
                }

                Skin SkinRequested = new Skin();
                try
                {
                    SkinRequested.param_s = ulong.Parse(ParamsArray[1]);
                    SkinRequested.param_a = ulong.Parse(ParamsArray[2]);//AssetID
                    SkinRequested.param_d = ulong.Parse(ParamsArray[3]);
                    SkinRequested.param_m = 0;
                    return SkinRequested;
                }
                catch (FormatException)
                {
                    return null;
                }
            }
            else
            {
                //TODO -- ???
            }
            return null;
        }
    }
}
