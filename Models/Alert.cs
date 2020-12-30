using System;
using System.Collections.Generic;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using temp.Timers;
using temp.Timers.Controls;

namespace temp.Timers.Models {

    public class AlertType {
        public float warningDuration { get; set; }
        public float alertDuration { get; set; }
        public string warning { get; set; }
        public string alert { get; set; }
        public string icon { get; set; }
        public List<float> fillColor { get; set; }
        public List<float> timestamps { get; set; }
        // Non-serialized
        private const float DEFAULT_WARNING_DURATION = 15.0f;
        private const float DEFAULT_ALERT_DURATION = 5.0f;
        public Color Fill { get; set; }
        public string Init() {
            if (this.warningDuration == 0)
                this.warningDuration = DEFAULT_WARNING_DURATION;
            if (this.alertDuration == 0)
                this.alertDuration = DEFAULT_ALERT_DURATION;
            if (this.warning == null || this.warning.Length == 0)
                this.warningDuration = 0;
            if (this.alert == null || this.alert.Length == 0)
                this.alertDuration = 0;
            if (this.timestamps?.Count == 0)
                return this.warning + "/" + this.alert + " could not read timestamps";

            if (this.fillColor == null) { 
                this.Fill = Color.DarkGray;
            } else if (this.fillColor.Count == 3) {
                this.Fill = new Color(fillColor[0], fillColor[1], fillColor[2]);
            } else if (this.fillColor.Count == 4) {
                this.Fill = new Color(fillColor[0], fillColor[1], fillColor[2], fillColor[3]);
            } else {
                return this.warning + "/" + this.alert + " could not read fillcolor";
            }

            return null;
        }
    }

    public class Alert {
        public float time { get; set; }
        public AlertType source { get; set; }
        // Non-serialized
        public AlertPanel panel { get; set; }
        public void Init(FlowPanel parent) {
            this.panel = new AlertPanel {
                Parent = parent,
                Text = (this.source.warning == "") ? this.source.alert : this.source.warning,
                Icon = TimersModule.Resources.getIcon(this.source.icon, "onepath"),
                MaxFill = this.source.warningDuration,
                CurrentFill = 0.0f,
                FillColor = this.source.Fill
            };
        }
        public void Update(AlertContainer parent, float elapsedTime) {
            if (this.panel == null) {
                if (this.source.warning == "" &&
                    elapsedTime >= this.time &&
                    elapsedTime < this.time + this.source.alertDuration) {
                    // If no warning, initialize on alert
                    this.Init(parent);
                } else if (this.source.warning != "" &&
                    elapsedTime >= (this.time - this.source.warningDuration) &&
                    elapsedTime < this.time + this.source.alertDuration) {
                    // If warning, initialize any time in duration
                    this.Init(parent);
                }
            }
            else if (this.panel != null) {
                // For on-going timers...
                float activeTime = elapsedTime - (this.time - this.source.warningDuration);
                if (activeTime >= this.source.warningDuration + this.source.alertDuration) {
                    // Dispose old timers
                    this.Stop();
                }
                else if (activeTime >= this.source.warningDuration) {
                    // Show alert text on completed timers.
                    this.panel.CurrentFill = this.source.warningDuration;
                    this.panel.Text = (this.source.alert == "" ||
                                        this.source.alert == null) ? this.source.warning : this.source.alert;
                    this.panel.TimerText = "";
                }
                else {
                    // Update incomplete timers.
                    this.panel.CurrentFill = activeTime + Encounter.TICKINTERVAL;
                    if ((this.source.warningDuration - activeTime) < 5) 
                        this.panel.TimerText = ((float)Math.Round((decimal)(this.source.warningDuration - activeTime), 1)).ToString("0.0");
                    else
                        this.panel.TimerText = ((float)Math.Floor((decimal)(this.source.warningDuration - activeTime))).ToString();
                }
            }
        }
        public void Stop() {
            if (this.panel == null) return;
            this.panel.Dispose();
            this.panel = null;
        }
    }
}
