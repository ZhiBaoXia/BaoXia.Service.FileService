namespace BaoXia.Service.FileService.Constants
{
        public class FilePaths
        {
                private static string? _resourcesDictionaryPath;

                public static string ResourcesDictionaryPath
                {
                        get
                        {
                                if (_resourcesDictionaryPath == null)
                                {
                                        _resourcesDictionaryPath
                                                = BaoXia.Utils.Environment.ApplicationDirectoryPath
                                                + "Resources/";
                                }
                                return _resourcesDictionaryPath;
                        }
                }

                private static string? _fontsDictionaryPath;

                public static string FontsDictionaryPath
                {
                        get
                        {
                                if (_fontsDictionaryPath == null)
                                {
                                        _fontsDictionaryPath
                                                = FilePaths.ResourcesDictionaryPath
                                                + "Fonts/";
                                }
                                return _fontsDictionaryPath;
                        }
                }
        }
}
