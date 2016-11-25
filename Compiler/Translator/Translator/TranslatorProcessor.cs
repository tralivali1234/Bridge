using Bridge.Contract;
using Bridge.Translator.Logging;
using Bridge.Translator.Utils;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bridge.Translator
{
    public class TranslatorProcessor
    {
        public BridgeOptions BridgeOptions { get; private set; }
        public Logger Logger { get; private set; }
        public IAssemblyInfo TranslatorConfiguration { get; private set; }

        public Translator Translator { get; private set; }

        public TranslatorProcessor(BridgeOptions bridgeOptions, Logger logger)
        {
            this.BridgeOptions = bridgeOptions;
            this.Logger = logger;
        }

        public string PreProcess()
        {
            //System.Diagnostics.Debugger.Launch();
            this.AdjustBridgeOptions();

            this.TranslatorConfiguration = this.ReadConfiguration();

            if (this.TranslatorConfiguration == null)
            {
                return "Could not get configuration";
            }

            if (!this.SetLoggerConfigurationParameters())
            {
                return "Could not set logger configuration";
            }

            this.Translator = this.SetTranslatorProperties();

            if (this.Translator == null)
            {
                return "Could not get Translator";
            }

            return null;
        }

        private void AdjustBridgeOptions()
        {
            var pathHelper = new ConfigHelper();
            var bridgeOptions = this.BridgeOptions;

            bridgeOptions.BridgeLocation = pathHelper.ConvertPath(bridgeOptions.BridgeLocation);
            bridgeOptions.DefaultFileName = pathHelper.ConvertPath(bridgeOptions.DefaultFileName);
            bridgeOptions.Folder = pathHelper.ConvertPath(bridgeOptions.Folder);
            bridgeOptions.Lib = pathHelper.ConvertPath(bridgeOptions.Lib);
            bridgeOptions.OutputLocation = pathHelper.ConvertPath(bridgeOptions.OutputLocation);
            bridgeOptions.ProjectLocation = pathHelper.ConvertPath(bridgeOptions.ProjectLocation);
            bridgeOptions.Source = pathHelper.ConvertPath(bridgeOptions.Source);
        }

        public void Process()
        {
            this.Translator.Translate();
        }

        public string PostProcess()
        {
            var logger = this.Logger;
            var bridgeOptions = this.BridgeOptions;
            var translator = this.Translator;

            logger.Info("Post processing...");

            var outputPath = GetOutputFolder();

            logger.Info("outputPath is " + outputPath);

            translator.CleanOutputFolderIfRequired(outputPath);

            translator.PrepareResourcesConfig();

            var projectPath = Path.GetDirectoryName(translator.Location);
            logger.Info("projectPath is " + projectPath);

            if (bridgeOptions.ExtractCore)
            {
                translator.ExtractCore(outputPath, projectPath);
            }
            else
            {
                logger.Info("No extracting core scripts option enabled");
            }

            var fileName = bridgeOptions.DefaultFileName;

            var files = translator.SaveTo(outputPath, fileName);

            translator.InjectResources(outputPath, projectPath, files);

            translator.RunAfterBuild();

            translator.Flush(outputPath, fileName);

            logger.Info("Run plugins AfterOutput...");
            translator.Plugins.AfterOutput(translator, outputPath, !bridgeOptions.ExtractCore);
            logger.Info("Done plugins AfterOutput");

            logger.Info("Done post processing");

            return outputPath;
        }

        private string GetOutputFolder(bool basePathOnly = false, bool strict = false)
        {
            var bridgeOptions = this.BridgeOptions;
            string basePath = bridgeOptions.IsFolderMode ? bridgeOptions.Folder : Path.GetDirectoryName(bridgeOptions.ProjectLocation);

            if (basePathOnly)
            {
                return new ConfigHelper().ConvertPath(basePath);
            }

            string assemblyOutput = string.Empty;

            if (this.Translator != null)
            {
                assemblyOutput = this.Translator.AssemblyInfo.Output;
            }
            else if (this.TranslatorConfiguration != null)
            {
                assemblyOutput = this.TranslatorConfiguration.Output;
            }
            else if (strict)
            {
                throw new InvalidOperationException("Could not get output folder as assembly configuration is still null");
            }
            else
            {
                this.Logger.Warn("Could not get assembly output folder");
            }

            string outputPath = string.IsNullOrWhiteSpace(assemblyOutput)
                ? Path.Combine(basePath, Path.GetDirectoryName(bridgeOptions.OutputLocation))
                : Path.Combine(basePath, assemblyOutput);

            outputPath = new ConfigHelper().ConvertPath(outputPath);

            return outputPath;
        }

        private IAssemblyInfo ReadConfiguration()
        {
            var bridgeOptions = this.BridgeOptions;

            var logger = this.Logger;

            var location = bridgeOptions.IsFolderMode ? bridgeOptions.Folder : bridgeOptions.ProjectLocation;

            try
            {
                var configReader = new AssemblyConfigHelper(logger);

                return configReader.ReadConfig(bridgeOptions.IsFolderMode, location, bridgeOptions.Configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not read configuration: " + ex.ToString());
            }

            return null;
        }

        private bool SetLoggerConfigurationParameters()
        {
            var logger = this.Logger;
            var bridgeOptions = this.BridgeOptions;
            var assemblyConfig = this.TranslatorConfiguration;

            if (bridgeOptions.NoLoggerSetUp)
            {
                return true;
            }

            logger.Info("Applying logger configuration parameters...");

            logger.Name = bridgeOptions.Name;

            if (!string.IsNullOrEmpty(logger.Name))
            {
                logger.Info("Logger name: " + logger.Name);
            }

            var loggerLevel = assemblyConfig.Logging.Level ?? LoggerLevel.None;

            logger.Info("Logger level: " + loggerLevel);

            if (loggerLevel <= LoggerLevel.None)
            {
                logger.Info("To enable further logging use configuration setting \"logging\" in bridge.json. See http://bridge.net/docs/global-configuration/#logging");
            }

            try
            {
                logger.LoggerLevel = loggerLevel;

                logger.Info("Read config file: " + AssemblyConfigHelper.ConfigToString(assemblyConfig));

                logger.BufferedMode = false;

                if (bridgeOptions.NoTimeStamp.HasValue)
                {
                    logger.UseTimeStamp = !bridgeOptions.NoTimeStamp.Value;
                }
                else if (assemblyConfig.Logging.TimeStamps.HasValue)
                {
                    logger.UseTimeStamp = assemblyConfig.Logging.TimeStamps.Value;
                }
                else
                {
                    logger.UseTimeStamp = true;
                }

                var fileLoggerWriter = logger.GetFileLogger();

                if (fileLoggerWriter != null)
                {
                    var logFileFolder = assemblyConfig.Logging.Folder;
                    if (string.IsNullOrWhiteSpace(logFileFolder))
                    {
                        logFileFolder = this.GetOutputFolder(false, false);
                    }
                    else if (!Path.IsPathRooted(logFileFolder))
                    {
                        logFileFolder = Path.Combine(this.GetOutputFolder(true, false), logFileFolder);
                    }

                    fileLoggerWriter.SetParameters(logFileFolder, assemblyConfig.Logging.FileName, assemblyConfig.Logging.MaxSize);
                }

                logger.Flush();

                logger.Info("Setting logger configuration parameters done");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not read configuration: " + ex.ToString());
            }

            return false;
        }

        private Translator SetTranslatorProperties()
        {
            var logger = this.Logger;
            var bridgeOptions = this.BridgeOptions;
            var assemblyConfig = this.TranslatorConfiguration;

            logger.Info("Setting translator properties...");

            try
            {
                Bridge.Translator.Translator translator = null;

                // FIXME: detect by extension whether first argument is a project or DLL
                if (!bridgeOptions.IsFolderMode)
                {
                    translator = new Bridge.Translator.Translator(bridgeOptions.ProjectLocation, bridgeOptions.FromTask);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(bridgeOptions.Lib))
                    {
                        throw new Exception("Please define path to assembly using -lib option");
                    }

                    bridgeOptions.Lib = Path.Combine(bridgeOptions.Folder, bridgeOptions.Lib);
                    translator = new Bridge.Translator.Translator(bridgeOptions.Folder, bridgeOptions.Source, bridgeOptions.Recursive, bridgeOptions.Lib);
                }

                translator.AssemblyInfo = assemblyConfig;
                translator.Configuration = bridgeOptions.Configuration;
                translator.Platform = bridgeOptions.Platform;

                if (string.IsNullOrEmpty(bridgeOptions.BridgeLocation))
                {
                    bridgeOptions.BridgeLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Bridge.dll");
                }

                translator.BridgeLocation = bridgeOptions.BridgeLocation;
                translator.Rebuild = bridgeOptions.Rebuild;
                translator.Log = logger;

                if (bridgeOptions.DefinitionConstants != null)
                {
                    translator.DefineConstants.AddRange(bridgeOptions.DefinitionConstants.Split(';').Select(s => s.Trim()).Where(s => s != ""));
                    translator.DefineConstants = translator.DefineConstants.Distinct().ToList();
                }

                translator.Log.Info("Translator properties:");
                translator.Log.Info("\tBridgeLocation:" + translator.BridgeLocation ?? "");
                translator.Log.Info("\tBuildArguments:" + translator.BuildArguments ?? "");
                translator.Log.Info("\tConfiguration:" + translator.Configuration ?? "");
                translator.Log.Info("\tPlatform:" + translator.Platform ?? "");
                translator.Log.Info("\tDefineConstants:" + (translator.DefineConstants != null ? string.Join(" ", translator.DefineConstants) : ""));
                translator.Log.Info("\tRebuild:" + translator.Rebuild);

                logger.Info("Setting translator properties done");

                return translator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not read configuration: " + ex.ToString());
            }

            return null;
        }
    }
}