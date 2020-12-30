using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace temp.Timers.Controls {
    class AlertWindow : WindowBase {

        public AlertWindow() : base() {
            this.ZIndex = Screen.TOOLWINDOW_BASEZINDEX;
            Texture2D bg = TimersModule.ModuleInstance.ContentsManager.GetTexture(@"textures\1032335.png");
            ConstructWindow(bg,
                            new Vector2(0, 32),
                            new Rectangle(0, 0, 400, 800),
                            Thickness.Zero,
                            32,
                            false);
            this.ContentRegion = new Rectangle(0, 32, _size.X, _size.Y - 32);

            this.Show();
        }

    }
}
