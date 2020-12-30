using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Blish_HUD.Pathing.Content;
using temp.Timers.Controls;

namespace temp.Timers.Models {
    public class Phase : IDisposable {

        // Serialized Properties
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unnamed Phase";
        [JsonPropertyName("start")]
        public StartTrigger Start { get; set; }
        [JsonPropertyName("finish")]
        public EndTrigger Finish { get; set; }
        [JsonPropertyName("alerts")]
        public List<AlertType> AlertTypes { get; set; }
        [JsonPropertyName("directions")]
        public List<Direction> Directions { get; set; } = new List<Direction>();
        [JsonPropertyName("markers")]
        public List<Marker> Markers { get; set; } = new List<Marker>();

        // Private members
        private List<Alert> _alerts { get; set; } = new List<Alert>();

        // Methods
        public string Init(PathableResourceManager pathableResourceManager) {

            // Validation
            if (Start == null) return "phase missing start trigger";

            string message = Start.Init();
            if (message != null) return message;

            if (Finish != null) {
                message = Finish.Init();
                if (message != null) return message;
            }

            // Validation & Initialization
            foreach (AlertType type in AlertTypes) {
                message = type.Init(pathableResourceManager);
                if (message != null) return message;
                foreach (float time in type.Timestamps) {
                    _alerts.Add(new Alert {
                        Time = time,
                        Source = type
                    });
                }
            }

            foreach (Direction dir in Directions) {
                message = dir.Init(pathableResourceManager);
                if (message != null) return message;
            }

            foreach (Marker mark in Markers) {
                message = mark.Init(pathableResourceManager);
                if (message != null) return message;
            }

            return null;
        }
        public void Activate() {
            Directions.ForEach(dir => dir.Activate());
            Markers.ForEach(mark => mark.Activate());
        }
        public void Deactivate() {
            _alerts.ForEach(al => al.Deactivate());
            Directions.ForEach(dir => dir.Deactivate());
            Markers.ForEach(mark => mark.Deactivate());
        }
        public void Update(AlertContainer parent, float elapsedTime) {
            _alerts.ForEach(al => al.Update(parent, elapsedTime));
            Directions.ForEach(dir => dir.Update(elapsedTime));
            Markers.ForEach(mark => mark.Update(elapsedTime));
        }
        public void Stop() {
            _alerts.ForEach(al => al.Stop());
            Directions.ForEach(dir => dir.Stop());
            Markers.ForEach(mark => mark.Stop());
        }
        public void Dispose() {
            this.Stop();
            this.Deactivate();
            AlertTypes.ForEach(at => at.Dispose());
        }
    }

}
