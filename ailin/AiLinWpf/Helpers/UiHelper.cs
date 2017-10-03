using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using AiLinWpf.Styles;

namespace AiLinWpf.Helpers
{
    public static class UiHelper
    {
        public static Panel GetFirstPanelFromDatabound(this DependencyObject element)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is Panel p)
                {
                    return p;
                }
                var p2 = child.GetFirstPanelFromDatabound();
                if (p2 != null)
                {
                    return p2;
                }
            }
            return null;
        }

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

        public static string GetText(this InlineCollection ic)
        {
            var sb = new StringBuilder();
            foreach (var run in ic.OfType<Run>())
            {
                sb.Append(run.Text);
            }
            return sb.ToString();
        }

        public static IEnumerable<Tuple<FrameworkElement, FrameworkElement>> 
            Highlight(this ICollection<FrameworkElement> tfes, string text)
        {
            foreach (var tfe in tfes)
            {
                if (tfe is TextBlock tb)
                {
                    string s;
                    var hl = tb.Inlines.FirstInline as Hyperlink;
                    if (hl != null)
                    {
                        s = hl.Inlines.GetText();
                    }
                    else
                    {
                        s = tb.Text;
                    }

                    var index = s.IndexOf(text);
                    if (index >= 0)
                    {
                        var ntb = new TextBlock
                        {
                            FontSize = tb.FontSize,
                            FontWeight = tb.FontWeight,
                            FontStyle = tb.FontStyle,
                            Foreground = tb.Foreground,
                            Background = tb.Background,
                            VerticalAlignment = tb.VerticalAlignment,
                            Margin = tb.Margin
                        };
                        InlineCollection inlines;
                        if (hl != null)
                        {
                            var nhl = new Hyperlink()
                            {
                                NavigateUri = hl.NavigateUri,
                                Style = hl.Style
                            };
                            ntb.Inlines.Add(nhl);
                            inlines = nhl.Inlines;
                        }
                        else
                        {
                            inlines = ntb.Inlines;
                        }
                        var lastIndex = 0;
                        do
                        {
                            inlines.Add(s.Substring(lastIndex, index - lastIndex));
                            var run = new Run(s.Substring(index, text.Length))
                            {
                                Background = Coloring.YellowBrush
                            };
                            inlines.Add(run);
                            lastIndex = index + text.Length;
                            index = s.IndexOf(text, lastIndex);
                        } while (index >= 0);

                        inlines.Add(s.Substring(lastIndex));
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
