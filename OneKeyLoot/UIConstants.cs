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
