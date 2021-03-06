//-------------------------------------------------------------------------------------------------
// <copyright file="GenerateTestConsumer.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
// 
// <summary>
// MSBuild task to class to scan object files for unit tests.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace WixToolset.Lux
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using WixToolset.Data;

    /// <summary>
    /// An MSBuild task to class to scan object files for unit tests.
    /// </summary>
    public sealed class GenerateTestConsumer : Task
    {
        private ITaskItem[] extensions;
        private ITaskItem[] inputFiles;
        private ITaskItem[] inputFragments;
        private ITaskItem outputFile;

        /// <summary>
        /// Sets the list of WiX extensions used by the input files.
        /// </summary>
        public ITaskItem[] Extensions
        {
            set
            {
                this.extensions = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of WiX object and library files to scan for unit tests.
        /// </summary>
        [Required]
        public ITaskItem[] InputFiles
        {
            get
            {
                return this.inputFiles;
            }

            set
            {
                this.inputFiles = value;
            }
        }

        /// <summary>
        /// Gets the subset of InputFiles that contain unit tests and should be included in a test package.
        /// </summary>
        [Output]
        public ITaskItem[] InputFragments
        {
            get
            {
                return this.inputFragments;
            }
        }

        /// <summary>
        /// Sets the optional generated test package source file.
        /// </summary>
        public ITaskItem OutputFile
        {
            set
            {
                this.outputFile = value;
            }
        }

        /// <summary>
        /// Scan the input files for unit tests and, if specified, generate a test package source file.
        /// </summary>
        /// <returns>True if successful or False if there were no unit tests in the input files or a test package couldn't be created.</returns>
        public override bool Execute()
        {
            List<string> generatorExtensions = new List<string>();

            Messaging.Instance.InitializeAppName("LXT", "LuxTasks.dll").Display += this.DisplayMessage;

            Generator generator = new Generator();
            generator.Extensions = GenerateTestConsumer.ToStringCollection(this.extensions);
            generator.InputFiles = GenerateTestConsumer.ToListOfString(this.inputFiles);
            generator.OutputFile = null != this.outputFile ? this.outputFile.ItemSpec : null;

            bool success = generator.Generate();
            this.inputFragments = GenerateTestConsumer.ToITaskItemArray(generator.InputFragments);
            return success;
        }

        /// <summary>
        /// Converts a string list to an ITaskItem array for MSBuild.
        /// </summary>
        /// <param name="list">The list to convert.</param>
        /// <returns>The converted array.</returns>
        private static ITaskItem[] ToITaskItemArray(List<string> list)
        {
            ITaskItem[] items = new ITaskItem[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                items[i] = new TaskItem(list[i]);
            }

            return items;
        }

        /// <summary>
        /// Converts an array of ITaskItem ItemSpec strings to a string list.
        /// </summary>
        /// <param name="items">The array to convert.</param>
        /// <returns>The converted list.</returns>
        private static List<string> ToListOfString(ITaskItem[] items)
        {
            List<string> list = null;

            if (null != items && 0 < items.Length)
            {
                list = new List<string>(items.Length);

                foreach (ITaskItem item in items)
                {
                    list.Add(item.ItemSpec);
                }
            }

            return list;
        }

        /// <summary>
        /// Converts an array of ITaskItem ItemSpec strings to a string collection.
        /// </summary>
        /// <param name="items">The array to convert.</param>
        /// <returns>The string collection.</returns>
        private static StringCollection ToStringCollection(ITaskItem[] items)
        {
            StringCollection collection = null;

            if (null != items && 0 < items.Length)
            {
                collection = new StringCollection();

                foreach (ITaskItem item in items)
                {
                    collection.Add(item.ItemSpec);
                }
            }

            return collection;
        }

        /// <summary>
        /// Display a test-generator message to the user via the MSBuild logger.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Arguments for the message event.</param>
        private void DisplayMessage(object sender, DisplayEventArgs e)
        {
            MessageImportance importance = (MessageLevel.Error == e.Level || MessageLevel.Warning == e.Level) ? MessageImportance.High : MessageImportance.Normal;
            this.Log.LogMessage(importance, e.Message);
        }
    }
}
