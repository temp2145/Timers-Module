using System;
using System.ComponentModel;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using temp.Timers.Controls.Effects;

namespace temp.Timers.Controls {

    public class AlertPanel : FlowPanel {

        private const int ALERTPANEL_WIDTH = 320;
        private const int ALERTPANEL_HEIGHT = 64;

        private AsyncTexture2D _icon;
        private float _maxFill;
        private float _currentFill;
        private Color _fillColor = Color.LightGray;
        private string _text;
        private Color _textColor = Color.White;
        private string _timerText;
        private Color _timerTextColor = Color.White;
        private readonly SimpleScrollingHighlightEffect _scrollEffect;

        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public string TimerText {
            get => _timerText;
            set => SetProperty(ref _timerText, value);
        }

        public Color TimerTextColor {
            get => _timerTextColor;
            set => SetProperty(ref _timerTextColor, value);
        }

        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public float MaxFill {
            get => _maxFill;
            set => SetProperty(ref _maxFill, value);
        }

        public float CurrentFill {
            get => _currentFill;
            set {
                if (SetProperty(ref _currentFill, Math.Min(value, _maxFill))) {
                    _animFill?.Cancel();
                    _animFill = null;
                    _animFill = Animation.Tweener.Tween(this, 
                        new { DisplayedFill = _currentFill }, 
                        TimersModule.ModuleInstance.Resources.TICKINTERVAL, 
                        0, false);
                }
                if (_currentFill >= _maxFill)
                    _scrollEffect.Enable();
            }
        }

        public Color FillColor {
            get => _fillColor;
            set => SetProperty(ref _fillColor, value);
        }

        /// <summary>
        /// Do not directly manipulate this property.  It is only public because the animation library requires it to be public.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float DisplayedFill { get; set; } = 0;

        private Glide.Tween _animFill;

        public AlertPanel () {
            this.Size = new Point(ALERTPANEL_WIDTH, ALERTPANEL_HEIGHT);
            _scrollEffect = new SimpleScrollingHighlightEffect(this) {
                Enabled = false
            };
            this.EffectBehind = _scrollEffect;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        public override void RecalculateLayout() { }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {

            // TODO: Move all calculations into RecalculateLayout()

            bool minimalistMode = (TimersModule.ModuleInstance.AlertMode == AlertPanelMode.Minimalist);

            // Draw background
            if (!minimalistMode) {
                spriteBatch.DrawOnCtrl(this,
                                       ContentService.Textures.Pixel,
                                       bounds,
                                       Color.Black * 0.2f);
            }

            int iconSize = _size.Y;
            float fillPercent = (_maxFill > 0) ? (_currentFill / _maxFill) : 0f;
            float fillSpace = iconSize * fillPercent;

            // Handle fill 
            if (_maxFill > 0 && !minimalistMode) {

                // Draw icon twice
                if (_icon != null) {

                    float localIconFill = (fillSpace - iconSize / 2f + 32) / 64;

                    // Icon above the fill
                    if (localIconFill < 1) {
                        spriteBatch.DrawOnCtrl(this,
                                               _icon,
                                               new Rectangle(
                                                             iconSize / 2 - 64 / 2,
                                                             iconSize / 2 - 64 / 2,
                                                             64,
                                                             64 - (int)(64 * localIconFill)
                                                            ),
                                               new Rectangle(0, 0, 64, 64 - (int)(64 * localIconFill)),
                                               Color.DarkGray * 0.4f);
                    }

                    // Icon below the fill
                    if (localIconFill > 0) {
                        spriteBatch.DrawOnCtrl(
                                               this,
                                               _icon,
                                               new Rectangle(
                                                             iconSize / 2 - 64 / 2,
                                                             iconSize / 2 - 64 / 2 + (64 - (int)(localIconFill * 64)),
                                                             64,
                                                             (int)(localIconFill * 64)
                                                            ),
                                               new Rectangle(0, 64 - (int)(localIconFill * 64), 64, (int)(localIconFill * 64))
                                              );
                    }

                }

                if (_currentFill > 0) {
                    // Draw the fill
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, (int)(iconSize - fillSpace), iconSize, (int)(fillSpace)), _fillColor * 0.3f);

                    // Only show the fill crest if we aren't full
                    if (fillPercent < 0.99f)
                        spriteBatch.DrawOnCtrl(this, TimersModule.ModuleInstance.Resources.TextureFillCrest, new Rectangle(0, iconSize - (int)(fillSpace), iconSize, iconSize));
                }

            } else if (_icon != null && !minimalistMode) {
                // Draw icon without any fill effects
                spriteBatch.DrawOnCtrl(
                                       this,
                                       _icon,
                                       new Rectangle(iconSize / 2 - 64 / 2,
                                                     iconSize / 2 - 64 / 2,
                                                     64,
                                                     64));
            }

            // Draw icon vignette (draw with or without the icon to keep a consistent look)
            if (!minimalistMode) {
                spriteBatch.DrawOnCtrl(this,
                                        TimersModule.ModuleInstance.Resources.TextureVignette,
                                        new Rectangle(0, 0, iconSize, iconSize));
            }

            // Draw time text
            if (!string.IsNullOrEmpty(_timerText))
                spriteBatch.DrawStringOnCtrl(this, $"{this._timerText}", Content.DefaultFont32, new Rectangle(0, 0, iconSize, (int)(iconSize * 0.99f)), this.TimerTextColor, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);

            // Draw alert text
            spriteBatch.DrawStringOnCtrl(this, _text, TimersModule.ModuleInstance.Resources.Font, new Rectangle(iconSize + 16, 0, _size.X - iconSize - 35, this.Height), this.TextColor, true, true);
        }

        public void Dispose() {
            _icon.Dispose();
            base.Dispose();
        }

    }
}
