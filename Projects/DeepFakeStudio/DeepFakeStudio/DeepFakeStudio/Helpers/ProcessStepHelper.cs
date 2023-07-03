namespace DeepFakeStudio.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Models;

    /// <summary>
    /// Defines the <see cref="ProcessStepHelper" />.
    /// </summary>
    internal static class ProcessStepHelper
    {
        #region Methods

        /// <summary>
        /// The VerifyWorkspaceFolder.
        /// </summary>
        /// <param name="folder">The folder<see cref="string"/>.</param>
        /// <param name="steps">The steps<see cref="IList{ProcessStep}"/>.</param>
        /// <param name="handler">The handler<see cref="MessageHandler"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool VerifyWorkspaceFolder(string folder, IList<ProcessStep> steps, MessageHandler handler)
        {
            handler.Separator();
            handler.WriteLine($@"Verify process step batch files");
            handler.Separator();
            if (!Directory.Exists(folder))
            {
                handler.SendError($@"Folder ""{folder}"" doesn't exist");
            }
            else
            {
                Directory.SetCurrentDirectory(folder);
            }

            var filesFound = true;
            foreach (var step in steps)
            {
                var stepPath = step.ProcessCommand;
                if (!File.Exists(stepPath))
                {
                    handler.SendError($@"Path ""{stepPath}"" doesn't exist");
                    filesFound = false;
                }
                else
                {
                    handler.WriteLine($@"Path ""{stepPath}"" is found");
                }
            }

            return filesFound;
        }

        #endregion Methods
    }
}