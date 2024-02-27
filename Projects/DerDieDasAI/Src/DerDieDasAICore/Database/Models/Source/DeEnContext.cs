// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using Microsoft.EntityFrameworkCore;

namespace DerDieDasAICore.Database.Models.Source;

public partial class DeEnContext : DbContext
{
    #region Constructors

    public DeEnContext()
    {
    }

    public DeEnContext(DbContextOptions<DeEnContext> options)
        : base(options)
    {
    }

    #endregion Constructors

    #region Properties

    public static DeEnContext Instance { get; } = new();

    public virtual DbSet<TranslationGrouped> TranslationGroupeds { get; set; }

    public virtual DbSet<Translation> Translations { get; set; }

    #endregion Properties

    #region Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("DataSource=C:\\Temp\\de-en.sqlite3");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Translation>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("translation");

            entity.Property(e => e.IsGood).HasColumnName("is_good");
            entity.Property(e => e.Lexentry).HasColumnName("lexentry");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Sense).HasColumnName("sense");
            entity.Property(e => e.SenseNum).HasColumnName("sense_num");
            entity.Property(e => e.TransList).HasColumnName("trans_list");
            entity.Property(e => e.WrittenRep).HasColumnName("written_rep");
        });

        modelBuilder.Entity<TranslationGrouped>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("translation_grouped");

            entity.Property(e => e.Lexentry).HasColumnName("lexentry");
            entity.Property(e => e.MinSenseNum).HasColumnName("min_sense_num");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.SenseList).HasColumnName("sense_list");
            entity.Property(e => e.TransList).HasColumnName("trans_list");
            entity.Property(e => e.WrittenRep).HasColumnName("written_rep");
        });

        ////OnModelCreatingPartial(modelBuilder);
    }

    ////partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    #endregion Methods
}