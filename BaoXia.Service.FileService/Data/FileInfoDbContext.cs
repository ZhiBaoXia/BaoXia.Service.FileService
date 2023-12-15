using Microsoft.EntityFrameworkCore;

namespace BaoXia.Service.FileService.Data
{
    public class FileInfoDbContext : DbContext
    {

        ////////////////////////////////////////////////
        // @自身实现
        ////////////////////////////////////////////////

        #region 自身实现

        public FileInfoDbContext(DbContextOptions<FileInfoDbContext> options)
            : base(options)
        { }


        #endregion


        ////////////////////////////////////////////////
        // @数据表
        ////////////////////////////////////////////////

        #region 自身实现

        public DbSet<Models.FileInfo>? FileInfo { get; set; }

        #endregion

    }
}
