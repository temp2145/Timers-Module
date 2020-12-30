using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Blish_HUD;

namespace temp.Timers.Models {

    public class StartTrigger {

        // Serialized
        [JsonPropertyName("position")]
        public List<float> Position { get; set; }
        [JsonPropertyName("antipode")]
        public List<float> Antipode { get; set; }
        [JsonPropertyName("radius")]
        public float Radius { get; set; }
        [JsonPropertyName("requireCombat")]
        public bool CombatRequired { get; set; }

        // Methods
        public string Init() {
            if (Position?.Count != 3)
                return "invalid start position";
            if (Antipode?.Count != 3 && Radius == 0)
                return "invalid start radius/size";
            return null;
        }
        public bool Passing() {
            // Check combat status
            if (CombatRequired && !GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
                return false;

            float x = GameService.Gw2Mumble.PlayerCharacter.Position.X;
            float y = GameService.Gw2Mumble.PlayerCharacter.Position.Y;
            float z = GameService.Gw2Mumble.PlayerCharacter.Position.Z;

            if (Antipode?.Count == 3) {
                if (x > Math.Min(Position[0], Antipode[0]) && x < Math.Max(Position[0], Antipode[0]) &&
                    y > Math.Min(Position[1], Antipode[1]) && y < Math.Max(Position[1], Antipode[1]) &&
                    z > Math.Min(Position[2], Antipode[2]) && z < Math.Max(Position[2], Antipode[2]))
                    return true;
            } else {
                if (Math.Sqrt(
                    Math.Pow(x - Position[0], 2) +
                    Math.Pow(y - Position[1], 2) +
                    Math.Pow(z - Position[2], 2)) < Radius) {
                    return true;
                }
            }

            return false;
        }
    }

    public class EndTrigger {

        // Serialized
        [JsonPropertyName("position")]
        public List<float> Position { get; set; }
        [JsonPropertyName("antipode")]
        public List<float> Antipode { get; set; }
        [JsonPropertyName("radius")]
        public float Radius { get; set; }
        [JsonPropertyName("requireOutOfCombat")]
        public bool OutOfCombatRequired { get; set; }
        [JsonPropertyName("requireDeparture")]
        public bool DepartureRequired { get; set; }

        // Methods
        public string Init() {
            if (DepartureRequired && Position?.Count != 3)
                return "invalid finish position";
            if (DepartureRequired && Antipode?.Count != 3 && Radius == 0)
                return "invalid finish radius";
            if (!DepartureRequired && !OutOfCombatRequired)
                return "no possible finish conditions";
            return null;
        }
        public bool Passing() {
            bool hasMetRequirements = false;
            if (OutOfCombatRequired) {
                if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat) return false;
                hasMetRequirements = true;
            }
            if (DepartureRequired) {

                float x = GameService.Gw2Mumble.PlayerCharacter.Position.X;
                float y = GameService.Gw2Mumble.PlayerCharacter.Position.Y;
                float z = GameService.Gw2Mumble.PlayerCharacter.Position.Z;

                if (Antipode?.Count == 3) {
                    if (x > Math.Min(Position[0], Antipode[0]) && x < Math.Max(Position[0], Antipode[0]) &&
                        y > Math.Min(Position[1], Antipode[1]) && y < Math.Max(Position[1], Antipode[1]) &&
                        z > Math.Min(Position[2], Antipode[2]) && z < Math.Max(Position[2], Antipode[2]))
                        return false;
                }
                else {
                    if (Math.Sqrt(
                        Math.Pow(x - Position[0], 2) +
                        Math.Pow(y - Position[1], 2) +
                        Math.Pow(z - Position[2], 2)) < Radius) {
                        return false;
                    }
                }
                hasMetRequirements = true;

            }
            return hasMetRequirements;
        }
    }

}
