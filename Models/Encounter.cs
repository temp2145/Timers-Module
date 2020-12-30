using System;
using System.Collections.Generic;
using System.Timers;
using System.Text.Json.Serialization;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Pathing.Content;
using temp.Timers.Controls;

namespace temp.Timers.Models {

    public class TimerReadException : Exception {
        public TimerReadException() { }
        public TimerReadException(string message) : base(message) { }
        public TimerReadException(string message, Exception inner) : base(message, inner) { }
    }

    public class Encounter : IDisposable {

        // Serialized Properties
        [JsonPropertyName("id")]
        public string Id { get; set; } = "unknown";
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unknown Timer";
        [JsonPropertyName("category")]
        public string Category { get; set; } = "Other";
        [JsonPropertyName("description")]
        public string Description { get; set; } = "Timer description has not been set.";
        [JsonPropertyName("author")]
        public string Author { get; set; } = "Unknown Author";
        [JsonPropertyName("icon")]
        public string IconString { get; set; } = "raid";
        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; }
        [JsonPropertyName("map")]
        public int Map { get; set; }
        [JsonPropertyName("phases")]
        public List<Phase> Phases { get; set; }
        [JsonPropertyName("reset")]
        public EndTrigger Reset { get; set; }


        // Non-serialized
        [JsonIgnore]
        public bool Enabled { get; set; }
        [JsonIgnore]
        public bool Active { 
            get { return _active; }
            set {
                _active = value;
                if (value) {
                    foreach (Phase ph in Phases) {
                        ph.Activate();
                    }
                    if (_clock != null) {
                        _clock.Elapsed -= Tick;
                        _clock.Dispose();
                        _clock = null;
                    }
                    _clock = new Timer(TimersModule.ModuleInstance.Resources.TICKINTERVAL);
                    _clock.AutoReset = true;
                    _clock.Elapsed += Tick;
                } else {
                    foreach (Phase ph in Phases) {
                        ph.Deactivate();
                    }
                    Stop();
                    if (_clock != null) {
                        _clock.Elapsed -= Tick;
                        _clock.Dispose();
                        _clock = null;
                    }
                }
            } }
        [JsonIgnore]
        public AsyncTexture2D Icon { get; set; }


        // Private members
        private bool _active { get; set; }
        private bool _pendingUpdates { get; set; }
        private bool _awaitingNextPhase { get; set; }
        private int _currentPhase { get; set; }
        private Timer _clock { get; set; }
        private DateTime _startTime { get; set; }


        public void Init (PathableResourceManager pathableResourceManager) {
            // Validation
            if (Map == 0) 
                throw new TimerReadException( Id + ": map property undefined");
            if (Phases?.Count == 0) 
                throw new TimerReadException( Id + ": phase property undefined");

            string message = Reset.Init();
            if (message != null)
                throw new TimerReadException( Id + ": " + message);

            // Validate and Initialize Phases
            foreach (Phase ph in Phases) {
                message = ph.Init(pathableResourceManager);
                if (message != null)
                    throw new TimerReadException( Id + ": " + message);
            }

            Icon = TimersModule.ModuleInstance.Resources.GetIcon(IconString);
            if (Icon == null)
                Icon = pathableResourceManager.LoadTexture(IconString);
        }


        // Private Methods
        private bool ShouldStart () {
            // Check if already active.
            if (Active) return false;

            // Double-check map. Should be unnecessary.
            if (Map != GameService.Gw2Mumble.CurrentMap.Id) 
                return false;

            // Check trigger for first phase.
            Phase first = Phases[0];
            return first.Start.Passing();
        }
        private bool ShouldStop () {
            // Check if already inactive.
            if (!Active) return false;

            // Double-check map. Should be unnecessary.
            if (Map != GameService.Gw2Mumble.CurrentMap.Id)
                return true;

            // Check if in final phase and passing
            if (_currentPhase == (Phases.Count - 1) &&
                Phases[_currentPhase].Finish != null &&
                Phases[_currentPhase].Finish.Passing())
                return true;

            // Check encounter reset
            return Reset.Passing();
        }
        private void Start(AlertContainer parent) {
            // Initialize timer
            _clock.Start();
            _startTime = DateTime.Now;
            _active = true;
            Phases[_currentPhase].Update(parent, 0.0f);
        }
        private void Stop() {
            _clock?.Stop();
            foreach (Phase ph in Phases) {
                ph.Stop();
            }
            _active = false;
            _pendingUpdates = false;
            _currentPhase = 0;
        }
        private void Pause() {
            // Timer is paused when waiting between phases.
            _clock?.Stop();
            _pendingUpdates = false;
        }
        private void Tick(object source, ElapsedEventArgs e) {
            _pendingUpdates = true;
        }


        // Public Methods
        public void Update(AlertContainer parent) {
            if (ShouldStart()) {
                Start(parent);
            } else if (ShouldStop()) {
                Stop();
            } else if (_awaitingNextPhase) {
                // Waiting period between phases.
                if (_currentPhase + 1 < Phases.Count) {
                    // In theory, the above condition should never fail...
                    if (Phases[_currentPhase + 1].Start != null &&
                        Phases[_currentPhase + 1].Start.Passing()) {
                        _currentPhase++;
                        _awaitingNextPhase = false;
                        Start(parent);
                    }
                }
            } else if (Phases[_currentPhase].Finish != null &&
                       Phases[_currentPhase].Finish.Passing()) {
                // Transition to waiting period between phases.
                _awaitingNextPhase = true;
                Pause();
            } else if (this._pendingUpdates) {
                // Phase updates.
                float elapsedTime = (float) (DateTime.Now - _startTime).TotalSeconds;
                Phases[_currentPhase].Update(parent, elapsedTime);
            }
        }
        public void Dispose() {
            foreach (Phase ph in Phases) {
                ph.Dispose();
            }
            if (_clock != null) {
                _clock.Elapsed -= Tick;
                _clock.Dispose();
            }
            if (Icon != null) {
                Icon.Dispose();
            }
        }
    }

}
