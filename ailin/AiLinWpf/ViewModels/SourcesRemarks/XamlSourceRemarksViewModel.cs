using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AiLinWpf.ViewModels.SourcesRemarks
{
    public class XamlSourceRemarksViewModel
    {
        public FrameworkElement Element { get; set; }

        public static XamlSourceRemarksViewModel TryParseBlock(string remarks)
        {
            try
            {
                var sb = new StringBuilder("<WrapPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
                sb.Append(remarks);
                sb.Append("</WrapPanel>");
                var wp = (WrapPanel)XamlReader.Parse(sb.ToString());
                var xsrvm = new XamlSourceRemarksViewModel { Element = wp };
                return xsrvm;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
