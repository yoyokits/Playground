// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models
{
    using Microsoft.EntityFrameworkCore;

    public class DictionaryContext : DbContext
    {
        #region Constructors

        public DictionaryContext()
        {
            Database.EnsureCreated();
        }

        public DictionaryContext(DbContextOptions<DictionaryContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        #endregion Constructors

        #region Properties

        public static DictionaryContext Instance { get; private set; }

        public virtual DbSet<Noun> Nouns { get; set; }

        internal static string DBPath { get; private set; }

        #endregion Properties

        #region Methods

        internal static void CreateInstance(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Folder {directory} not found");
            }

            if (Instance != null)
            {
                return;
            }

            DBPath = Path.Combine(directory, "NounsDict.sqlite3");
            Instance = new DictionaryContext();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
                => optionsBuilder.UseSqlite($"Data Source={DBPath}");

        #endregion Methods
    }
}