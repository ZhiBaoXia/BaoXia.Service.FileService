using BaoXia.Utils.Extensions;
using BaoXia.Utils;

namespace BaoXia.Service.FileService.Models
{
        public class ImageAfterFilterFileInfo
        {

                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性

                /// <summary>
                /// 滤镜处理过的图片文件路径规则。
                /// </summary>
                public string? FilePathFormatter { get; set; }

                /// <summary>
                /// 滤镜处理过的图片文件Uri规则。
                /// </summary>
                public string? FileAbsoluteUriFormatter { get; set; }

                /// <summary>
                /// WEBP图片的默认保存质量，取值范围：0-100。
                /// </summary>
                public int ImageSaveQualityForWebpDefault { get; set; }

                /// <summary>
                /// JPEG图片的默认保存质量，取值范围：0-100。
                /// </summary>
                public int ImageSaveQualityForJpegDefault { get; set; }

                #endregion


                ////////////////////////////////////////////////
                // @自身实现
                ////////////////////////////////////////////////

                #region 自身实现

                public string? GetFileAbsolutePathWithFileId(
                    string? imageOriginalUri,
                    string? imageFileExtensionName,
                    string? imageFilterName,
                    string? filePathFormatterSpecified = null)
                {
                        if (filePathFormatterSpecified == null)
                        {
                                filePathFormatterSpecified = this.FilePathFormatter;
                        }
                        return Models.FilePath.CreateAbsolutePathWithFilePathFormatter(
                                filePathFormatterSpecified,
                                0,
                                0,
                                imageFileExtensionName,
                                null,
                                (StringWithFunctionExpression.FunctionExpressionInfo functionInfo) =>
                                {
                                        string? functionResult = null;
                                        if (Models.FilePath.ImageOriginalUriHashCodeFunction.Name
                                                .EqualsIgnoreCase(functionInfo.Name))
                                        {
                                                functionResult = imageOriginalUri?.ToHashCodeByMD532();
                                        }
                                        else if (Models.FilePath.ImageSizeFunction.Name
                                                .EqualsIgnoreCase(functionInfo.Name))
                                        {
                                                functionResult = 0 + "x" + 0;
                                        }
                                        else if (Models.FilePath.ImageFilterNameFunction.Name
                                                .EqualsIgnoreCase(functionInfo.Name))
                                        {
                                                functionResult = imageFilterName;
                                        }
                                        return functionResult;
                                });
                }
                
                public string? GetFileAbsoluteUriWithFileId(
                    string? imageOriginalUri,
                    string? imageFileExtensionName,
                    string? imageFilterName,
                    string? fileAbsoluteUriFormatterSpecified = null)
                {
                        if (fileAbsoluteUriFormatterSpecified == null)
                        {
                                fileAbsoluteUriFormatterSpecified = this.FileAbsoluteUriFormatter;
                        }
                        return Models.FilePath.CreateAbsoluteUriWithFileUriFormatter(
                                fileAbsoluteUriFormatterSpecified,
                                0,
                                0,
                                imageFileExtensionName,
                                null,
                                (StringWithFunctionExpression.FunctionExpressionInfo functionInfo) =>
                                {
                                        string? functionResult = null;
                                        if (Models.FilePath.ImageOriginalUriHashCodeFunction.Name
                                                .EqualsIgnoreCase(functionInfo.Name))
                                        {
                                                functionResult = imageOriginalUri?.ToHashCodeByMD532();
                                        }
                                        else if(Models.FilePath.ImageSizeFunction.Name
                                                .EqualsIgnoreCase(functionInfo.Name))
                                        {
                                                functionResult = 0 + "x" + 0;
                                        }
                                        else if (Models.FilePath.ImageFilterNameFunction.Name
                                                .EqualsIgnoreCase(functionInfo.Name))
                                        {
                                                functionResult = imageFilterName;
                                        }
                                        return functionResult;
                                });
                }

                #endregion
        }
}
