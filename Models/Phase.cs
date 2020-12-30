using System;
using System.Collections.Generic;
using Blish_HUD.Pathing.Content;
using temp.Timers.Controls;

namespace temp.Timers.Models {
    public class Phase : IDisposable {
        public string name { get; set; }
        public StartTrigger start { get; set; }
        public EndTrigger finish { get; set; }
        public List<AlertType> alerts { get; set; }
        public List<Direction> directions { get; set; }
        public List<Marker> markers { get; set; }
        // Non-serialized
        private List<Alert> _alerts { get; set; }
        public string Init(PathableResourceManager pathableResourceManager) {

            // Validation
            if (start == null) return "phase missing start trigger";
            string message = start.Init();
            if (message != null) return message;
            if (finish != null) {
                message = finish.Init();
                if (message != null) return message;
            }

            // Validation & Initialization
            this._alerts = new List<Alert>();

            foreach (AlertType type in this.alerts) {
                message = type.Init();
                if (message != null) return message;
                foreach (float time in type.timestamps) {
                    this._alerts.Add(new Alert {
                        time = time,
                        source = type
                    });
                }
            }

            if (this.directions == null)
                this.directions = new List<Direction>();

            foreach (Direction dir in this.directions) {
                message = dir.Init(pathableResourceManager);
                if (message != null) return message;
            }

            if (this.markers == null)
                this.markers = new List<Marker>();

            foreach (Marker mark in this.markers) {
                message = mark.Init(pathableResourceManager);
                if (message != null) return message;
            }

            return null;
        }
        public void Activate() {
            foreach (Direction dir in this.directions) {
                dir.Activate();
            }
            foreach (Marker mark in this.markers) {
                mark.Activate();
            }
        }
        public void Deactivate() {
            foreach (Direction dir in this.directions) {
                dir.Deactivate();
            }
            foreach (Marker mark in this.markers) {
                mark.Deactivate();
            }
        }
        public void Update(AlertContainer parent, float elapsedTime) {
            foreach (Alert al in this._alerts) {
                al.Update(parent, elapsedTime);
            }
            foreach (Direction dir in this.directions) {
                dir.Update(elapsedTime);
            }
            foreach (Marker mark in this.markers) {
                mark.Update(elapsedTime);
            }
        }
        public void Stop() {
            foreach (Alert al in this._alerts) {
                al.Stop();
            }
            foreach (Direction dir in this.directions) {
                dir.Stop();
            }
            foreach (Marker mark in this.markers) {
                mark.Stop();
            }
        }
        public void Dispose() {
            this.Deactivate();
            this.Stop();
        }
    }

}
