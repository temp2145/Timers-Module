using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using temp.Timers.Pathables;

namespace temp.Timers.Models {

    public class Marker {
        public string name { get; set; }
        public List<float> position { get; set; }
        public List<float> rotation { get; set; }
        public float duration { get; set; }
        public float alpha { get; set; }
        public float size { get; set; }
        public string texture { get; set; }
        public string text { get; set; }
        public List<float> timestamps { get; set; }
        // Non-serialized
        private PathableAttributeCollection _attributes;
        private MarkerPathable _pathable { get; set; }
        public string Init(PathableResourceManager pathableResourceManager) {

            if (this.duration == 0)
                return this.name + " could not read duration";
            if (this.position?.Count != 3)
                return this.name + " could not read position";
            if (this.timestamps?.Count == 0)
                return this.name + " could not read timestamps";
            if (this.texture == "")
                return this.name + " could not read texture";
            if (this.alpha == 0) this.alpha = 0.8f;
            if (this.size == 0) this.size = 1.0f;

            List<PathableAttribute> attr = new List<PathableAttribute>();
            attr.Add(new PathableAttribute("xPos", this.position[0].ToString()));
            attr.Add(new PathableAttribute("zPos", this.position[1].ToString()));
            attr.Add(new PathableAttribute("yPos", this.position[2].ToString()));
            if (this.rotation?.Count == 3) {
                attr.Add(new PathableAttribute("rotate-x", this.rotation[0].ToString()));
                attr.Add(new PathableAttribute("rotate-y", this.rotation[1].ToString()));
                attr.Add(new PathableAttribute("rotate-z", this.rotation[2].ToString()));
            }
            attr.Add(new PathableAttribute("alpha", this.alpha.ToString()));
            attr.Add(new PathableAttribute("fadeNear", "0"));
            attr.Add(new PathableAttribute("fadeFar", "10000"));
            attr.Add(new PathableAttribute("iconSize", this.size.ToString()));
            attr.Add(new PathableAttribute("iconFile", this.texture));
            if (this.text != "" && this.text != null)
                attr.Add(new PathableAttribute("titleText", this.text));
            _attributes = new PathableAttributeCollection(attr);

            this._pathable = new MarkerPathable(_attributes, pathableResourceManager);

            if (!this._pathable.SuccessfullyLoaded)
                return this.name + " could not load marker";

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
        }
        public void Update(float elapsedTime) {
            if (this._pathable == null) return;

            // this._pathable.Refresh();

            bool enabled = false;
            foreach (float time in this.timestamps) {
                if (elapsedTime >= time && elapsedTime < time + this.duration) {
                    enabled = true;
                    this._pathable.ManagedEntity.Visible = true;
                }
            }
            if (!enabled) {
                this.Stop();
            }
        }
    }
}
