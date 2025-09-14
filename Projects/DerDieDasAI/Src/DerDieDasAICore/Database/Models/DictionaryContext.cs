// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models
{
    using DerDieDasAIApp.UI.Models;
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

        internal static string NounsDBPath { get; private set; }

        #endregion Properties

        #region Methods

        public static void CreateInstance(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (Instance != null)
            {
                return;
            }

            NounsDBPath = Path.Combine(directory, "NounsDict.sqlite3");
            
            // Create the DictionaryContext instance first
            Instance = new DictionaryContext();
            
            // Only generate the nouns table if the database file doesn't exist
            // This should be done after the Instance is created to avoid circular dependency
            if (!System.IO.File.Exists(NounsDBPath))
            {
                try
                {
                    Console.WriteLine("Initializing nouns database...");
                    var process = new GenerateNounsTableProcess();
                    process.Execute();
                    Console.WriteLine("Nouns database initialization completed.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception during nouns database initialization: {e}");
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
                => optionsBuilder.UseSqlite($"Data Source={NounsDBPath}");

        #endregion Methods
    }
}