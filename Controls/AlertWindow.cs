using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace temp.Timers.Controls {

    // Effectively a copy of WindowBase with some features removed and added.

    class AlertWindow : Container {

        private const int TITLEBAR_WIDTH = 320;
        private const int TITLEBAR_HEIGHT = 32;
        private const int COMMON_MARGIN = 16;
        private const int TITLE_OFFSET = 80;
        private const int SUBTITLE_OFFSET = 20;

        protected string _title = "No Title";
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        protected string _subtitle = "";
        public string Subtitle {
            get => _subtitle;
            set => SetProperty(ref _subtitle, value);
        }

        protected Texture2D _emblem = null;
        public Texture2D Emblem {
            get => _emblem;
            set => SetProperty(ref _emblem, value);
        }

        private Panel _activePanel;
        public Panel ActivePanel {
            get => _activePanel;
            set => {
                if (_activePanel != null) {
                    _activePanel.Hide();
                    _activePanel.Parent = null;
                }

                if (value == null) return;

                _activePanel = value;
                _activePanel.Parent = this;
                _activePanel.Location = Point.Zero;
                _activePanel.Size = this.ContentRegion.Size;
            }
        }

        private readonly Glide.Tween _animFade;

        protected bool Dragging = false;
        protected Point DragStart = Point.Zero;
        
        protected Vector2 _windowBackgroundOrigin;
        protected Rectangle _windowBackgroundBounds;
        protected Rectangle _titleBarBounds;

        private Rectangle _layoutLeftTitleBarBounds;
        private Rectangle _layoutRightTitleBarBounds;
        private Rectangle _layoutSubtitleBounds;
        private Rectangle _layoutWindowCornerBounds;

        protected bool MouseOverTitleBar = false;

/*
public AlertWindow() : base() {
    Texture2D bg = Content.GetTexture(@"controls/notification/notification-gray");
    ConstructWindow(bg,
                    new Vector2(0, TITLEBAR_HEIGHT),
                    new Rectangle(0, 0, 320, 320),
                    Thickness.Zero,
                    TITLEBAR_HEIGHT,
                    true);
    ContentRegion = new Rectangle(0, TITLEBAR_HEIGHT, _size.X, _size.Y - TITLEBAR_HEIGHT);
    Show();
}
*/

        public AlertWindow () {

            this.Opacity = 0f;
            this.Visible = false;
            this.ZIndex = Screen.WINDOW_BASEZINDEX;

            Input.Mouse.LeftMouseButtonReleased += delegate { Dragging = false; };

            _animFade = Animation.Tweener.Tween(this, new { Opacity = 1f }, 0.2f).Repeat().Reflect();
            _animFade.Pause();

            _animFade.OnComplete(() => {
                _animFade.Pause();
                if (_opacity <= 0) this.Visible = false;
            });

            // Previously ConstructWindow()

            this.Padding = Thickness.Zero;

            _titleBarBounds = new Rectangle(0, 0, TITLEBAR_WIDTH, TITLEBAR_HEIGHT);
            _windowBackgroundBounds = new Rectangle(0,
                TITLEBAR_HEIGHT,
                TITLEBAR_WIDTH + (int) _padding.Right + (int) _padding.Left,
                TITLEBAR_HEIGHT + (int) _padding.Bottom);

        }

        public override void RecalculateLayout () {

            // Title bar bounds 
            int titleBarDrawOffset = _titleBarBounds.Y -
                (TimersModule.ModuleInstance.Resources.WindowTitleBarLeft.Height / 2 -
                _titleBarBounds.Height / 2);
            int titleBarRightWidth = TimersModule.ModuleInstance.Resources.WindowTitleBarRight.Width - COMMON_MARGIN;

            _layoutLeftTitleBarBounds = new Rectangle(
                _titleBarBounds.X,
                titleBarDrawOffset,
                Math.Min(_titleBarBounds.Width - titleBarRightWidth, _windowBackgroundBounds.Width - titleBarRightWidth),
                TimersModule.ModuleInstance.Resources.WindowTitleBarLeft.Height);

            _layoutRightTitleBarBounds = new Rectangle(
                _titleBarBounds.Right - titleBarRightWidth,
                titleBarDrawOffset,
                TimersModule.ModuleInstance.Resources.WindowTitleBarRight.Width,
                TimersModule.ModuleInstance.Resources.WindowTitleBarRight.Height);

            // Title bar text bounds
            if (!string.IsNullOrEmpty(_title) && !string.IsNullOrEmpty(_subtitle)) {
                int titleTextWidth = (int)Content.DefaultFont32.MeasureString(_title).Width;
                _layoutSubtitleBounds = _layoutLeftTitleBarBounds.OffsetBy(TITLE_OFFSET + titleTextWidth + SUBTITLE_OFFSET, 0);
            }

            // Corner edge bounds
            _layoutWindowCornerBounds = new Rectangle(
                _layoutRightTitleBarBounds.Right - TimersModule.ModuleInstance.Resources.WindowCorner.Width - COMMON_MARGIN,
                this.ContentRegion.Bottom - TimersModule.ModuleInstance.Resources.WindowCorner.Height + COMMON_MARGIN,
                TimersModule.ModuleInstance.Resources.WindowCorner.Width,
                TimersModule.ModuleInstance.Resources.WindowCorner.Height);

        }

        protected override void OnMouseMoved (MouseEventArgs e) {
            MouseOverTitleBar = false;
            if (this.RelativeMousePosition.Y < _titleBarBounds.Bottom)
                MouseOverTitleBar = true;

            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft (MouseEventArgs e) {
            MouseOverTitleBar = false;
            base.OnMouseLeft(e);
        }

        protected override CaptureType CapturesInput () {
            return CaptureType.Mouse | CaptureType.MouseWheel | CaptureType.Filter;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e) {
            if (MouseOverTitleBar) {
                Dragging = true;
                DragStart = Input.Mouse.Position;
            }
            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            Dragging = false;
            base.OnLeftMouseButtonReleased(e);
        }

        public void ToggleWindow () {
            if (_visible) Hide();
            else Show();
        }

        public override void Show() {
            if (_visible) return;
            this.Location = new Point(
                Math.Max(0, _location.X),
                Math.Max(0, _location.Y));

            this.Opacity = 0;
            this.Visible = true;

            _animFade.Resume();
        }

        public override void Hide() {
            if (!this.Visible) return;
            _animFade.Resume();
        }

        public override void UpdateContainer (GameTime gameTime) {
            if (Dragging) {
                var nOffset = Input.Mouse.Position - DragStart;
                Location += nOffset;
                DragStart = Input.Mouse.Position;
            }
        }

        protected void PaintWindowBackground(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this,
                TimersModule.ModuleInstance.Resources.WindowBackground,
                bounds,
                null,
                Color.White,
                0f,
                _windowBackgroundOrigin);
        }

        protected void PaintTitleBar(SpriteBatch spriteBatch, Rectangle bounds) {

            // Titlebar
            if (_mouseOver && MouseOverTitleBar) {
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarLeftActive, _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarLeftActive, _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarRightActive, _layoutRightTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarRightActive, _layoutRightTitleBarBounds);
            } else {
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarLeft, _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarLeft, _layoutLeftTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarRight, _layoutRightTitleBarBounds);
                spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.WindowTitleBarRight, _layoutRightTitleBarBounds);
            }

            // Title & Subtitle
            if (!string.IsNullOrEmpty(_title)) {
                spriteBatch.DrawStringOnCtrl(this,
                    _title,
                    Content.DefaultFont32,
                    _layoutLeftTitleBarBounds.OffsetBy(TITLE_OFFSET, 0),
                    ContentService.Colors.ColonialWhite);

                if (!string.IsNullOrEmpty(_subtitle)) {
                    spriteBatch.DrawStringOnCtrl(this,
                        _subtitle,
                        Content.DefaultFont16,
                        _layoutSubtitleBounds,
                        ContentService.Colors.ColonialWhite);
                }

            }

        }

        protected void PaintEmblem (SpriteBatch spriteBatch, Rectangle bounds) {
            if (_emblem != null) {
                spriteBatch.DrawOnCtrl(this,
                    _emblem,
                    _emblem.Bounds.Subtract(new Rectangle(_emblem.Width / 8, _emblem.Height / 4, 0, 0)));
            }
        }

        protected void PaintCorner (SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this,
                TimersModule.ModuleInstance.Resources.WindowCorner,
                _layoutWindowCornerBounds);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            PaintWindowBackground(spriteBatch, _windowBackgroundBounds.Subtract(new Rectangle(0, -4, 0, 0)));
            PaintTitleBar(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            PaintEmblem(spriteBatch, bounds);
            PaintCorner(spriteBatch, bounds);
        }

    }
}
