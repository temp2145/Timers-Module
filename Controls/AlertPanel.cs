using System;
using System.ComponentModel;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using temp.Timers.Models;
using temp.Timers.Controls.Effects;

namespace temp.Timers.Controls {

    public class AlertPanel : FlowPanel {

        private const int DEFAULT_EVENTSUMMARY_WIDTH = 320;
        private const int DEFAULT_EVENTSUMMARY_HEIGHT = 64;

        #region Load Static

        private static readonly Texture2D _textureFillCrest;
        private static readonly Texture2D _textureVignette;
        private static readonly BitmapFont _font;

        static AlertPanel() {
            _textureFillCrest = Content.GetTexture(@"controls/detailsbutton/605004");
            _textureVignette = Content.GetTexture(@"controls/detailsbutton/605003");
            _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size22,
                ContentService.FontStyle.Regular);
        }

        #endregion

        private AsyncTexture2D _icon;
        private float _maxFill;
        private float _currentFill;
        private string _timerText;
        private string _text;
        private Color _fillColor = Color.LightGray;
        private readonly SimpleScrollingHighlightEffect _scrollEffect;

        /// <summary>
        /// The text displayed on the right side of the <see cref="DetailsButton"/>.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string TimerText {
            get => _timerText;
            set => SetProperty(ref _timerText, value);
        }

        /// <summary>
        /// The icon to display on the left side of the <see cref="DetailsButton"/>.
        /// </summary>
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        /// <summary>
        /// The maximum the <see cref="CurrentFill"/> can be set to.  If <see cref="ShowVignette"/>
        /// is true,then setting this value to a value greater than 0 will enable the fill.
        /// </summary>
        public float MaxFill {
            get => _maxFill;
            set => SetProperty(ref _maxFill, value);
        }

        /// <summary>
        /// The current fill progress.  The maximum value is clamped to <see cref="MaxFill"/>.
        /// </summary>
        public float CurrentFill {
            get => _currentFill;
            set {
                if (SetProperty(ref _currentFill, Math.Min(value, _maxFill))) {
                    _animFill?.Cancel();
                    _animFill = null;
                    _animFill = Animation.Tweener.Tween(this, new { DisplayedFill = _currentFill }, Encounter.TICKINTERVAL, 0, false);
                }
                if (_currentFill >= _maxFill) {
                    _scrollEffect.Enable();
                }
            }
        }

        /// <summary>
        /// Do not directly manipulate this property.  It is only public because the animation library requires it to be public.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float DisplayedFill { get; set; } = 0;

        /// <summary>
        /// The <see cref="Color"/> of the fill.
        /// </summary>
        public Color FillColor {
            get => _fillColor;
            set => SetProperty(ref _fillColor, value);
        }

        private Glide.Tween _animFill;

        public AlertPanel() {
            this.Size = new Point(DEFAULT_EVENTSUMMARY_WIDTH, DEFAULT_EVENTSUMMARY_HEIGHT);

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

            // Draw background
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   bounds,
                                   Color.Black * 0.2f);

            int iconSize = _size.Y;

            float fillPercent = _maxFill > 0
                                    ? _currentFill / _maxFill
                                    : 0f;

            float fillSpace = iconSize * fillPercent;

            /*** Handle fill ***/
            if (_maxFill > 0) {

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
                        spriteBatch.DrawOnCtrl(this, _textureFillCrest, new Rectangle(0, iconSize - (int)(fillSpace), iconSize, iconSize));
                }

                if (_timerText != null)
                    spriteBatch.DrawStringOnCtrl(this, $"{this._timerText}", Content.DefaultFont32, new Rectangle(0, 0, iconSize, (int)(iconSize * 0.99f)), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);
            
            } else if (_icon != null) {
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
            spriteBatch.DrawOnCtrl(this,
                                    _textureVignette,
                                    new Rectangle(0, 0, iconSize, iconSize));

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, _text, _font, new Rectangle(iconSize + 16, 0, _size.X - iconSize - 35, this.Height), Color.White, true, true);
        }

    }
}
