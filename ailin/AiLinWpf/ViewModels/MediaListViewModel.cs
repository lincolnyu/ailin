using AiLinLib.Media;
using AiLinWpf.Helpers;
using QSharp.Scheme.Classical.Sequential;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AiLinWpf.ViewModels
{
    public class MediaListViewModel
    {
        public MediaListViewModel(MediaRepository repository)
        {
            Repository = repository;
            PopulateList();
        }

        public MediaRepository Repository { get; }

        public ObservableCollection<MediaInfoViewModel> MediaViewModels { get; } = new ObservableCollection<MediaInfoViewModel>();

        public List<Comparison<MediaInfoViewModel>> Comparisons = new List<Comparison<MediaInfoViewModel>>();

        public static Comparison<MediaInfoViewModel> CompareTypeAscending => (x, y) => x.Type.CompareTo(y.Type);
        public static Comparison<MediaInfoViewModel> CompareTypeDescending => (x, y) => y.Type.CompareTo(x.Type);
        public static Comparison<MediaInfoViewModel> CompareTitleAscending => (x, y) => ChineseHelper.Compare(x.Title, y.Title);
        public static Comparison<MediaInfoViewModel> CompareTitleDescending => (x, y) => ChineseHelper.Compare(y.Title, x.Title);
        public static Comparison<MediaInfoViewModel> CompareDateAscending => (x, y) => x.Date.CompareTo(y.Date);
        public static Comparison<MediaInfoViewModel> CompareDateDescending => (x, y) => y.Date.CompareTo(x.Date);

        private bool AreOnSame(Comparison<MediaInfoViewModel> x, Comparison<MediaInfoViewModel> y)
            => x == y ||
            x == CompareTypeAscending && y == CompareTypeDescending || x == CompareTypeDescending && y == CompareTypeAscending ||
            x == CompareTitleAscending && y == CompareTitleDescending || x == CompareTitleDescending && y == CompareTitleAscending ||
            x == CompareDateAscending && y == CompareDateDescending || x == CompareDateDescending && y == CompareDateAscending;

        private void PopulateList()
        {
            foreach (var item in Repository.MediaList)
            {
                MediaViewModels.Add(new MediaInfoViewModel(item));
            }
        }

        public void Push(Comparison<MediaInfoViewModel> c)
        {
            for (var i = 0; i < Comparisons.Count; i++)
            {
                var x = Comparisons[i];
                if (AreOnSame(x, c))
                {
                    Comparisons.RemoveAt(i);
                    break;
                }
            }
            Comparisons.Insert(0, c);
            Sort();
        }

        private int Compare(MediaInfoViewModel x, MediaInfoViewModel y)
        {
            foreach (var c in Comparisons)
            {
                var cr = c(x, y);
                if (cr != 0) return cr;
            }
            return 0;
        }

        public void Sort()
        {
            QuickSort.Sort(MediaViewModels, Compare);
        }
    }
}
