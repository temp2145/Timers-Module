using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;

namespace temp.Timers {
    public class Resources {

        public readonly Effect MasterScrollEffect;
        private readonly string[,] icons = {
            { "raid", "9F5C23543CB8C715B7022635C10AA6D5011E74B3/1302679" },
            { "boss", "7554DCAF5A1EA1BDF5297352A203AF2357BE2B5B/498983" },
            { "onepath", "1B3B7103E5FFFEB4B94137CFEF6AC5A528AE1BA8/1730771" },
            { "slayer", "E00460A2CAD85D47406EAB4213D1010B3E80C9B0/42675" },
            { "hero", "CD94B9A33CD82E9C7BBE59ADB051CE7CE00929AC/42679" },
            { "weaponmaster", "E57F44931D5D1C0DEB16A27803A4744492B834E2/42682" },
            { "community", "AED92D932A30A6990F0B5C35073AEB4C4556E2F3/42681" },
            { "daybreak", "056C32B97B15F04E2BD6A660FC451946ED086040/1895981" },
            { "noquarter", "2D305E1F34A7985BF572D40717062096E3BD58BA/2293615" },
            { "transferchaser", "203A4F05DD7DF36A4DEBF9B4D9DE90AEC8A7155A/1769806" },
            { "conservation", "21B767B52BC1C40F0698B1D9C77EDDDD22E6B46D/1769807" },
            { "winter", "2A2DA0B946A85A0DB5D59E0703796C26AB4D650D/1914854" },
            { "foefire", "CFAAC3D0D89BF997BD55FF647F1E546CACFCD795/1635137" },
            { "djinn", "3504199B06996B43237C0ACF10084950716DCF38/2063558" },
            { "pvp", "7F4E2835316DE912B1493CCF500A9D5CF4A83B4A/42676" },
            { "wvw", "2BBA251A24A2C1A0A305D561580449AF5B55F54F/338457" },
            { "event", "C2E37DE77D0C024B06F1E0A5F738524A07E9CF2B/797625" },
            { "dungeon", "37A6BBC111E4EF34CDFF93314A26992A4858EF14/602776" },
            { "fractal", "9A6791950A5F3EBD15C91C2942F1E3C8D5221B28/602779" }
        };
        private readonly Dictionary<string, AsyncTexture2D> _iconFiles;
        private const string DEFAULT_ICON = "raid";

        public Resources () {
            MasterScrollEffect = ContentService.Content.ContentManager.Load<Effect>(@"effects\menuitem");
            MasterScrollEffect.Parameters["Mask"].SetValue(GameService.Content.GetTexture("156072"));
            MasterScrollEffect.Parameters["Overlay"].SetValue(GameService.Content.GetTexture("156071"));

            _iconFiles = new Dictionary<string, AsyncTexture2D>();
        }

        public AsyncTexture2D getIcon (string name, string unset = DEFAULT_ICON) {
            if (name == "null") return null;
            if (unset == "" || unset == null) unset = DEFAULT_ICON;
            if (name == "" || name == null) name = unset;
            else name = name.Trim().ToLower();

            AsyncTexture2D value;
            if (_iconFiles.TryGetValue(name, out value)) {
                return value;
            } else {
                for (int i = 0; i < icons.Length; i++) {
                    if (icons[i, 0] == name) {
                        value = GameService.Content.GetRenderServiceTexture(icons[i, 1]);
                        _iconFiles.Add(name, value);
                        return value;
                    }
                }
                return getIcon(DEFAULT_ICON);
            }
        }

        public void Unload() {
            // EXCEPTION: "The GraphicsDevice must not be null when creating new resources."
            // MasterScrollEffect.Dispose();
            foreach (AsyncTexture2D icon in _iconFiles.Values) {
                icon.Dispose();
            }
            _iconFiles.Clear();
        }

    }
}
