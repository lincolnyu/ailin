using System.Windows;

namespace AiLinWpf.Styles
{
    public static class Layouts
    {
        public const int StandardTrackItemMarginThickness = 3;
        public const int StandardSeparationMarginThickness = 10;

        public static Thickness GenerateLeftJustified(int after)
            => GenerateHorizontalMargin(0, after);

        public static Thickness GenerateHorizontalMargin(int before, int after)
            => new Thickness(before, 0, after, 0);
    }
}
