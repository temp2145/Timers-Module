using System;
using System.Collections.Generic;
using Blish_HUD;

namespace temp.Timers.Models {

    public class StartTrigger {
        public List<float> position { get; set; }
        public float radius { get; set; }
        public bool requireCombat { get; set; }
        // Non-serialized
        public string Init() {
            if (this.position?.Count != 3)
                return "invalid start position";
            if (this.radius == 0)
                return "invalid start radius";
            return null;
        }
        public bool Passing() {
            // Check combat status on first phase.
            if (this.requireCombat && !GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
                return false;

            if (Math.Sqrt(
                Math.Pow(GameService.Gw2Mumble.PlayerCharacter.Position.X - this.position[0], 2) +
                Math.Pow(GameService.Gw2Mumble.PlayerCharacter.Position.Y - this.position[1], 2) +
                Math.Pow(GameService.Gw2Mumble.PlayerCharacter.Position.Z - this.position[2], 2)) < this.radius) {
                return true;
            }

            return false;
        }
    }

    public class EndTrigger {
        public List<float> position { get; set; }
        public float radius { get; set; }
        public bool requireOutOfCombat { get; set; }
        public bool requireDeparture { get; set; }
        public string Init() {
            if (this.requireDeparture && this.position?.Count != 3)
                return "invalid finish position";
            if (this.requireDeparture && this.radius == 0)
                return "invalid finish radius";
            return null;
        }
        public bool Passing() {
            bool hasMetRequirements = false;
            if (this.requireOutOfCombat) {
                if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat) return false;
                hasMetRequirements = true;
            }
            if (this.requireDeparture) {
                if (Math.Sqrt(
                    Math.Pow(GameService.Gw2Mumble.PlayerCharacter.Position.X - this.position[0], 2) +
                    Math.Pow(GameService.Gw2Mumble.PlayerCharacter.Position.Y - this.position[1], 2) +
                    Math.Pow(GameService.Gw2Mumble.PlayerCharacter.Position.Z - this.position[2], 2)) < this.radius) {
                    return false;
                }
                hasMetRequirements = true;
            }
            return hasMetRequirements;
        }
    }

}
