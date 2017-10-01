using System.Windows;

namespace AiLinWpf.ViewModels.SourcesMissing
{
    public class GeneralMissingViewModel
    {
        public const string CommonContact = "mailto:linc.yu@outlook.com?subject=爱琳投票助手影视资源建议";

        private static GeneralMissingViewModel _common;

        public string PreContactText { get; private set; }
        public string ContactText { get; private set; }
        public string PostContactText { get; private set; }
        public string Contact { get; set; } = CommonContact;
        public Thickness Margin { get; set; }

        public static GeneralMissingViewModel GenerateCommon()
        {
            var gm = new GeneralMissingViewModel
            {
                PreContactText = "资源暂无。如有线索烦请",
                ContactText = "告知作者",
                PostContactText = "。"
            };
            return gm;
        }

        public static GeneralMissingViewModel Parse(string missing)
        {
            var index = missing.IndexOf("告知作者");
            var gm = new GeneralMissingViewModel();
            if (index < 0)
            {
                gm.PreContactText = missing;
            }
            else
            {
                gm.PreContactText = missing.Substring(0, index);
                gm.ContactText = "告知作者";
                gm.PostContactText = missing.Substring(index + "告知作者".Length);
            }
            return gm;
        }
    }
}
