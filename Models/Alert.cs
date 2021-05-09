using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Pathing.Content;
using Microsoft.Xna.Framework;
using temp.Timers.Controls;

namespace temp.Timers.Models {

    public class AlertType : IDisposable {

        // Serialized
        [JsonPropertyName("warningDuration")]
        public float WarningDuration { get; set; } = 15.0f;
        [JsonPropertyName("alertDuration")]
        public float AlertDuration { get; set; } = 5.0f;
        [JsonPropertyName("warning")]
        public string WarningText { get; set; }
        [JsonPropertyName("warningColor")]
        public List<float> WarningTextColor { get; set; }
        [JsonPropertyName("alert")]
        public string AlertText { get; set; }
        [JsonPropertyName("alertColor")]
        public List<float> AlertTextColor { get; set; }
        [JsonPropertyName("icon")]
        public string IconString { get; set; } = "raid";
        [JsonPropertyName("fillColor")]
        public List<float> FillColor { get; set; }
        [JsonPropertyName("timestamps")]
        public List<float> Timestamps { get; set; }

        // Non-serialized
        [JsonIgnore]
        public Color Fill { get; set; } = Color.DarkGray;
        [JsonIgnore]
        public Color WarningColor { get; set; } = Color.White;
        [JsonIgnore]
        public Color AlertColor { get; set; } = Color.White;
        [JsonIgnore]
        public AsyncTexture2D Icon { get; set; }

        public string Init(PathableResourceManager pathableResourceManager) {
            if (string.IsNullOrEmpty(WarningText))
                WarningDuration = 0;
            if (string.IsNullOrEmpty(AlertText))
                AlertDuration = 0;

            if (Timestamps?.Count == 0)
                return WarningText + "/" + AlertText + " timestamps property invalid";

            Fill = Resources.ParseColor(Fill, FillColor);
            WarningColor = Resources.ParseColor(WarningColor, WarningTextColor);
            AlertColor = Resources.ParseColor(AlertColor, AlertTextColor);

            Icon = TimersModule.ModuleInstance.Resources.GetIcon(IconString);
            if (Icon == null) 
                Icon = pathableResourceManager.LoadTexture(IconString);

            return null;
        }

        public void Dispose() { 
            Icon.Dispose();
        }
    }

    public class Alert : IDisposable {
        public float Time { get; set; }
        public AlertType Source { get; set; }
        // Non-serialized
        public AlertPanel Panel { get; set; }
        public void Init(FlowPanel parent) {
            Panel = new AlertPanel {
                Parent = parent,
                Text = (string.IsNullOrEmpty(Source.WarningText)) ? Source.AlertText : Source.WarningText,
                TextColor = Source.WarningColor,
                Icon = Texture2DExtension.Duplicate(Source.Icon),
                MaxFill = Source.WarningDuration,
                CurrentFill = 0.0f,
                FillColor = Source.Fill
            };
        }
        public void Update(AlertContainer parent, float elapsedTime) {
            if (Panel == null) {
                if (string.IsNullOrEmpty(Source.WarningText) &&
                    elapsedTime >= Time &&
                    elapsedTime < Time + Source.AlertDuration) {
                    // If no warning, initialize on alert
                    Init(parent);
                } else if (!string.IsNullOrEmpty(Source.WarningText) &&
                    elapsedTime >= (Time - Source.WarningDuration) &&
                    elapsedTime < Time + Source.AlertDuration) {
                    // If warning, initialize any time in duration
                    Init(parent);
                }
            }
            else if (Panel != null) {
                // For on-going timers...
                float activeTime = elapsedTime - (Time - Source.WarningDuration);
                if (activeTime >= Source.WarningDuration + Source.AlertDuration) {
                    // Dispose old timers
                    Stop();
                } else if (activeTime >= Source.WarningDuration) {
                    // Show alert text on completed timers.
                    if (Panel.CurrentFill != Source.WarningDuration)
                        Panel.CurrentFill = Source.WarningDuration;
                    Panel.Text = (string.IsNullOrEmpty(Source.AlertText)) ? Source.WarningText : Source.AlertText;
                    Panel.TimerText = "";
                    Panel.TextColor = Source.AlertColor;
                }
                else {
                    // Update incomplete timers.
                    Panel.CurrentFill = activeTime + TimersModule.ModuleInstance.Resources.TICKINTERVAL;
                    if ((Source.WarningDuration - activeTime) < 5) {
                        Panel.TimerText = ((float)Math.Round((decimal)(Source.WarningDuration - activeTime), 1)).ToString("0.0");
                        Panel.TimerTextColor = Color.Yellow;
                    } else {
                        Panel.TimerText = ((float)Math.Floor((decimal)(Source.WarningDuration - activeTime))).ToString();
                    }
                }
            }
        }
        public void Stop() {
            Dispose();
        }
        public void Deactivate() {
            Dispose();
        }
        public void Dispose() {
            if (Panel == null) return;
            Panel.Dispose();
            Panel = null;
        }
    }
}
