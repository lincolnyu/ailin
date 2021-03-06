﻿using System.Windows;

namespace AiLinWpf.ViewModels.SourcesRemarks
{
    public class CommonSourceRemarksViewModel
    {
        public const string CommonContact = "mailto:linc.yu@outlook.com?subject=爱琳投票助手影视资源建议";

        public string PreContactText { get; private set; }
        public string ContactText { get; private set; }
        public string PostContactText { get; private set; }
        public string Contact { get; set; } = CommonContact;
        public Thickness Margin { get; set; }

        public static CommonSourceRemarksViewModel TryParse(string remarks)
        {
            CommonSourceRemarksViewModel gm = null;
            if (remarks == "unavailable")
            {
                gm = new CommonSourceRemarksViewModel
                {
                    PreContactText = "资源暂无。如有线索烦请",
                    ContactText = "告知作者",
                    PostContactText = "。"
                };
            }
            else if (!string.IsNullOrWhiteSpace(remarks))
            {
                var index = remarks.IndexOf("告知作者");
                gm = new CommonSourceRemarksViewModel();
                if (index < 0)
                {
                    gm.PreContactText = remarks;
                }
                else
                {
                    gm.PreContactText = remarks.Substring(0, index);
                    gm.ContactText = "告知作者";
                    gm.PostContactText = remarks.Substring(index + "告知作者".Length);
                }
            }
            return gm;
        }
    }
}
