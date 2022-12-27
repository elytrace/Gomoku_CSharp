using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static Gomoku.Game;

namespace Gomoku
{
    public static class UtilityTool
    {
        public static Bitmap ScaleImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static void ChangeTurn()
        {
            currentTurn = 3 - currentTurn;
            turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @" turn";
            pb.Value = pb.Maximum;
        }
    }
}