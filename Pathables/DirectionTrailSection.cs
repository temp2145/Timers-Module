using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework;

namespace temp.Timers.Pathables {
    class DirectionTrailSection : ScrollingTrailSection {
        
        public bool Active { get; set; }

        public DirectionTrailSection() : base(null) { }
        public DirectionTrailSection(List<Vector3> points) : base(points) { }

        public override void Update(GameTime gameTime) {

            if (Active && this._trailPoints.Count == 2) {
                _trailPoints[0] = GameService.Gw2Mumble.PlayerCharacter.Position;
                InitTrailPoints();
            }

            base.Update(gameTime);
        }
    }
}
