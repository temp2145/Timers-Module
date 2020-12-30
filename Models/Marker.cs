using System.Collections.Generic;
using System.Text.Json.Serialization;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using temp.Timers.Pathables;

namespace temp.Timers.Models {

    public class Marker {

        // Serialized Properties
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unnamed Marker";
        [JsonPropertyName("position")]
        public List<float> Position { get; set; }
        [JsonPropertyName("rotation")]
        public List<float> Rotation { get; set; }
        [JsonPropertyName("duration")]
        public float Duration { get; set; } = 10f;
        [JsonPropertyName("alpha")]
        public float Alpha { get; set; } = 0.8f;
        [JsonPropertyName("size")]
        public float Size { get; set; } = 1.0f;
        [JsonPropertyName("texture")]
        public string TextureString { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("timestamps")]
        public List<float> Timestamps { get; set; }


        // Non-serialized
        private PathableAttributeCollection _attributes;
        private MarkerPathable _pathable { get; set; }

        // Methods
        public string Init (PathableResourceManager pathableResourceManager) {

            if (Position?.Count != 3)
                return Name + " invalid position property";
            if (Timestamps?.Count == 0)
                return Name + " invalid timestamps property";
            if (string.IsNullOrEmpty(TextureString))
                return Name + " invalid texture property";

            List<PathableAttribute> attr = new List<PathableAttribute>();
            attr.Add(new PathableAttribute("fadeNear", "0"));
            attr.Add(new PathableAttribute("fadeFar", "10000"));
            attr.Add(new PathableAttribute("xPos", Position[0].ToString()));
            attr.Add(new PathableAttribute("zPos", Position[1].ToString()));
            attr.Add(new PathableAttribute("yPos", Position[2].ToString()));
            if (Rotation?.Count == 3) {
                attr.Add(new PathableAttribute("rotate-x", Rotation[0].ToString()));
                attr.Add(new PathableAttribute("rotate-y", Rotation[1].ToString()));
                attr.Add(new PathableAttribute("rotate-z", Rotation[2].ToString()));
            }
            attr.Add(new PathableAttribute("alpha", Alpha.ToString()));
            attr.Add(new PathableAttribute("iconSize", Size.ToString()));
            attr.Add(new PathableAttribute("iconFile", TextureString));
            if (!string.IsNullOrEmpty(Text))
                attr.Add(new PathableAttribute("titleText", Text));
            _attributes = new PathableAttributeCollection(attr);

            _pathable = new MarkerPathable(_attributes, pathableResourceManager);

            if (!_pathable.SuccessfullyLoaded)
                return Name + " marker loading unsuccessful";

            _pathable.ManagedEntity.Visible = false;

            return null;
        }
        public void Activate() {
            if (_pathable != null) {
                _pathable.Active = true;
                GameService.Pathing.RegisterPathable(_pathable);
            }
        }
        public void Deactivate() {
            if (_pathable != null) {
                _pathable.Active = false;
                GameService.Pathing.UnregisterPathable(_pathable);
            }
        }
        public void Stop() {
            if (_pathable != null) 
                _pathable.ManagedEntity.Visible = false;
        }
        public void Update(float elapsedTime) {

            if (_pathable == null) return;

            bool enabled = false;
            foreach (float time in Timestamps) {
                if (elapsedTime >= time && elapsedTime < time + Duration) {
                    enabled = true;
                    _pathable.ManagedEntity.Visible = true;
                }
            }
            if (!enabled) Stop();
        }
    }
}
