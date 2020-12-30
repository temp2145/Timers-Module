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
    public enum AlertPanelOrientation {
        Horizontal,
        Vertical
    }

    public enum AlertPanelMode {
        Locked,
        Unlocked,
        Minimalist
    }

    [Export(typeof(Module))]
    public class TimersModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(TimersModule));

        internal static TimersModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        // Resources
        public Resources Resources;

        // Configuration
        private const int COUNTDOWN_SIDE_MARGIN = 50;
        private const int COUNTDOWN_SIDE_TOP_MARGIN = 400;
        private const int COUNTDOWN_CENTER_TOP_MARGIN = 250;

        // Controls - UI
        private WindowBase _alertWindow;
        private AlertContainer _alertPanel;
        private Label _debug;

        // Controls - Tab
        private WindowTab _timersTab;
        private Panel _tabPanel;
        private List<DetailsButton> _displayedTimers;

        // Settings
        private SettingEntry<AlertPanelOrientation> _alertOrientation;
        private SettingEntry<AlertPanelMode> _alertPanelMode;
        private SettingEntry<bool> _alertPanelCenter;
        private SettingEntry<bool> _showDirections;
        private SettingEntry<bool> _showMarkers;
        private SettingEntry<bool> _showDebug;
        private SettingCollection _timerCollection;

        // File reading
        private DirectoryReader _directoryReader;
        private PathableResourceManager _basePathableResourceManager;
        private List<PathableResourceManager> _pathableResourceManagers;
        private JsonSerializerOptions _jsonOptions;
        private bool _encountersLoaded;
        private bool _errorCaught;
        // Model
        private List<Encounter> _encounters;
        private List<Encounter> _activeEncounters;

        private EventHandler<EventArgs> _onNewMapLoaded;

        [ImportingConstructor]
        public TimersModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) 
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings (SettingCollection settings)
        {
            _alertOrientation = settings.DefineSetting("AlertOrientation", AlertPanelOrientation.Horizontal, "Alert Panel Orientation", "The direction in which alerts in alert panel will stack.");
            _alertPanelMode = settings.DefineSetting("AlertPanelMode", AlertPanelMode.Locked, "Alert Panel Mode", "Changes the presentation and interactibility of the alert panel.");
            _alertPanelCenter = settings.DefineSetting("AlertPanelCenter", true, "Center Alert Panel", "The alert panel will always be centered to the middle of the screen.");
            _showDirections = settings.DefineSetting("ShowDirections", true, "Show 3D Directions", "Directions are timed 3D trails that point the player to a certain location.");
            _showMarkers = settings.DefineSetting("ShowMarkers", true, "Show 3D Markers", "Markers are timed 3D objects that represent points of interest.");

            _showDebug = settings.DefineSetting("ShowDebug", false, "Show Debug Text", "Placed in top-left corner. Displays any timer-reading errors otherwise displays location and in-combat status.");
            _timerCollection = settings.AddSubCollection("Watching");
        }

        protected override void Initialize ()
        {
            // Singleton. Feels like a code smell. Plugging my nose now...
            Resources = new Resources();

            // Basic controls
            /*
            _alertWindow = new AlertWindow {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Alerts",
                Location = GameService.Graphics.SpriteScreen.Size / new Point(2)
            };
            */

            _alertPanel = new AlertContainer {
                Parent = GameService.Graphics.SpriteScreen,
                ControlPadding = new Vector2(10, 10),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Location = GameService.Graphics.SpriteScreen.Size / new Point(2)
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

            // Instantiations
            _encounters = new List<Encounter>();
            _activeEncounters = new List<Encounter>();
            _displayedTimers = new List<DetailsButton>();
            _pathableResourceManagers = new List<PathableResourceManager>();

            // Bind setting listeners
            _alertOrientation.SettingChanged += SettingsUpdateOrientation;
            _alertPanelMode.SettingChanged += SettingsUpdateAlertMode;
            _alertPanelCenter.SettingChanged += SettingsUpdateAlertCenter;
            _showDirections.SettingChanged += SettingsUpdateShowDirections;
            _showMarkers.SettingChanged += SettingsUpdateShowMarkers;
            _showDebug.SettingChanged += SettingsUpdateShowDebug;
        }

        #region Setting Handlers
        public AlertPanelOrientation AlertOrientation {
            get { return _alertOrientation.Value; }
        }
        public AlertPanelMode AlertMode {
            get { return _alertPanelMode.Value; }
        }
        public bool AlertCenter {
            get { return _alertPanelCenter.Value; }
        }
        public bool ShowDirections {
            get { return _showDirections.Value; }
        }
        public bool ShowMarkers {
            get { return _showMarkers.Value; }
        }
        public bool ShowDebug {
            get { return _showDebug.Value; }
        }
        private void SettingsUpdateOrientation(object sender = null, EventArgs e = null) {
            switch (_alertOrientation.Value) {
                case AlertPanelOrientation.Vertical:
                    _alertPanel.FlowDirection = ControlFlowDirection.SingleLeftToRight;
                    break;
                case AlertPanelOrientation.Horizontal:
                default:
                    _alertPanel.FlowDirection = ControlFlowDirection.SingleTopToBottom;
                    break;
            }
        }

        private void SettingsUpdateAlertMode(object sender = null, EventArgs e = null) {
            switch (_alertPanelMode.Value) {
                case AlertPanelMode.Locked:

                    break;
                case AlertPanelMode.Unlocked:

                    break;
                case AlertPanelMode.Minimalist:

                    break;
            }
        }

        private void SettingsUpdateAlertCenter(object sender = null, EventArgs e = null) {
        }

        private void SettingsUpdateShowDirections(object sender = null, EventArgs e = null) {
        }

        private void SettingsUpdateShowMarkers(object sender = null, EventArgs e = null) {
        }

        private void SettingsUpdateShowDebug(object sender = null, EventArgs e = null) {
            if (_showDebug.Value)
                _debug.Show();
            else
                _debug.Hide();
        }
        #endregion Setting Handlers

        protected override async Task LoadAsync ()
        {
            // Load data
            string timerDirectory = DirectoriesManager.GetFullDirectoryPath("timers");
            _directoryReader = new DirectoryReader(timerDirectory);
            _basePathableResourceManager = new PathableResourceManager(_directoryReader);
            _pathableResourceManagers.Add(_basePathableResourceManager);

            _jsonOptions = new JsonSerializerOptions {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues = true
            };

            // Load files directly
            _directoryReader.LoadOnFileType((Stream fileStream, IDataReader dataReader) => {
                readJson(fileStream, _basePathableResourceManager);
            }, ".bhtimer");

            // Load ZIP files
            List<string> zipFiles = new List<string>();
            zipFiles.AddRange(Directory.GetFiles(timerDirectory, "*.zip", SearchOption.AllDirectories));

            foreach (string zipFile in zipFiles) {
                ZipArchiveReader zipDataReader = new ZipArchiveReader(zipFile);
                PathableResourceManager zipResourceManager = new PathableResourceManager(zipDataReader);
                _pathableResourceManagers.Add(zipResourceManager);
                zipDataReader.LoadOnFileType((Stream fileStream, IDataReader dataReader) => {
                    readJson(fileStream, zipResourceManager);
                }, ".bhtimer");
            }

            // Final load tasks.
            // Is the above async? Can this be done sequentially?
            _encountersLoaded = true;
            _tabPanel = BuildSettingPanel(GameService.Overlay.BlishHudWindow.ContentRegion);
            _onNewMapLoaded = delegate {
                ResetActivatedEncounters();
            };
            ResetActivatedEncounters();
        }

        private void readJson (Stream fileStream, PathableResourceManager pathableResourceManager) {
            string jsonContent;
            using (var jsonReader = new StreamReader(fileStream)) {
                jsonContent = jsonReader.ReadToEnd();
            }

            Encounter enc = null;
            try {
                enc = JsonSerializer.Deserialize<Encounter>(jsonContent, _jsonOptions);
                enc.Init(pathableResourceManager);
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
        }

        protected override void OnModuleLoaded (EventArgs e)
        {
            GameService.Pathing.NewMapLoaded += _onNewMapLoaded;
            _timersTab = GameService.Overlay.BlishHudWindow.AddTab("Timers", ContentsManager.GetTexture(@"textures\155035small.png"), _tabPanel);
            
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void ResetActivatedEncounters () {
            _activeEncounters.Clear();
            foreach (Encounter enc in _encounters) {
                if (enc.Map == GameService.Gw2Mumble.CurrentMap.Id &&
                    enc.Enabled) {
                    if (!enc.Active)
                        enc.Active = true;
                    _activeEncounters.Add(enc);
                } else {
                    enc.Active = false;
                }
            }
        }

        private Panel BuildSettingPanel(Rectangle panelBounds) {

            // 1. Wrappers

            Panel markerPanel = new Panel {
                CanScroll = false,
                Size = panelBounds.Size
            };

            TextBox searchBox = new TextBox {
                Parent = markerPanel,
                Location = new Point(Dropdown.Standard.ControlOffset.Y, Panel.MenuStandard.PanelOffset.X),
                PlaceholderText = "Search"
            };

            Panel menuSection = new Panel {
                Parent = markerPanel,
                Location = new Point(Panel.MenuStandard.PanelOffset.X, searchBox.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Size = Panel.MenuStandard.Size - new Point(0, Panel.MenuStandard.ControlOffset.Y),
                Title = "Timer Categories",
                ShowBorder = true
            };

            searchBox.Width = menuSection.Width;

            FlowPanel timerPanel = new FlowPanel {
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

            // 2. Entries

            foreach (Encounter enc in _encounters) {

                SettingEntry<bool> setting = _timerCollection.DefineSetting("watchTimer:" + enc.Id, enc.Disabled);
                enc.Enabled = setting.Value;

                TimerDetailsButton entry = new TimerDetailsButton {
                    Parent = timerPanel,
                    BasicTooltipText = "Category: " + enc.Category,
                    Encounter = enc,
                    Text = enc.Name,
                    IconSize = DetailsIconSize.Small,
                    ShowVignette = false,
                    HighlightType = DetailsHighlightType.LightHighlight,
                    ShowToggleButton = true,
                    ToggleState = enc.Enabled,
                    Icon = enc.Icon
                };

                if (!string.IsNullOrEmpty(enc.Description)) {
                    GlowButton descButton = new GlowButton {
                        Icon = Resources.TextureDescription,
                        BasicTooltipText = enc.Description,
                        Parent = entry
                    };
                }

                if (!string.IsNullOrEmpty(enc.Author)) {
                    GlowButton authButton = new GlowButton {
                        Icon = Resources.TextureDescription,
                        BasicTooltipText = "By: " + enc.Author,
                        Parent = entry
                    };
                }

                GlowButton toggleButton = new GlowButton {
                    Icon = Resources.TextureEye,
                    ActiveIcon = Resources.TextureEyeActive,
                    BasicTooltipText = "Click to toggle timer",
                    ToggleGlow = true,
                    Checked = enc.Enabled,
                    Parent = entry
                };

                toggleButton.Click += delegate {
                    enc.Enabled = toggleButton.Checked;
                    setting.Value = toggleButton.Checked;
                    entry.ToggleState = toggleButton.Checked;
                    ResetActivatedEncounters();
                };

                _displayedTimers.Add(entry);

            }

            // 3. Categories

            Menu timerCategories = new Menu {
                Size = menuSection.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent = menuSection,
                CanSelect = true
            };

            List<IGrouping<string, Encounter>> categories = _encounters.GroupBy(enc => enc.Category).ToList();

            MenuItem timerAll = timerCategories.AddMenuItem("All Timers");
            timerAll.Select();
            timerAll.Click += delegate {
                timerPanel.FilterChildren<TimerDetailsButton>(db => true);
            };

            MenuItem timerActive = timerCategories.AddMenuItem("Enabled Timers");
            timerActive.Click += delegate {
                timerPanel.FilterChildren<TimerDetailsButton>(db => db.Encounter.Enabled);
            };

            MenuItem timerMap = timerCategories.AddMenuItem("Current Map");
            timerMap.Click += delegate {
                timerPanel.FilterChildren<TimerDetailsButton>(db => (db.Encounter.Map == GameService.Gw2Mumble.CurrentMap.Id));
            };

            foreach (IGrouping<string, Encounter> category in categories) {
                MenuItem cat = timerCategories.AddMenuItem(category.Key);
                cat.Click += delegate {
                    timerPanel.FilterChildren<TimerDetailsButton>(db => string.Equals(db.Encounter.Category, category.Key));
                };
            }

            return markerPanel;
        }

        protected override void Update (GameTime gameTime) {

            if (_encountersLoaded) {

                if (!_errorCaught && _debug.Visible) {
                    _debug.Text = "Debug: " +
                        GameService.Gw2Mumble.PlayerCharacter.Position.X.ToString("0.0") + " " +
                        GameService.Gw2Mumble.PlayerCharacter.Position.Y.ToString("0.0") + " " +
                        GameService.Gw2Mumble.PlayerCharacter.Position.Z.ToString("0.0") + " " +
                        GameService.Gw2Mumble.PlayerCharacter.IsInCombat.ToString();
                }

                _activeEncounters.ForEach(enc => enc.Update(_alertPanel));

                /*
                if (AlertCenter) {
                    _alertWindow.Location = new Point(
                        GameService.Graphics.SpriteScreen.Width / 2 - _alertWindow.Width / 2,
                        _alertWindow.Top);
                }
                */

            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here
            // Deregister event handlers
            GameService.Pathing.NewMapLoaded -= _onNewMapLoaded;
            _alertOrientation.SettingChanged -= SettingsUpdateOrientation;
            _alertPanelMode.SettingChanged -= SettingsUpdateAlertMode;
            _alertPanelCenter.SettingChanged -= SettingsUpdateAlertCenter;
            _showDirections.SettingChanged -= SettingsUpdateShowDirections;
            _showMarkers.SettingChanged -= SettingsUpdateShowMarkers;
            _showDebug.SettingChanged -= SettingsUpdateShowDebug;

            // Cleanup tab
            GameService.Overlay.BlishHudWindow.RemoveTab(_timersTab);
            _tabPanel.Dispose();
            _displayedTimers.ForEach(de => de.Dispose());
            _displayedTimers.Clear();

            // Cleanup model
            _encounters.ForEach(enc => enc.Dispose());

            // Cleanup readers and resource managers
            _directoryReader.Dispose();
            _pathableResourceManagers.ForEach(GameService.Pathing.UnregisterPathableResourceManager);
            _pathableResourceManagers.ForEach(m => m.Dispose());
            _basePathableResourceManager = null;

            // Cleanup leftover UI
            // _alertWindow.Dispose();
            _alertPanel.Dispose();
            _debug.Dispose();

            Resources.Dispose();

            // All static members must be manually unset
            ModuleInstance = null;
        }

    }

}