using System;
using System.Collections.Generic;

namespace AiLinWpf.Helpers
{
    public static class ChineseHelper
    {
        public static Dictionary<string, string> CharToPinyin = new Dictionary<string, string>
        {
            {"啊","a1"},
            {"爱","ai4"},
            {"案","an4"},
            {"白","bai2"},
            {"北","bei3"},
            {"辈","bei4"},
            {"本","ben3"},
            {"遍","bian4"},
            {"别","bie2"},
            {"抱","bao4"},
            {"不","bu4"},
            {"布","bu4"},
            {"草","cao3"},
            {"长","chang2"},
            {"朝","chao2"},
            {"初","chu1"},
            {"触","chu4"},
            {"传","chuan2"},
            {"春","chun1"},
            {"次","ci4"},
            {"大","da4"},
            {"代","dai4"},
            {"到","dao4"},
            {"的","de"},
            {"等","deng3"},
            {"底","di3"},
            {"地","di4"},
            {"第","di4"},
            {"爹","die1"},
            {"独","du2"},
            {"对","dui4"},
            {"尔","er2"},
            {"二","er4"},
            {"方","fang1"},
            {"房","fang2"},
            {"放","fang4"},
            {"非","fei1"},
            {"风","feng1"},
            {"峰","feng1"},
            {"傅", "fu4"},
            {"港","gang3"},
            {"高","gao1"},
            {"更","geng4"},
            {"共","gong4"},
            {"贾","gu1"},
            {"鼓","gu3"},
            {"关","guan1"},
            {"光","guang1"},
            {"国","guo2"},
            {"果","guo3"},
            {"过","guo4"},
            {"海","hai3"},
            {"和","he2"},
            {"贺","he4"},
            {"黑","hei1"},
            {"红","hong2"},
            {"洪","hong2"},
            {"后","hou4"},
            {"花","hua1"},
            {"桦","hua4"},
            {"怀","huai2"},
            {"会","hui4"},
            {"婚","hun1"},
            {"魂","hun2"},
            {"火","huo3"},
            {"记","ji4"},
            {"家","jia1"},
            {"将","jiang1"},
            {"接","jie1"},
            {"界","jie4"},
            {"金","jin1"},
            {"径","jin4"},
            {"静","jing4"},
            {"剧","ju4"},
            {"军","jun1"},
            {"开","kai1"},
            {"凯","kai3"},
            {"可","ke3"},
            {"克","ke4"},
            {"口","kou3"},
            {"拉","la1"},
            {"栏","lan2"},
            {"梨","li2"},
            {"离","li2"},
            {"恋","lian4"},
            {"临","lin2"},
            {"柳","liu3"},
            {"六","liu4"},
            {"龙","long2"},
            {"路","lu4"},
            {"论","lun4"},
            {"锣","luo2"},
            {"骆","luo4"},
            {"旅","lv3"},
            {"妈","ma1"},
            {"马","ma3"},
            {"么","me"},
            {"们","men"},
            {"门","men2"},
            {"密","mi4"},
            {"民","min2"},
            {"命","min4"},
            {"那","na4"},
            {"你","ni3"},
            {"年","nian2"},
            {"娘","niang2"},
            {"女","nv3"},
            {"牌","pai2"},
            {"叛","pan4"},
            {"配","pei4"},
            {"披","pi1"},
            {"票","piao4"},
            {"平","ping3"},
            {"奇","qi2"},
            {"弃","qi4"},
            {"腔","qiang1"},
            {"抢","qiang3"},
            {"亲","qin1"},
            {"秦","qin2"},
            {"青","qing1"},
            {"情","qing2"},
            {"群","qun2"},
            {"让","rang4"},
            {"人","ren2"},
            {"荣","rong2"},
            {"山","shan1"},
            {"商","shang1"},
            {"上","shang4"},
            {"烧","shao1"},
            {"谁","shei2"},
            {"什","shen2"},
            {"生","sheng1"},
            {"十","shi2"},
            {"使","shi3"},
            {"是","shi4"},
            {"事","shi4"},
            {"思","si1"},
            {"撕","si1"},
            {"胎","tai1"},
            {"台","tai2"},
            {"堂","tang2"},
            {"逃","tao2"},
            {"天","tian1"},
            {"跳","tiao4"},
            {"庭","ting2"},
            {"桐","tong2"},
            {"驼","tuo2"},
            {"弯","wan1"},
            {"卫","wei4"},
            {"我","wo3"},
            {"无","wu2"},
            {"舞","wu3"},
            {"西","xi1"},
            {"戏","xi4"},
            {"下","xia4"},
            {"险","xian3"},
            {"相","xiang1"},
            {"巷","xiang4"},
            {"肖","xiao1"},
            {"硝","xiao1"},
            {"小","xiao3"},
            {"心","xin1"},
            {"兄","xiong1"},
            {"雄","xiong2"},
            {"旋","xuan2"},
            {"选","xuan3"},
            {"雪","xue3"},
            {"烟","yan1"},
            {"阳","yang2"},
            {"要","yao4"},
            {"也","ye3"},
            {"夜","ye4"},
            {"依","yi1"},
            {"阴","yin1"},
            {"英","ying1"},
            {"影","ying3"},
            {"永","yong3"},
            {"游","you2"},
            {"有","you3"},
            {"与","yu3"},
            {"雨","yu3"},
            {"浴","yu4"},
            {"园","yuan2"},
            {"远","yuan3"},
            {"月","yue4"},
            {"云","yun2"},
            {"运","yun4"},
            {"在","zai4"},
            {"遭","zao1"},
            {"择","ze2"},
            {"栅","zha4"},
            {"斋","zhai1"},
            {"战","zhan4"},
            {"者","zhe3"},
            {"争","zheng1"},
            {"之","zhi1"},
            {"中","zhong1"},
            {"重","zhong4"},
            {"住","zhu4"},
            {"子","zi3"},
            {"走","zou3"},
            {"族","zu2"},
            {"组","zu3"},
            {"昨","zuo2"},
            {"做","zuo4"}
        };

        public static int Compare(string a, string b)
        {
            var len = Math.Min(a.Length, b.Length);
            for (var i = 0; i < len; i++)
            {
                var c1 = a[i].ToString();
                var c2 = b[i].ToString();
                var isChn1 = CharToPinyin.TryGetValue(c1, out var i1);
                var isChn2 = CharToPinyin.TryGetValue(c2, out var i2);
                int cmp;
                if (isChn1 && isChn2)
                {
                    cmp = i1.CompareTo(i2);
                    if (cmp != 0) return cmp;
                    continue;
                }
                if (!isChn1)
                {
                    System.Diagnostics.Debug.WriteLine($"{c1}");
                }
                if (!isChn2)
                {
                    System.Diagnostics.Debug.WriteLine($"{c2}");
                }
                if (isChn1)
                {
                    return 1;
                }
                if (isChn2)
                {
                    return -1;
                }
                cmp = c1.CompareTo(c2);
                if (cmp != 0) return cmp;
            }
            return a.Length.CompareTo(b.Length);
        }
    }
}
