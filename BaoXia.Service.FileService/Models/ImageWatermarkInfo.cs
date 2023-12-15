using System;
using BaoXia.Utils.Extensions;

namespace BaoXia.Service.FileService.Models
{
        public class ImageWatermarkInfo
        {

                ////////////////////////////////////////////////
                // @静态常量
                ////////////////////////////////////////////////

                #region 静态常量

                public enum AlignType
                {
                        Left,
                        Right,

                        Center,

                        Top,
                        Bottom
                }

                public enum LayoutType
                {
                        HorizontalLayout,

                        VerticalLayout,
                }

                #endregion


                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性


                public int MaxImageWidth { get; set; }

                public int MaxImageHeight { get; set; }



                public string? WatermarkImageFilePath { get; set; }

                public LayoutType WatermarkLayout { get; set; }

                public string? WatermarkLayoutName
                {
                        get
                        {
                                return EnumExtension.NameOfEnumValue(this.WatermarkLayout);
                        }

                        set
                        {
                                this.WatermarkLayout
                                        = EnumExtension.ValueOfEnumValueName(
                                                value,
                                                LayoutType.HorizontalLayout);
                        }
                }

                public AlignType WatermarkLayoutAlignType { get; set; }

                public string? WatermarkLayoutAlignTypeName
                {
                        get
                        {
                                return EnumExtension.NameOfEnumValue(this.WatermarkLayoutAlignType);
                        }

                        set
                        {
                                this.WatermarkLayoutAlignType
                                        = EnumExtension.ValueOfEnumValueName(
                                                value,
                                                AlignType.Center);
                        }
                }

                public bool IsWatermarkDrawImageFirst { get; set; }

                public int WatermarkImageAndCaptionSeparatorSize { get; set; }

                public string? WatermarkCaptionFontFileName { get; set; }

                public float WatermarkCaptionFontSize { get; set; }

                public string? WatermarkCaptionColor { get; set; }
                
                public int WatermarkCaptionBorderSize { get; set; }

                public string? WatermarkCaptionBorderColor { get; set; }

                public AlignType HorizontalAlignType { get; set; }

                public AlignType VerticalAlignType { get; set; }

                public string? AlignTypeNames
                {
                        get
                        {
                                var horizontalAlignTypeName
                                    = EnumExtension.NameOfEnumValue(
                                        this.HorizontalAlignType);
                                var verticalAlignTypeName
                                    = EnumExtension.NameOfEnumValue(
                                        this.VerticalAlignType);

                                return horizontalAlignTypeName
                                        + " "
                                        + verticalAlignTypeName;
                        }
                        set
                        {
                                var horizontalAlignType = AlignType.Left;
                                var verticalAlignType = AlignType.Top;
                                var alignTypeNames = value?.Split(
                                        " ",
                                        StringSplitOptions.RemoveEmptyEntries
                                        | StringSplitOptions.TrimEntries);
                                if (alignTypeNames != null)
                                {
                                        if (alignTypeNames.Length > 0)
                                        {
                                                horizontalAlignType = EnumExtension.ValueOfEnumValueName(
                                                        alignTypeNames[0],
                                                        AlignType.Left);
                                                if (alignTypeNames.Length > 1)
                                                {
                                                        verticalAlignType = EnumExtension.ValueOfEnumValueName(
                                                                alignTypeNames[1],
                                                                AlignType.Top);
                                                }
                                        }
                                }
                                this.HorizontalAlignType = horizontalAlignType;
                                this.VerticalAlignType = verticalAlignType;
                        }
                }

                public int MarginLeft { get; set; }

                public int MarginTop { get; set; }

                public int MarginRight { get; set; }

                public int MarginBottom { get; set; }

                public string? MarginValues
                {
                        get
                        {
                                var marginValues
                                        = this.MarginLeft
                                        + " "
                                        + this.MarginTop
                                        + " "
                                        + this.MarginRight
                                        + " "
                                        + this.MarginBottom;
                                return marginValues;
                        }
                        set
                        {
                                var marginTop = 0;
                                var marginRight = 0;
                                var marginBottom = 0;
                                var marginLeft = 0;
                                var marginValues = value?.Split(
                                        " ",
                                        StringSplitOptions.RemoveEmptyEntries
                                        | StringSplitOptions.TrimEntries);
                                if (marginValues != null)
                                {
                                        if (marginValues.Length > 0)
                                        {
                                                _ = int.TryParse(marginValues[0], out marginTop);
                                                if (marginValues.Length > 1)
                                                {
                                                        _ = int.TryParse(marginValues[1], out marginRight);
                                                        if (marginValues.Length > 2)
                                                        {
                                                                _ = int.TryParse(marginValues[2], out marginBottom);
                                                                if (marginValues.Length > 3)
                                                                {
                                                                        _ = int.TryParse(marginValues[3], out marginLeft);
                                                                }
                                                        }
                                                }
                                        }
                                }
                                this.MarginTop = marginTop;
                                this.MarginRight = marginRight;
                                this.MarginBottom = marginBottom;
                                this.MarginLeft = marginLeft;
                        }
                }

                public bool IsValid
                {
                        get
                        {
                                if (this.WatermarkImageFilePath?.Length > 0)
                                {
                                        return true;
                                }
                                return false;
                        }
                }

                #endregion
        }
}