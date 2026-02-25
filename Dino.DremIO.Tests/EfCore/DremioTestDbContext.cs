using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dino.Dremio.EntityframeworkCore.Provider.Attributes;
namespace Dino.DremIO.Tests.EfCore
{
    [Keyless]
    [TableContext("youtube-channel-content")]
    [Table("youtube-channel-revenue-combine")]
    public class RevenueCombine
    {
        public string ChannelId { get; set; }
        public decimal View { get; set; }
        public decimal Money { get; set; }
        //public Guid ProfileId { get; set; }
    }
    // ── DbContext dùng cho test ───────────────────────────────────────────────────

    /// <summary>
    /// DbContext tối giản để kiểm tra việc đăng ký DremioOptionsExtension.
    /// </summary>
    public class DremioTestDbContext : DbContext
    {
        public DremioTestDbContext(DbContextOptions<DremioTestDbContext> options)
            : base(options) { }

        public  DbSet<RevenueCombine> RevenueCombines { get; set; }
    }
}
