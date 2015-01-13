using WalkCompanion.Resources;
using Microsoft.Phone.Shell;
using System;
using System.Windows.Media;

namespace WalkCompanion.Utils
{
    public class TileUpdate
    {
        private static Color defaultColor = Colors.Transparent;
        private static Uri defaultSmallIcon = new Uri(@"Assets\Tiles\IconicTileSmall.png", UriKind.Relative);
        private static Uri defaultIcon = new Uri(@"Assets\Tiles\IconicTileMediumLarge.png", UriKind.Relative);

        public static void UpdateIconicTile(string title, int count, string wideContent1, string wideContent2, string wideContent3, Color? bgColor = null, Uri smallIcon = null, Uri icon = null)
        {
            IconicTileData TileData = new IconicTileData
            {
                Title = title,
                Count = count,
                WideContent1 = wideContent1,
                WideContent2 = wideContent2,
                WideContent3 = wideContent3,
                SmallIconImage = smallIcon != null ? smallIcon : defaultSmallIcon,
                IconImage = icon != null ? icon : defaultIcon,
                BackgroundColor = bgColor != null && bgColor.HasValue ? bgColor.Value : defaultColor
            };

            foreach (ShellTile tile in ShellTile.ActiveTiles)
            {
                tile.Update(TileData);
            }
        }

        public static void ClearTile()
        {
            UpdateIconicTile(AppResources.ApplicationTitle, 0, string.Empty, string.Empty, string.Empty);
        }
    }
}