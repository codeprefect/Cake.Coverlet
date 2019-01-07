﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;
using Cake.Core.IO;
using Cake.Coverlet;

namespace Cake.Coverlet
{
    internal static class ArgumentsProcessor
    {
        public static ProcessArgumentBuilder ProcessMSBuildArguments(
                CoverletSettings settings,
                ICakeEnvironment cakeEnvironment,
                ProcessArgumentBuilder builder,
                FilePath project)
        {
            builder.AppendMSBuildProperty(nameof(CoverletSettings.CollectCoverage), settings.CollectCoverage.ToString());
            builder.AppendPropertyList(nameof(CoverletSettings.CoverletOutputFormat), SplitFlagEnum(settings.CoverletOutputFormat));

            if (settings.Threshold.HasValue)
            {
                if (settings.Threshold > 100)
                {
                    throw new Exception("Threshold Percentage cannot be set as greater than 100%");
                }

                builder.AppendMSBuildProperty(nameof(CoverletSettings.Threshold), settings.Threshold.ToString());

                if (settings.ThresholdType != ThresholdType.NotSet)
                {
                    builder.AppendPropertyList(nameof(CoverletSettings.ThresholdType), SplitFlagEnum(settings.ThresholdType));
                }
            }

            if (settings.CoverletOutputDirectory != null && string.IsNullOrEmpty(settings.CoverletOutputName))
            {
                var directoryPath = settings.CoverletOutputDirectory
                    .MakeAbsolute(cakeEnvironment).FullPath;

                builder.AppendMSBuildProperty("CoverletOutput", directoryPath);
            }
            else if (!string.IsNullOrEmpty(settings.CoverletOutputName))
            {
                var dir = settings.CoverletOutputDirectory ?? project.GetDirectory();
                var directoryPath = dir.MakeAbsolute(cakeEnvironment).FullPath;

                var filepath = FilePath.FromString(settings.OutputTransformer(settings.CoverletOutputName, directoryPath));

                builder.AppendMSBuildProperty("CoverletOutput", filepath.MakeAbsolute(cakeEnvironment).FullPath);
            }

            if (settings.ExcludeByFile.Count > 0)
            {
                builder.AppendPropertyList(nameof(CoverletSettings.ExcludeByFile), settings.ExcludeByFile);
            }


            if (settings.Include.Count > 0)
            {
                builder.AppendPropertyList(nameof(CoverletSettings.Include), settings.Include);
            }


            if (settings.Exclude.Count > 0)
            {
                builder.AppendPropertyList(nameof(CoverletSettings.Exclude), settings.Exclude);
            }

            if (settings.ExcludeByAttribute.Count > 0)
            {
                builder.AppendPropertyList(nameof(CoverletSettings.ExcludeByAttribute), settings.ExcludeByAttribute);
            }

            if (settings.MergeWithFile != null && settings.MergeWithFile.GetExtension() == ".json")
            {
                builder.AppendMSBuildPropertyQuoted("MergeWith", settings.MergeWithFile.MakeAbsolute(cakeEnvironment).FullPath);
            }

            return builder;
        }

        public static ProcessArgumentBuilder ProcessToolArguments(
                CoverletSettings settings,
                ICakeEnvironment cakeEnvironment,
                ProcessArgumentBuilder builder,
                FilePath project)
        {
            builder.AppendSwitch("--format", SplitFlagEnum(settings.CoverletOutputFormat));

            if (settings.Threshold.HasValue)
            {
                if (settings.Threshold > 100)
                {
                    throw new Exception("Threshold Percentage cannot be set as greater than 100%");
                }

                builder.AppendSwitch(nameof(CoverletSettings.Threshold), settings.Threshold.ToString());

                if (settings.ThresholdType != ThresholdType.NotSet)
                {
                    builder.AppendSwitchQuoted("--threshold-type", SplitFlagEnum(settings.ThresholdType));
                }
            }

            if (settings.CoverletOutputDirectory != null && string.IsNullOrEmpty(settings.CoverletOutputName))
            {
                var directoryPath = settings.CoverletOutputDirectory
                    .MakeAbsolute(cakeEnvironment).FullPath;

                builder.AppendSwitchQuoted("--output", directoryPath);
            }
            else if (!string.IsNullOrEmpty(settings.CoverletOutputName))
            {
                var dir = settings.CoverletOutputDirectory ?? project.GetDirectory();
                var directoryPath = dir.MakeAbsolute(cakeEnvironment).FullPath;

                var filepath = FilePath.FromString(settings.OutputTransformer(settings.CoverletOutputName, directoryPath));

                builder.AppendSwitchQuoted("--output", filepath.MakeAbsolute(cakeEnvironment).FullPath);
            }

            if (settings.ExcludeByFile.Count > 0)
            {
                builder.AppendSwitchQuoted("--exclude-by-file", settings.ExcludeByFile);
            }

            if (settings.ExcludeByAttribute.Count > 0)
            {
                builder.AppendSwitchQuoted("--exclude-by-attribute", settings.ExcludeByAttribute);
            }

            if (settings.Exclude.Count > 0)
            {
                builder.AppendSwitchQuoted("--exclude", settings.Exclude);
            }

            if (settings.Include.Count > 0)
            {
                builder.AppendSwitchQuoted("--include", settings.Include);
            }

            if (settings.MergeWithFile != null && settings.MergeWithFile.GetExtension() == ".json")
            {
                builder.AppendSwitchQuoted("--merge-with", settings.MergeWithFile.MakeAbsolute(cakeEnvironment).FullPath);
            }

            return builder;
        }

        private static IEnumerable<string> SplitFlagEnum(Enum @enum) => @enum.ToString("g").Split(',').Select(s => s.ToLowerInvariant());
    }
}
