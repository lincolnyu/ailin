using AiLinWpf.Styles;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AiLinWpf.Helpers
{
    public static class UiHelper
    {
        public static IEnumerable<FrameworkElement> GetAllTexts(this Panel panel)
        {
            foreach (var c in panel.Children)
            {
                if (c is TextBoxBase tbb)
                {
                    yield return tbb;
                }
                else if (c is TextBlock tb)
                {
                    yield return tb;
                }
                else if (c is Panel p)
                {
                    var tbs = GetAllTexts(p);
                    foreach (var t in tbs)
                    {
                        yield return t;
                    }
                }
            }
        }

        public static void Replace(FrameworkElement oldElem, FrameworkElement newElem)
        {
            var parent = oldElem.Parent;
            if (parent is Panel panel)
            {
                var index = panel.Children.IndexOf(oldElem);
                panel.Children.RemoveAt(index);
                panel.Children.Insert(index, newElem);
            }
        }

        public static IEnumerable<Tuple<FrameworkElement, FrameworkElement>> 
            Highlight(this ICollection<FrameworkElement> tfes, string text)
        {
            foreach (var tfe in tfes)
            {
                if (tfe is TextBlock tb)
                {
                    if (tb.Text.Contains(text))
                    {
                        var ntb = new TextBlock
                        {
                            FontSize = tb.FontSize,
                            FontWeight = tb.FontWeight,
                            FontStyle = tb.FontStyle,
                            Foreground = tb.Foreground,
                            Background = Coloring.YellowBrush
                        };
                        ntb.Text = tb.Text;
                        Replace(tb, ntb);
                        yield return new Tuple<FrameworkElement, FrameworkElement>(tb, ntb);
                    }
                }
                else if (tfes is RichTextBox rtb)
                {
                    // TODO implement it
                }
            }
        }

        public static void DeHighlight(this IEnumerable<Tuple<FrameworkElement, FrameworkElement>> highlighted)
        {
            foreach (var pair in highlighted)
            {
                Replace(pair.Item2, pair.Item1);
            }
        }
    }
}
