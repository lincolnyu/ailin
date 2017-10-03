using AiLinWpf.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace AiLinWpf.ViewModels.SourcesRemarks
{
    public class XamlSourceRemarksViewModel
    {
        public FrameworkElement Element { get; set; }
        public Thickness Margin { get; set; }

        public static XamlSourceRemarksViewModel TryParseBlock(string remarks)
        {
            try
            {
                var sb = new StringBuilder("<WrapPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
                sb.Append(remarks);
                sb.Append("</WrapPanel>");
                var wp = (WrapPanel)XamlReader.Parse(sb.ToString());
                TrySetStyles(wp);
                var xsrvm = new XamlSourceRemarksViewModel { Element = wp };
                return xsrvm;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void TrySetStyles(WrapPanel wp)
        {
            try
            {
                var descStyle = (Style)Application.Current.MainWindow.FindResource("Description");
                var hylStyle = (Style)Application.Current.MainWindow.FindResource("NormalHyperlink");
                var allTbs = wp.FindAll(new Type[] { typeof(TextBlock) });
                var allHls = wp.FindAll(new Type[] { typeof(Hyperlink) });
                foreach (var tb in allTbs.Cast<TextBlock>())
                {
                    tb.Style = descStyle;
                }
                foreach (var hl in allHls.Cast<Hyperlink>())
                {
                    hl.Style = hylStyle;
                }
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
            }
        }
    }
}
