using UnityEngine;

namespace OneKeyLoot
{
    public static class UIConstants
    {
        private static Color Hex(string str)
        {
            if (ColorUtility.TryParseHtmlString(str, out var color))
            {
                return color;
            }
            return new Color(0.30f, 0.69f, 0.31f, 1f);
        }

        public static readonly Color Color2 = Hex("#4CAF4F"); // ≥2 草绿 // ≥2 초록
        public static readonly Color Color3 = Hex("#42A5F5"); // ≥3 天蓝 // ≥3 하늘
        public static readonly Color Color4 = Hex("#BA68C6"); // ≥4 柔紫 // ≥4 보라
        public static readonly Color Color5 = Hex("#BF7F33"); // ≥5 暖橙 // ≥5 주황

        // used in automatically filling value button background color
        public static readonly Color aColor0 = Hex("#FFFFFF"); // ≥0 white // ≥0 흰색
        public static readonly Color aColor1 = Hex("#7cff7c"); // ≥1 yellow green // ≥1 연두
        public static readonly Color aColor2 = Hex("#7cd5ff"); // ≥2 sky blue // ≥2 하늘
        public static readonly Color aColor3 = Hex("#d0acff"); // ≥3 lavender // ≥3 연보라
        public static readonly Color aColor4 = Hex("#ffdc24"); // ≥4 yellow // ≥4 노랑
        public static readonly Color aColor5 = Hex("#ff5858"); // ≥5 orange // ≥5 주황
        public static readonly Color aColor6 = Hex("#bb0000"); // ≥6 red // ≥6 빨강
        
        public const string LabelName = "OKL_Label";
        public const float ButtonRowSpacing = 8f;
        public const float BottomSpacerHeight = 16f;

        public const string TitleName = "OKL_Title";

        public const string QualityRowName = "OKL_Row_Quality";
        public const string QualityPanelName = "OKL_LabelPanel_Quality";
        public const string QualityTitleName = "OKL_Title_Quality";
        public const string ValueRowName = "OKL_Row_Value";
        public const string ValuePanelName = "OKL_LabelPanel_Value";
        public const string ValueTitleName = "OKL_Title_Value";
        public const string ValueWeightRowName = "OKL_Row_ValueWeight";
        public const string ValueWeightPanelName = "OKL_LabelPanel_ValueWeight";
        public const string ValueWeightTitleName = "OKL_Title_ValueWeight";

        public const int TitleFontSize = 24;
        public static readonly Color TitleColor = new(0.90f, 0.90f, 0.90f, 1f);
    }
}
