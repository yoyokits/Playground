// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.App.UI.Models
{
    using DerDieDasAIApp.UI.Models;
    using DerDieDasAICore.Database.Models;
    using DerDieDasAICore.Database.Models.Source;
    using DerDieDasAICore.Properties;
    using FluentAssertions;
    using System.Linq;

    [TestClass]
    public class GenerateNounsTableProcessTest
    {
        #region Methods

        [TestMethod]
        public void Execute()
        {
            // Diagnostic: Check if embedded resources are available
            DiagnoseEmbeddedResources();
            
            // Ensure test directory exists and is clean
            var testDirectory = "C:\\Temp\\DerDieDas";
            CleanupTestDirectory(testDirectory);

            try
            {
                // Test the database initialization first
                Console.WriteLine("Testing DeContext initialization...");
                TestDeContextInitialization();
                
                // Only proceed if database files are properly initialized
                Console.WriteLine("Creating DictionaryContext instance...");
                DictionaryContext.CreateInstance(testDirectory);
                
                Console.WriteLine("Creating GenerateNounsTableProcess...");
                var process = new GenerateNounsTableProcess();
                
                Console.WriteLine("Executing process...");
                process.Execute();
                
                Console.WriteLine("Getting DictionaryContext instance...");
                var dictionaryDB = DictionaryContext.Instance;
                
                Console.WriteLine("Querying nouns...");
                var nouns = dictionaryDB.Nouns.ToArray();
                
                Console.WriteLine($"Found {nouns.Length} nouns");
                nouns.Should().HaveCountGreaterThan(0);
                
                // Additional validation
                Console.WriteLine("Validating noun data quality...");
                ValidateNounData(nouns);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed with exception: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Additional diagnostics on failure
                DiagnoseFileSystem(testDirectory);
                throw;
            }
        }

        private void CleanupTestDirectory(string testDirectory)
        {
            if (Directory.Exists(testDirectory))
            {
                Console.WriteLine($"Test directory exists: {testDirectory}");
                
                // List existing files
                var files = Directory.GetFiles(testDirectory, "*.sqlite3");
                Console.WriteLine($"Existing SQLite files: {string.Join(", ", files)}");
                
                // Clean up for fresh test
                foreach (var file in files)
                {
                    try
                    {
                        // Force garbage collection to release any database connections
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        
                        File.Delete(file);
                        Console.WriteLine($"Deleted existing file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not delete {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(testDirectory);
                Console.WriteLine($"Created test directory: {testDirectory}");
            }
        }

        private void DiagnoseEmbeddedResources()
        {
            Console.WriteLine("=== Embedded Resources Diagnostics ===");
            var assembly = typeof(DeContext).Assembly;
            var resources = assembly.GetManifestResourceNames();
            
            Console.WriteLine($"Assembly: {assembly.FullName}");
            Console.WriteLine($"Total embedded resources found: {resources.Length}");
            
            foreach (var resource in resources)
            {
                Console.WriteLine($"  - {resource}");
            }
            
            // Check for expected resources
            var expectedResources = new[] { "de.sqlite3", "de-en.sqlite3" };
            foreach (var expected in expectedResources)
            {
                var found = resources.Any(r => r.Contains(expected));
                Console.WriteLine($"Expected resource '{expected}' found: {found}");
                
                if (found)
                {
                    var fullResourceName = resources.First(r => r.Contains(expected));
                    using var stream = assembly.GetManifestResourceStream(fullResourceName);
                    Console.WriteLine($"  Resource '{fullResourceName}' stream: {(stream != null ? $"OK ({stream.Length} bytes)" : "NULL")}");
                }
            }
            Console.WriteLine("=== End Embedded Resources Diagnostics ===\n");
        }

        private void TestDeContextInitialization()
        {
            Console.WriteLine("=== DeContext Initialization Test ===");
            try
            {
                // Test if we can create a DeContext instance
                using var deContext = new DeContext();
                Console.WriteLine("DeContext created successfully");
                
                // Test if database files exist after initialization
                var rootDir = Settings.Default.RootDirectory;
                var deDbPath = Path.Combine(rootDir, "de.sqlite3");
                var deEnDbPath = Path.Combine(rootDir, "de-en.sqlite3");
                
                Console.WriteLine($"Root directory: {rootDir}");
                Console.WriteLine($"de.sqlite3 exists: {File.Exists(deDbPath)}");
                Console.WriteLine($"de-en.sqlite3 exists: {File.Exists(deEnDbPath)}");
                
                // Validate file sizes
                ValidateDatabaseFile(deDbPath, "de.sqlite3");
                ValidateDatabaseFile(deEnDbPath, "de-en.sqlite3");
                
                // Test basic database connectivity
                Console.WriteLine("Testing database connectivity...");
                var canConnect = deContext.Database.CanConnect();
                Console.WriteLine($"Database can connect: {canConnect}");
                
                if (!canConnect)
                {
                    throw new InvalidOperationException("Cannot connect to database - check embedded resources");
                }
                
                // Test database schema
                Console.WriteLine("Testing database schema...");
                TestDatabaseSchema(deContext);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeContext initialization failed: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }
            Console.WriteLine("=== End DeContext Initialization Test ===\n");
        }

        private void ValidateDatabaseFile(string dbPath, string fileName)
        {
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                Console.WriteLine($"{fileName} size: {fileInfo.Length} bytes");
                
                if (fileInfo.Length == 0)
                {
                    throw new InvalidDataException($"{fileName} is empty - embedded resource copy failed");
                }
                
                if (fileInfo.Length < 1024) // Arbitrary minimum size for a valid SQLite file
                {
                    Console.WriteLine($"Warning: {fileName} seems unusually small ({fileInfo.Length} bytes)");
                }
            }
            else
            {
                throw new FileNotFoundException($"Required database file not found: {dbPath}");
            }
        }

        private void TestDatabaseSchema(DeContext deContext)
        {
            try
            {
                // Test if we can query the database without getting data
                var entryExists = deContext.Entries.Any();
                Console.WriteLine($"Entries table accessible: {entryExists}");
                
                if (entryExists)
                {
                    var entryCount = deContext.Entries.Count();
                    Console.WriteLine($"Total entries in database: {entryCount}");
                    
                    // Test a simple query to validate schema
                    var sampleEntry = deContext.Entries.FirstOrDefault();
                    if (sampleEntry != null)
                    {
                        Console.WriteLine($"Sample entry: Word='{sampleEntry.WrittenRep}', Gender='{sampleEntry.Gender}', POS='{sampleEntry.PartOfSpeech}'");
                    }
                }
                else
                {
                    Console.WriteLine("Warning: No entries found in database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database schema test failed: {ex.Message}");
                throw new InvalidOperationException("Database schema validation failed - check embedded database files", ex);
            }
        }

        private void ValidateNounData(Noun[] nouns)
        {
            if (nouns.Length == 0)
            {
                Console.WriteLine("Warning: No nouns generated");
                return;
            }
            
            // Check data quality
            var validNouns = nouns.Where(n => !string.IsNullOrEmpty(n.Word) && !string.IsNullOrEmpty(n.Gender)).ToArray();
            var nounsWithTranslation = nouns.Where(n => !string.IsNullOrEmpty(n.Translation)).ToArray();
            var nounsWithImportance = nouns.Where(n => n.Importance > 0).ToArray();
            
            Console.WriteLine($"Valid nouns (with word and gender): {validNouns.Length}");
            Console.WriteLine($"Nouns with translation: {nounsWithTranslation.Length}");
            Console.WriteLine($"Nouns with importance score: {nounsWithImportance.Length}");
            
            // Sample data
            if (validNouns.Length > 0)
            {
                var sampleNoun = validNouns.First();
                Console.WriteLine($"Sample noun: '{sampleNoun.Word}' ({sampleNoun.Gender}) - '{sampleNoun.Translation}' (Importance: {sampleNoun.Importance})");
            }
            
            // Validate we have reasonable data
            validNouns.Should().HaveCountGreaterThan(0, "Should have at least some valid nouns with word and gender");
            nounsWithTranslation.Should().HaveCountGreaterThan(0, "Should have at least some nouns with translations");
        }

        private void DiagnoseFileSystem(string directory)
        {
            Console.WriteLine("=== File System Diagnostics ===");
            try
            {
                if (Directory.Exists(directory))
                {
                    var allFiles = Directory.GetFiles(directory);
                    Console.WriteLine($"Files in {directory}:");
                    foreach (var file in allFiles)
                    {
                        var fileInfo = new FileInfo(file);
                        Console.WriteLine($"  - {Path.GetFileName(file)} ({fileInfo.Length} bytes)");
                    }
                }
                else
                {
                    Console.WriteLine($"Directory does not exist: {directory}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File system diagnosis failed: {ex.Message}");
            }
            Console.WriteLine("=== End File System Diagnostics ===");
        }

        #endregion Methods
    }
}