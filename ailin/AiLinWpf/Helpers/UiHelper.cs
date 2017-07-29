using AiLinWpf.Styles;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

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
                    var s = tb.Text;
                    var index = s.IndexOf(text);
                    if (index >= 0)
                    {
                        var ntb = new TextBlock
                        {
                            FontSize = tb.FontSize,
                            FontWeight = tb.FontWeight,
                            FontStyle = tb.FontStyle,
                            Foreground = tb.Foreground,
                            Background = tb.Background
                        };
                        var lastIndex = 0;
                        do
                        {
                            ntb.Inlines.Add(s.Substring(lastIndex, index - lastIndex));
                            var run = new Run(s.Substring(index, text.Length))
                            {
                                Background = Coloring.YellowBrush
                            };
                            ntb.Inlines.Add(run);
                            lastIndex = index + text.Length;
                            index = tb.Text.IndexOf(text, lastIndex);
                        } while (index >= 0);
                        ntb.Inlines.Add(s.Substring(lastIndex));
                        Replace(tb, ntb);
                        System.Diagnostics.Trace.WriteLine($"New text is {ntb.Text}");
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
