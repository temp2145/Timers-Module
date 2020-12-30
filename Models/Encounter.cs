using System;
using System.Collections.Generic;
using System.Timers;
using Blish_HUD;
using Blish_HUD.Pathing.Content;
using temp.Timers.Controls;

namespace temp.Timers.Models {

    public class TimerReadException : Exception {
        public TimerReadException() { }
        public TimerReadException(string message) : base(message) { }
        public TimerReadException(string message, Exception inner) : base(message, inner) { }
    }

    public class Encounter : IDisposable {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public int map { get; set; }
        public List<Phase> phases { get; set; }
        public EndTrigger reset { get; set; }
        // Non-serialized
        public const int TICKRATE = 100;
        public const float TICKINTERVAL = (float) (TICKRATE / 1000.0f);
        public bool IsWatched { get; set; }
        public bool Activated { get; set; }
        private bool _active { get; set; }
        private bool _pendingUpdates { get; set; }
        private bool _awaitingNextPhase { get; set; }
        private int _currentPhase { get; set; }
        private Timer _clock { get; set; }
        private DateTime _startTime { get; set; }
        public void Init(PathableResourceManager pathableResourceManager) {
            // Validation
            if (this.map == 0) 
                throw new TimerReadException( this.id + ": could not read map property.");
            if (phases?.Count == 0) 
                throw new TimerReadException( this.id + ": could not read phase property.");
            string message = reset.Init();
            if (message != null)
                throw new TimerReadException(this.id + ": " + message);
            if (this.name == null || this.name == "")
                this.name = "Unknown Timer";
            if (this.category == null || this.category == "")
                this.category = "Other";
            if (this.description == null || this.description == "")
                this.description = "Timer description has not been set.";

            // Validate and Initialize Phases
            foreach (Phase ph in this.phases) {
                message = ph.Init(pathableResourceManager);
                if (message != null)
                    throw new TimerReadException(this.id + ": " + message);
            }
        }
        public bool ShouldStart () {
            // Check if already active.
            if (this._active) return false;

            // Double-check map. Should be unnecessary.
            if (this.map != GameService.Gw2Mumble.CurrentMap.Id) 
                return false;

            // Check trigger for first phase.
            Phase first = this.phases[0];
            return first.start.Passing();
        }
        public bool ShouldStop () {
            // Check if already inactive.
            if (!this._active) return false;

            // Double-check map. Should be unnecessary.
            if (this.map != GameService.Gw2Mumble.CurrentMap.Id)
                return true;

            // Check if in final phase and passing
            if (this._currentPhase == (this.phases.Count - 1) &&
                this.phases[this._currentPhase].finish != null &&
                this.phases[this._currentPhase].finish.Passing())
                return true;

            // Check encounter reset
            return this.reset.Passing();
        }
        public void Start(AlertContainer parent) {
            // Initialize timer
            this._clock.Start();
            this._startTime = DateTime.Now;
            this._active = true;
            this.phases[this._currentPhase].Update(parent, 0.0f);
        }
        public void Stop() {
            this._clock?.Stop();
            foreach (Phase ph in this.phases) {
                ph.Stop();
            }
            this._active = false;
            this._pendingUpdates = false;
            this._currentPhase = 0;
        }
        public void Pause() {
            // Timer is paused when waiting between phases.
            this._clock?.Stop();
            this._pendingUpdates = false;
        }
        public void Tick(object source, ElapsedEventArgs e) {
            this._pendingUpdates = true;
        }
        public void Activate() {
            foreach (Phase ph in this.phases) {
                ph.Activate();
            }
            this._clock?.Dispose();
            this._clock = new Timer(100);
            this._clock.AutoReset = true;
            this._clock.Elapsed += Tick;
            this.Activated = true;
        }
        public void Deactivate() {
            foreach (Phase ph in this.phases) {
                ph.Deactivate();
            }
            this.Stop();
            this._clock?.Dispose();
            this.Activated = false;
        }
        public void Update(AlertContainer parent) {
            if (this.ShouldStart()) {
                this.Start(parent);
            } else if (this.ShouldStop()) {
                this.Stop();
            } else if (this._awaitingNextPhase) {
                // Waiting period between phases.
                if (this._currentPhase + 1 < this.phases.Count) {
                    // In theory, the above condition should never fail...
                    if (this.phases[this._currentPhase + 1].start != null &&
                        this.phases[this._currentPhase + 1].start.Passing()) {
                        this._currentPhase++;
                        this._awaitingNextPhase = false;
                        this.Start(parent);
                    }
                }
            } else if (this.phases[this._currentPhase].finish != null &&
                       this.phases[this._currentPhase].finish.Passing()) {
                // Transition to waiting period between phases.
                this._awaitingNextPhase = true;
                this.Pause();
            } else if (this._pendingUpdates) {
                // Phase updates.
                float elapsedTime = (float) (DateTime.Now - this._startTime).TotalSeconds;
                this.phases[this._currentPhase].Update(parent, elapsedTime);
            }
        }
        public void Dispose() {
            foreach (Phase ph in this.phases) {
                ph.Dispose();
            }
            if (this._clock != null) {
                this._clock.Elapsed -= Tick;
                this._clock.Dispose();
            }
        }
    }

}
