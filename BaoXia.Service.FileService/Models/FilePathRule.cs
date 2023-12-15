using BaoXia.Utils;
using BaoXia.Utils.Extensions;
using BaoXia.Service.FileService.ConfigFiles;
using BaoXia.Service.FileService.ViewModels;

namespace BaoXia.Service.FileService.Models
{
	public class FilePathRule
	{
		/// <summary>
		/// 对应文件的扩展名数组，匹配规则为“或”。
		/// </summary>
		public string[]? FileExtensionNames { get; set; }

		/// <summary>
		/// 对应文件的标签数组，匹配规则为“且”。
		/// </summary>
		public string[]? FileTags { get; set; }

		public int FileTagsCount
		{
			get
			{
				if (FileTags != null)
				{
					return FileTags.Length;
				}
				return 0;
			}
		}

		/// <summary>
		/// 对应上传文件后的路径（绝对，或相对）格式字符串。
		/// </summary>
		public string? FilePathFormatter { get; set; }

		public string? GetFileAbsolutePathWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight,
		    string? filePathFormatterSpecified = null)
		{
			if (filePathFormatterSpecified == null)
			{
				filePathFormatterSpecified = this.FilePathFormatter;
			}
			return Models.FilePath.CreateAbsolutePathWithFilePathFormatter(
				filePathFormatterSpecified,
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				(StringWithFunctionExpression.FunctionExpressionInfo functionInfo) =>
				{
					string? functionResult = null;
					if (Models.FilePath.ImageSizeFunction.Name
						.EqualsIgnoreCase(functionInfo.Name))
					{
						functionResult = imageWidth + "x" + imageHeight;
					}
					return functionResult;
				});
		}

		/// <summary>
		/// 对应上传文件后的Uri格式字符串。
		/// </summary>
		public string? FileAbsoluteUriFormatter { get; set; }

		public string? GetFileAbsoluteUriWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight,
		    string? fileAbsoluteUriFormatterSpecified = null)
		{
			if (fileAbsoluteUriFormatterSpecified == null)
			{
				fileAbsoluteUriFormatterSpecified = this.FileAbsoluteUriFormatter;
			}
			return Models.FilePath.CreateAbsoluteUriWithFileUriFormatter(
				fileAbsoluteUriFormatterSpecified,
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				(StringWithFunctionExpression.FunctionExpressionInfo functionInfo) =>
				{
					string? functionResult = null;
					if (Models.FilePath.ImageSizeFunction.Name
						.EqualsIgnoreCase(functionInfo.Name))
					{
						functionResult = imageWidth + "x" + imageHeight;
					}
					return functionResult;
				});
		}


		////////////////////////////////////////////////
		// @原始图片相关属性。
		////////////////////////////////////////////////

		/// <summary>
		/// 内容图片的最大尺寸。
		/// </summary>
		public ImageSize? ImageSizeMax { get; set; }

		/// <summary>
		/// 原始图片除GIF图外的保存格式。
		/// </summary>
		public string? ImageSaveFormatExceptGifDefault { get; set; }

		/// <summary>
		/// WEBP图片的默认保存质量，取值范围：0-100。
		/// </summary>
		public int ImageSaveQualityForWebpDefault { get; set; }

		/// <summary>
		/// JPEG图片的默认保存质量，取值范围：0-100。
		/// </summary>
		public int ImageSaveQualityForJpegDefault { get; set; }

		////////////////////////////////////////////////
		// @列表图片相关属性。
		////////////////////////////////////////////////

		/// <summary>
		/// 是否开启，列表图片功能
		/// </summary>
		public bool IsListImageEnable { get; set; }

		/// <summary>
		/// 对应上传文件后，自动产生的列表图片的路径（绝对，或相对）格式字符串。
		/// </summary>
		public string? ListImageFilePathFormatter { get; set; }

		public string? GetListImageFileAbsolutePathWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight)
		{
			return this.GetFileAbsolutePathWithFileId(
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				imageWidth,
				imageHeight,
				//
				this.ListImageFilePathFormatter);
		}

		/// <summary>
		/// 对应上传文件后，自动产生的列表图片的Uri格式字符串。
		/// </summary>
		public string? ListImageFileAbsoluteUriFormatter { get; set; }

		public string? GetListImageFileAbsoluteUriWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight)
		{
			return this.GetFileAbsoluteUriWithFileId(
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				imageWidth,
				imageHeight,
				//
				this.ListImageFileAbsoluteUriFormatter);
		}

		/// <summary>
		/// 列表图片的尺寸。
		/// </summary>
		public ImageSize? ListImageSize { get; set; }

		/// <summary>
		/// 列表图片的格式。
		/// </summary>
		public string? ListImageFormat { get; set; }

		/// <summary>
		/// 列表图片的默认保存质量，取值范围：0-100。
		/// </summary>
		public int ListImageSaveQuality { get; set; }

		////////////////////////////////////////////////
		// @列表“正方形”图片相关属性。
		////////////////////////////////////////////////

		/// <summary>
		/// 是否开启，列表“正方形”图片功能
		/// </summary>
		public bool IsListSquareImageEnable { get; set; }

		/// <summary>
		/// 对应上传文件后，自动产生的列表图片的路径（绝对，或相对）格式字符串。
		/// </summary>
		public string? ListSquareImageFilePathFormatter { get; set; }

		public string? GetListSquareImageFileAbsolutePathWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight)
		{
			return this.GetFileAbsolutePathWithFileId(
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				imageWidth,
				imageHeight,
				//
				this.ListSquareImageFilePathFormatter);
		}

		/// <summary>
		/// 对应上传文件后，自动产生的列表图片的Uri格式字符串。
		/// </summary>
		public string? ListSquareImageFileAbsoluteUriFormatter { get; set; }

		public string? GetListSquareImageFileAbsoluteUriWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight)
		{
			return this.GetFileAbsoluteUriWithFileId(
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				imageWidth,
				imageHeight,
				//
				this.ListSquareImageFileAbsoluteUriFormatter);
		}

		/// <summary>
		/// 列表图片的尺寸。
		/// </summary>
		public ImageSize? ListSquareImageSize { get; set; }

		/// <summary>
		/// 列表图片的格式。
		/// </summary>
		public string? ListSquareImageFormat { get; set; }

		/// <summary>
		/// 列表图片的默认保存质量，取值范围：0-100。
		/// </summary>
		public int ListSquareImageSaveQuality { get; set; }


		////////////////////////////////////////////////
		// @内容图片相关属性。
		////////////////////////////////////////////////

		/// <summary>
		/// 是否开启，内容图片（详情页图片）功能。
		/// </summary>
		public bool IsContentImageEnable { get; set; }

		/// <summary>
		/// 对应上传文件后，自动产生的内容图片的相对路径格式字符串。
		/// </summary>
		public string? ContentImageFilePathFormatter { get; set; }

		public string? GetContentImageFileAbsolutePathWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight)
		{
			return this.GetFileAbsolutePathWithFileId(
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				imageWidth,
				imageHeight,
				//
				this.ContentImageFilePathFormatter);
		}

		/// <summary>
		/// 对应上传文件后，自动产生的内容图片的Uri格式字符串。
		/// </summary>
		public string? ContentImageFileAbsoluteUriFormatter { get; set; }

		public string? GetContentImageFileAbsoluteUriWithFileId(
		    int fileId,
		    int userId,
		    string? fileExtensionName,
		    string[]? fileTags,
		    int imageWidth,
		    int imageHeight)
		{
			return this.GetFileAbsoluteUriWithFileId(
				fileId,
				userId,
				fileExtensionName,
				fileTags,
				imageWidth,
				imageHeight,
				//
				this.ContentImageFileAbsoluteUriFormatter);
		}

		/// <summary>
		/// 内容图片的最大尺寸。
		/// </summary>
		public ImageSize? ContentImageSizeMax { get; set; }

		/// <summary>
		/// 内容图片的格式。
		/// </summary>
		public string? ContentImageFormat { get; set; }

		/// <summary>
		/// 内容图片的保存质量，取值范围：0-100。
		/// </summary>
		public int ContentImageSaveQuality { get; set; }


		////////////////////////////////////////////////
		// 图象水印信息：
		////////////////////////////////////////////////

		protected ImageWatermarkInfo[]? _imageWatermarkInfes;

		/// <summary>
		/// 水印信息数组。
		/// </summary>
		public ImageWatermarkInfo[]? ImageWatermarkInfes
		{
			get
			{
				return _imageWatermarkInfes;
			}
			set
			{
				var imageWatermarkInfes = value;
				if (imageWatermarkInfes?.Length > 0)
				{
					// 优先按图片宽度，其次按图片高度，由大到小排序：
					Array.Sort(imageWatermarkInfes,
					    (watermarkInfoA, watermarkInfoB) =>
					    {
						    var maxImageWidthA = watermarkInfoA.MaxImageWidth;
						    if (maxImageWidthA == 0)
						    {
							    maxImageWidthA = int.MaxValue;
						    }
						    var maxImageHeightA = watermarkInfoA.MaxImageHeight;
						    if (maxImageHeightA == 0)
						    {
							    maxImageHeightA = int.MaxValue;
						    }

						    var maxImageWidthB = watermarkInfoB.MaxImageWidth;
						    if (maxImageWidthB == 0)
						    {
							    maxImageWidthB = int.MaxValue;
						    }
						    var maxImageHeightB = watermarkInfoB.MaxImageHeight;
						    if (maxImageHeightB == 0)
						    {
							    maxImageHeightB = int.MaxValue;
						    }

						    if (maxImageWidthA > maxImageWidthB)
						    {
							    return 1;
						    }
						    else if (maxImageWidthA < maxImageWidthB)
						    {
							    return -1;
						    }

						    if (maxImageHeightA > maxImageHeightB)
						    {
							    return 1;
						    }
						    else if (maxImageWidthA < maxImageHeightB)
						    {
							    return -1;
						    }
						    return 0;
					    });
				}
				_imageWatermarkInfes = value;
			}
		}

		public ImageWatermarkInfo? GetWatermarkInfoForImage(
			int imageWidth,
			int imageHeight)
		{
			var imageWatermarkInfes = _imageWatermarkInfes;

			if (imageWatermarkInfes == null
				|| imageWatermarkInfes.Length < 1)
			{
				imageWatermarkInfes = Config.Service.ImageWatermarkInfesDefault;
			}
			if (imageWatermarkInfes == null
				|| imageWatermarkInfes.Length < 1)
			{
				return null;
			}

			foreach (var imageWatermarkInfo in imageWatermarkInfes)
			{
				var watermarkMaxImageWidth = imageWatermarkInfo.MaxImageWidth;
				var watermarkMaxImageHeight = imageWatermarkInfo.MaxImageHeight;
				var isImageWidthMatched
					= watermarkMaxImageWidth <= 0
					|| imageWidth <= imageWatermarkInfo.MaxImageWidth;
				var isImageHeightMatched
					= watermarkMaxImageHeight <= 0
					|| imageHeight <= imageWatermarkInfo.MaxImageHeight;
				if (isImageWidthMatched
					&& isImageHeightMatched)
				{
					return imageWatermarkInfo;
				}
			}
			return null;
		}
	}
}
