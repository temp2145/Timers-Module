using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Pathing.Content;
using temp.Timers.Controls;
using temp.Timers.Models;

namespace temp.Timers
{
    public enum AlertOrientation {
        Horizontal,
        Vertical
    }

    [Export(typeof(Module))]
    public class TimersModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(TimersModule));

        internal static TimersModule ModuleInstance;
        public static Resources Resources;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        // Configuration
        private const int COUNTDOWN_SIDE_MARGIN = 50;
        private const int COUNTDOWN_SIDE_TOP_MARGIN = 400;
        private const int COUNTDOWN_CENTER_TOP_MARGIN = 250;

        private AlertContainer _timerPanel;
        private Label _debug;

        private WindowTab _timersTab;
        private Panel _tabPanel;

        private List<Encounter> _encounters;
        private List<Encounter> _activeEncounters;

        private EventHandler<EventArgs> _onNewMapLoaded;

        private SettingEntry<HorizontalAlignment> _timerAlignment;
        private SettingEntry<bool> _showDebug;
        private SettingCollection _timerCollection;

        private DirectoryReader _directoryReader;
        private PathableResourceManager _pathableResourceManager;

        private Texture2D _textureWatch;
        private Texture2D _textureWatchActive;
        private List<DetailsButton> _displayedTimers;

        private bool _encountersLoaded;
        private bool _errorCaught;

        [ImportingConstructor]
        public TimersModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) 
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings (SettingCollection settings)
        {
            _timerAlignment = settings.DefineSetting("TimerAlignment", HorizontalAlignment.Center, "Timer Position", "Changes Screen Location of Timers");
            _showDebug = settings.DefineSetting("ShowDebug", false, "Show Debug Text", "Placed in top-left corner. Displays any timer-reading errors otherwise displays location and in-combat status.");
            _timerCollection = settings.AddSubCollection("Watching");
        }

        protected override void Initialize ()
        {
            // Singleton. Feels like a code smell. Plugging my nose now...
            Resources = new Resources();

            _timerPanel = new AlertContainer {
                Parent = GameService.Graphics.SpriteScreen,
                ControlPadding = new Vector2(10, 10),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize
            };

            _debug = new Label {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(10, 33),
                Size = new Point(800, 33),
                TextColor = Color.White,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24,
                    ContentService.FontStyle.Regular),
                Text = "DEBUG",
                HorizontalAlignment = HorizontalAlignment.Left,
                StrokeText = true,
                ShowShadow = true,
                AutoSizeWidth = true
            };

            _timerAlignment.SettingChanged += UpdateAlignment;
            _showDebug.SettingChanged += UpdateDebug;
            UpdateAlignment();
            UpdateDebug();

            _encounters = new List<Encounter>();
            _activeEncounters = new List<Encounter>();
            _displayedTimers = new List<DetailsButton>();

            _onNewMapLoaded = delegate {
                CheckForEncounters();
            };

        }

        protected override async Task LoadAsync ()
        {
            _encounters.Clear();

            _textureWatch = ContentsManager.GetTexture(@"textures\605021.png");
            _textureWatchActive = ContentsManager.GetTexture(@"textures\605019.png");

            _directoryReader = new DirectoryReader(DirectoriesManager.GetFullDirectoryPath("timers"));
            _pathableResourceManager = new PathableResourceManager(_directoryReader);

            JsonSerializerOptions options = new JsonSerializerOptions {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            _directoryReader.LoadOnFileType((Stream fileStream, IDataReader dataReader) => {

                string jsonContent;
                using (var jsonReader = new StreamReader(fileStream)) {
                    jsonContent = jsonReader.ReadToEnd();
                }

                Encounter enc = null;
                try {
                    enc = JsonSerializer.Deserialize<Encounter>(jsonContent, options);
                    enc.Init(_pathableResourceManager);
                    _encounters.Add(enc);
                }
                catch (TimerReadException ex) {
                    if (enc != null) enc.Dispose();
                    _debug.Text = ex.Message;
                    _errorCaught = true;
                    Logger.Error("Timer parsing failure: " + ex.Message);
                }
                catch (Exception ex) {
                    if (enc != null) enc.Dispose();
                    _debug.Text = ex.Message;
                    _errorCaught = true;
                    Logger.Error("Deserialization failure: " + ex.Message);
                }

            }, ".bhtimer");

            // Is the above async? Can this be done sequentially?
            _encountersLoaded = true;
            _tabPanel = BuildSettingPanel(GameService.Overlay.BlishHudWindow.ContentRegion);
            CheckForEncounters();
        }

        protected override void OnModuleLoaded (EventArgs e)
        {
            GameService.Pathing.NewMapLoaded += _onNewMapLoaded;
            _timersTab = GameService.Overlay.BlishHudWindow.AddTab("Timers", this.ContentsManager.GetTexture(@"textures\155035small.png"), _tabPanel);
            
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        // Repositions countdown panel to the desired location.
        private void UpdateAlignment (object sender = null, EventArgs e = null) {
            switch (_timerAlignment.Value) {
                case HorizontalAlignment.Left:
                    _timerPanel.FlowDirection = ControlFlowDirection.SingleTopToBottom;
                    _timerPanel.Location = new Point(COUNTDOWN_SIDE_MARGIN, COUNTDOWN_SIDE_TOP_MARGIN);
                    break;
                case HorizontalAlignment.Right:
                    _timerPanel.FlowDirection = ControlFlowDirection.SingleTopToBottom;
                    _timerPanel.Location = new Point(GameService.Graphics.SpriteScreen.Width - (300 + COUNTDOWN_SIDE_MARGIN), COUNTDOWN_SIDE_TOP_MARGIN);
                    break;
                default:
                    _timerPanel.FlowDirection = ControlFlowDirection.SingleLeftToRight;
                    _timerPanel.Location = new Point(GameService.Graphics.SpriteScreen.Width / 2, COUNTDOWN_CENTER_TOP_MARGIN);
                    break;
            }
        }

        private void UpdateDebug (object sender = null, EventArgs e = null) {
            if (_showDebug.Value) {
                _debug.Show();
            } else {
                _debug.Hide();
            }
        }

        private void CheckForEncounters () {
            _activeEncounters.Clear();
            foreach (Encounter enc in _encounters) {
                if (enc.map == GameService.Gw2Mumble.CurrentMap.Id &&
                    enc.IsWatched) {
                    if (!enc.Activated)
                        enc.Activate();
                    _activeEncounters.Add(enc);
                } else {
                    enc.Deactivate();
                }
            }
        }

        private Panel BuildSettingPanel(Rectangle panelBounds) {

            var markerPanel = new Panel {
                CanScroll = false,
                Size = panelBounds.Size
            };

            var searchBox = new TextBox {
                Parent = markerPanel,
                Location = new Point(Dropdown.Standard.ControlOffset.Y, Panel.MenuStandard.PanelOffset.X),
                PlaceholderText = "Search"
            };

            var menuSection = new Panel {
                Parent = markerPanel,
                Location = new Point(Panel.MenuStandard.PanelOffset.X, searchBox.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Size = Panel.MenuStandard.Size - new Point(0, Panel.MenuStandard.ControlOffset.Y),
                Title = "Timer Categories",
                ShowBorder = true
            };

            searchBox.Width = menuSection.Width;

            var timerPanel = new FlowPanel {
                Parent = markerPanel,
                Location = new Point(menuSection.Right + Panel.MenuStandard.ControlOffset.X, Panel.MenuStandard.PanelOffset.X),
                Size = new Point(markerPanel.Right - menuSection.Right - Control.ControlStandard.ControlOffset.X, markerPanel.Height - Panel.MenuStandard.PanelOffset.X),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(8, 8),
                CanScroll = true
            };

            searchBox.TextChanged += delegate (object sender, EventArgs args) {
                timerPanel.FilterChildren<TimerDetailsButton>(db => db.Text.ToLower().Contains(searchBox.Text.ToLower()));
            };

            foreach (Encounter enc in this._encounters) {

                var setting = _timerCollection.DefineSetting("watchTimer:" + enc.id, true);
                enc.IsWatched = setting.Value;

                var entry = new TimerDetailsButton {
                    Parent = timerPanel,
                    BasicTooltipText = enc.description,
                    Encounter = enc,
                    Text = enc.name,
                    IconSize = DetailsIconSize.Large,
                    ShowVignette = false,
                    HighlightType = DetailsHighlightType.LightHighlight,
                    ShowToggleButton = true,
                    ToggleState = enc.IsWatched,
                    Icon = Resources.getIcon(enc.icon, "boss")
                };

                var toggleButton = new GlowButton {
                    Icon = _textureWatch,
                    ActiveIcon = _textureWatchActive,
                    BasicTooltipText = "Click to toggle timer",
                    ToggleGlow = true,
                    Checked = enc.IsWatched,
                    Parent = entry
                };

                toggleButton.Click += delegate {
                    enc.IsWatched = toggleButton.Checked;
                    setting.Value = toggleButton.Checked;
                    entry.ToggleState = toggleButton.Checked;
                    CheckForEncounters();
                };

                _displayedTimers.Add(entry);
            }

            var timerCategories = new Menu {
                Size = menuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent = menuSection,
                CanSelect = true
            };

            List<IGrouping<string, Encounter>> categories = _encounters.GroupBy(e => e.category).ToList();

            var timerAll = timerCategories.AddMenuItem("All Timers");
            timerAll.Select();
            timerAll.Click += delegate {
                timerPanel.FilterChildren<TimerDetailsButton>(db => true);
            };

            var timerActive = timerCategories.AddMenuItem("Active Timers");
            timerActive.Click += delegate {
                timerPanel.FilterChildren<TimerDetailsButton>(db => db.Encounter.IsWatched);
            };

            var timerMap = timerCategories.AddMenuItem("Current Map");
            timerMap.Click += delegate {
                timerPanel.FilterChildren<TimerDetailsButton>(db => (db.Encounter.map == GameService.Gw2Mumble.CurrentMap.Id));
            };

            foreach (IGrouping<string, Encounter> category in categories) {
                var cat = timerCategories.AddMenuItem(category.Key);
                cat.Click += delegate {
                    timerPanel.FilterChildren<TimerDetailsButton>(db => string.Equals(db.Encounter.category, category.Key));
                };
            }

            return markerPanel;
        }

        protected override void Update (GameTime gameTime) {

            if (_encountersLoaded) {

                if (!_errorCaught) {
                    _debug.Text = "Debug: " +
                        GameService.Gw2Mumble.PlayerCharacter.Position.X.ToString("0.0") + " " +
                        GameService.Gw2Mumble.PlayerCharacter.Position.Y.ToString("0.0") + " " +
                        GameService.Gw2Mumble.PlayerCharacter.Position.Z.ToString("0.0") + " " +
                        GameService.Gw2Mumble.PlayerCharacter.IsInCombat.ToString();
                }

                foreach (Encounter enc in _activeEncounters) {
                    enc.Update(_timerPanel);
                }

                if (_timerAlignment.Value == HorizontalAlignment.Center) {
                    _timerPanel.Location = new Point(
                        GameService.Graphics.SpriteScreen.Width / 2 - _timerPanel.Width / 2,
                        COUNTDOWN_CENTER_TOP_MARGIN);
                }

            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here
            GameService.Pathing.NewMapLoaded -= _onNewMapLoaded;

            GameService.Overlay.BlishHudWindow.RemoveTab(_timersTab);
            _tabPanel.Dispose();
            
            _displayedTimers.ForEach(de => de.Dispose());
            _displayedTimers.Clear();

            foreach (Encounter enc in _encounters) {
                enc.Dispose();
            }
            _directoryReader.Dispose();
            _pathableResourceManager.Dispose();
            _timerPanel.Dispose();
            _debug.Dispose();


            // All static members must be manually unset
            Resources.Unload();
            Resources = null;
            ModuleInstance = null;
        }

    }

}