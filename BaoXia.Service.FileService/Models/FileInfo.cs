using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoXia.Service.FileService.Models
{
        [Index(new string[] { "RelativeFilePath" })]
        public class FileInfo
        {
                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性

                [Key]
                [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
                public int Id { get; set; }

                public FileState State { get; set; }

                public string? RelativeFilePath { get; set; }

                public string? TagNamesOrderByTagNameAES { get; set; }

                public int UserId { get; set; }

                public DateTime CreateTime { get; set; }

                public DateTime UpdateTime { get; set; }

                public DateTime StorageTime { get; set; }

                public double SecondsToResponse { get; set; }

                public double SecondsToDatabaseOperation { get; set; }

                public double SecondsToFileProcessing { get; set; }

                public double SecondsToFileStorage { get; set; }

                #endregion


                ////////////////////////////////////////////////
                // @静态常量
                ////////////////////////////////////////////////

                #region 静态常量

                //public FileInfo CreateFileInfoWith(string[]? fileTags)
                //{

                //}

                #endregion

        }
}
