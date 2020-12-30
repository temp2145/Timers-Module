using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Blish_HUD.Pathing.Format;
using Microsoft.Xna.Framework;

namespace temp.Timers.Pathables {
    public sealed class MarkerPathable : LoadedMarkerPathable {

        private const float DEFAULT_ICONSIZE = 1f;

        private readonly PathableAttributeCollection _sourceAttributes;

        public MarkerPathable(PathableAttributeCollection sourceAttributes, PathableResourceManager packContext) : base(packContext) {
            _sourceAttributes = sourceAttributes;
            this.MapId = -1;
            BeginLoad();
        }

        protected override void BeginLoad() {
            LoadAttributes(_sourceAttributes);
        }

        protected override void PrepareAttributes() {
            base.PrepareAttributes();

            // Alpha (alias:Opacity)
            RegisterAttribute("alpha", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.Opacity = fOut;
                return true;
            });

            // FadeNear
            RegisterAttribute("fadeNear", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.FadeNear = fOut;
                return true;
            });

            // FadeFar
            RegisterAttribute("fadeFar", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.FadeFar = fOut;
                return true;
            });

            RegisterAttribute("titleText", delegate (PathableAttribute attribute) {
                if (!string.IsNullOrEmpty(attribute.Value)) {
                    this.ManagedEntity.BasicTitleText = attribute.Value.Trim();

                    return true;
                }
                return false;
            });


            // IconSize
            RegisterAttribute("iconSize", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.AutoResize = false;
                this.ManagedEntity.Size = new Vector2(fOut * 2f);
                return true;
            });

        }

        protected override bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {

            // Finalize attributes
            if (attributeLoaders.ContainsKey("iconsize")) {
                if (!attributeLoaders["iconsize"].Loaded) {
                    this.ManagedEntity.Size = new Vector2(DEFAULT_ICONSIZE);
                }
            }

            // Let base finalize attributes
            return base.FinalizeAttributes(attributeLoaders);
        }

    }
}
