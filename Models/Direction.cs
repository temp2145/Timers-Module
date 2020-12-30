using System.Collections.Generic;
using System.Text.Json.Serialization;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Microsoft.Xna.Framework;
using temp.Timers.Pathables;

namespace temp.Timers.Models {
    public class Direction {

        // Serialized properties
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unnamed Direction";
        [JsonPropertyName("position")]
        public List<float> Position { get; set; }
        [JsonPropertyName("duration")]
        public float Duration { get; set; } = 10f;
        [JsonPropertyName("alpha")]
        public float Alpha { get; set; } = 0.8f;
        [JsonPropertyName("speed")]
        public float AnimSpeed { get; set; } = 1f;
        [JsonPropertyName("texture")]
        public string TextureString { get; set; }
        [JsonPropertyName("timestamps")]
        public List<float> Timestamps { get; set; }

        // Private members
        private PathableAttributeCollection _attributes;
        private DirectionPathable _pathable { get; set; }

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
            attr.Add(new PathableAttribute("texture", TextureString));
            attr.Add(new PathableAttribute("alpha", Alpha.ToString()));
            attr.Add(new PathableAttribute("animSpeed", AnimSpeed.ToString()));
            _attributes = new PathableAttributeCollection(attr);

            _pathable = new DirectionPathable(
                    _attributes,
                    pathableResourceManager,
                    new Vector3(Position[0], Position[1], Position[2])
                );

            if (!_pathable.SuccessfullyLoaded)
                return Name + " direction loading unsuccessful";

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
