// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models.Source;

using Microsoft.EntityFrameworkCore;

public partial class DeContext : DbContext
{
    #region Constructors

    public DeContext()
    {
    }

    public DeContext(DbContextOptions<DeContext> options)
        : base(options)
    {
    }

    #endregion Constructors

    #region Properties

    public static DeContext Instance { get; } = new DeContext();

    public virtual DbSet<Entry> Entries { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<Importance> Importances { get; set; }

    public virtual DbSet<InflectionTable> InflectionTables { get; set; }

    public virtual DbSet<RelImportance> RelImportances { get; set; }

    #endregion Properties

    #region Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlite("Data Source=D:\\Local\\Projects\\GitHub\\Playground\\Projects\\DerDieDasAI\\Data\\de.sqlite3");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entry>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("entry");

            entity.HasIndex(e => e.Lexentry, "entry_pkey").IsUnique();

            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Lexentry).HasColumnName("lexentry");
            entity.Property(e => e.PartOfSpeech).HasColumnName("part_of_speech");
            entity.Property(e => e.PronunList).HasColumnName("pronun_list");
            entity.Property(e => e.Vocable).HasColumnName("vocable");
            entity.Property(e => e.WrittenRep).HasColumnName("written_rep");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("form");

            entity.HasIndex(e => e.Lexentry, "form_lexentry_idx");

            entity.Property(e => e.Case).HasColumnName("case");
            entity.Property(e => e.Definiteness).HasColumnName("definiteness");
            entity.Property(e => e.Inflection).HasColumnName("inflection");
            entity.Property(e => e.Lexentry).HasColumnName("lexentry");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.OtherWritten).HasColumnName("other_written");
            entity.Property(e => e.OtherWrittenFull).HasColumnName("other_written_full");
            entity.Property(e => e.Person).HasColumnName("person");
            entity.Property(e => e.Pos).HasColumnName("pos");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Tense).HasColumnName("tense");
            entity.Property(e => e.Voice).HasColumnName("voice");
        });

        modelBuilder.Entity<Importance>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("importance");

            entity.HasIndex(e => e.WrittenRepGuess, "imp_unique_rep").IsUnique();

            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Vocable).HasColumnName("vocable");
            entity.Property(e => e.WrittenRepGuess).HasColumnName("written_rep_guess");
        });

        modelBuilder.Entity<InflectionTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("inflection_table");

            entity.Property(e => e.Case).HasColumnName("case");
            entity.Property(e => e.Definiteness).HasColumnName("definiteness");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Person).HasColumnName("person");
            entity.Property(e => e.Pos).HasColumnName("pos");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Tense).HasColumnName("tense");
            entity.Property(e => e.TenseName).HasColumnName("tense_name");
            entity.Property(e => e.Voice).HasColumnName("voice");
        });

        modelBuilder.Entity<RelImportance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("rel_importance");

            entity.Property(e => e.RelScore).HasColumnName("rel_score");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Vocable).HasColumnName("vocable");
            entity.Property(e => e.WrittenRepGuess).HasColumnName("written_rep_guess");
        });

        ////OnModelCreatingPartial(modelBuilder);
    }

    #endregion Methods
}