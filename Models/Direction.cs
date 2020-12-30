using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Microsoft.Xna.Framework;
using temp.Timers.Pathables;

namespace temp.Timers.Models {
    public class Direction {
        public string name { get; set; }
        public List<float> destination { get; set; }
        public float duration { get; set; }
        public float alpha { get; set; }
        public float animSpeed { get; set; }
        public string texture { get; set; }
        public List<float> timestamps { get; set; }
        // Non-serialized
        private PathableAttributeCollection _attributes;
        private DirectionPathable _pathable { get; set; }
        public string Init(PathableResourceManager pathableResourceManager) {

            if (this.duration == 0)
                return this.name + " could not read duration";
            if (this.destination?.Count != 3)
                return this.name + " could not read destination";
            if (this.timestamps?.Count == 0)
                return this.name + " could not read timestamps";
            if (this.texture == "")
                return this.name + " could not read texture";
            if (this.alpha == 0) this.alpha = 0.8f;

            List<PathableAttribute> attr = new List<PathableAttribute>();
            attr.Add(new PathableAttribute("fadeNear", "0"));
            attr.Add(new PathableAttribute("fadeFar", "10000"));
            attr.Add(new PathableAttribute("texture", this.texture));
            attr.Add(new PathableAttribute("alpha", this.alpha.ToString()));
            attr.Add(new PathableAttribute("animSpeed", this.animSpeed.ToString()));
            _attributes = new PathableAttributeCollection(attr);

            this._pathable = new DirectionPathable(
                    _attributes,
                    pathableResourceManager,
                    new Vector3(this.destination[0], this.destination[1], this.destination[2])
                );

            if (!this._pathable.SuccessfullyLoaded)
                return this.name + " could not load direction";

            this._pathable.ManagedEntity.Visible = false;

            return null;
        }
        public void Activate() {
            if (this._pathable != null) {
                this._pathable.Active = true;
                GameService.Pathing.RegisterPathable(this._pathable);
            }
        }
        public void Deactivate() {
            if (this._pathable != null) {
                this._pathable.Active = false;
                GameService.Pathing.UnregisterPathable(this._pathable);
            }
        }
        public void Stop() {
            if (this._pathable == null) return;
            this._pathable.ManagedEntity.Visible = false;
            this._pathable.Deactivate();
        }
        public void Update(float elapsedTime) {
            if (this._pathable == null) return;

            bool enabled = false;
            foreach (float time in this.timestamps) {
                if (elapsedTime >= time && elapsedTime < time + this.duration) {
                    enabled = true;
                    this._pathable.ManagedEntity.Visible = true;
                    this._pathable.Activate();
                }
            }
            if (!enabled) {
                this.Stop();
            }
        }
    }
}
