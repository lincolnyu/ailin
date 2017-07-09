using System;
using System.Text;
using WebKit;

namespace AiLinWpf.Data
{
    public class Record
    {
        public DateTime Time;
        public int? Votes;
        public int? Rank;
        public int? Popularity;

        public void CopyFromPageInfo(PageInfo pi)
        {
            Votes = pi.Votes;
            Rank = pi.Rank;
            Popularity = pi.Popularity;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Time},");
            sb.Append(Votes != null ? $"{Votes}," : ",");
            sb.Append(Rank != null ? $"{Rank}," : ",");
            sb.Append(Popularity != null ? $"{Popularity}," : ",");
            return sb.ToString();
        }

        public static Record ReadFromLine(string line)
        {
            int? votes = null;
            int? rank = null;
            int? pop = null;
            DateTime t = default(DateTime);
            var split = line.Split(',');
            if (split.Length > 0)
            {
                if (!DateTime.TryParse(split[0], out t))
                {
                    return null;
                }
            }
            if (split.Length > 1)
            {
                if (split[1].Trim() != "")
                {
                    if (int.TryParse(split[1], out int v))
                    {
                        votes = v;
                    }
                }
            }
            if (split.Length > 2)
            {
                if (split[1].Trim() != "")
                {
                    if (int.TryParse(split[2], out int v))
                    {
                        rank = v;
                    }
                }
            }
            if (split.Length > 3)
            {
                if (split[1].Trim() != "")
                {
                    if (int.TryParse(split[3], out int v))
                    {
                        pop = v;
                    }
                }
            }
            return new Record { Time = t, Votes = votes, Rank = rank, Popularity = pop };
        }
    }

}
