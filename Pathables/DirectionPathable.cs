using Blish_HUD.Pathing.Format;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Microsoft.Xna.Framework;

namespace temp.Timers.Pathables {
    public sealed class DirectionPathable : LoadedTrailPathable {

        private const float DEFAULT_TRAILSCALE = 1f;
        private const float DEFAULT_ANIMATIONSPEED = 0.5f;

        private Vector3 _destination;
        private DirectionTrailSection _trailSection;

        public float FadeNear {
            get => this.ManagedEntity.FadeNear;
            set => this.ManagedEntity.FadeNear = value;
        }

        public float FadeFar {
            get => this.ManagedEntity.FadeFar;
            set => this.ManagedEntity.FadeFar = value;
        }

        public Vector3 Destination {
            get => this._destination;
            set => this._destination = value;
        }

        private readonly PathableAttributeCollection _sourceAttributes;

        public DirectionPathable(PathableAttributeCollection sourceAttributes, PathableResourceManager pathableResourceManager, Vector3 destination) : base(pathableResourceManager) {
            _sourceAttributes = sourceAttributes;
            _destination = destination;
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
            RegisterAttribute("fadenear", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.FadeNear = fOut;
                return true;
            });

            // FadeFar
            RegisterAttribute("fadefar", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.FadeFar = fOut;
                return true;
            });

            // AnimationSpeed
            RegisterAttribute("animspeed", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.AnimationSpeed = fOut;
                return true;
            });

            // TrailScale
            RegisterAttribute("trailscale", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.Scale = fOut;
                return true;
            });

        }

        public void Activate() {
            _trailSection.Active = true;
        }

        public void Deactivate() {
            _trailSection.Active = false;
        }

        protected override bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {

            // Add destination
            List<Vector3> t = new List<Vector3>();
            t.Add(GameService.Gw2Mumble.PlayerCharacter.Position);
            t.Add(_destination);

            _trailSection = new DirectionTrailSection(t);
            this.ManagedEntity.AddSection(_trailSection);

            // Finalize attributes
            if (attributeLoaders.ContainsKey("trailscale")) {
                if (!attributeLoaders["trailscale"].Loaded) {
                    this.Scale = DEFAULT_TRAILSCALE;
                }
            }

            if (attributeLoaders.ContainsKey("animspeed")) {
                if (!attributeLoaders["animspeed"].Loaded) {
                    this.ManagedEntity.AnimationSpeed = DEFAULT_ANIMATIONSPEED;
                }
            }

            // Let base finalize attributes
            return base.FinalizeAttributes(attributeLoaders);
        }

    }
}
