﻿// --------------------------------------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: 2023 Siemens AG
//
//  SPDX-License-Identifier: MIT

// -------------------------------------------------------------------------------------------------------------------- 
using LCT.APICommunications;
using LCT.APICommunications.Model;
using LCT.ArtifactoryUploader;
using LCT.Common;
using LCT.Common.Constants;
using LCT.Common.Interface;
using log4net;
using log4net.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ArtifactoryUploader
{
    public static class Program
    {
        private static bool m_Verbose = false;
        public static Stopwatch UploaderStopWatch { get; set; }
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static async Task Main(string[] args)
        {
            UploaderStopWatch = new Stopwatch();
            UploaderStopWatch.Start();

            if (!m_Verbose && CommonHelper.IsAzureDevOpsDebugEnabled())
                m_Verbose = true;

            ISettingsManager settingsManager = new SettingsManager();
            CommonAppSettings appSettings = settingsManager.ReadConfiguration<CommonAppSettings>(args, FileConstant.appSettingFileName);
            string FolderPath = InitiateLogger(appSettings);

            Logger.Logger.Log(null, Level.Notice, $"\n====================<<<<< Artifactory Uploader >>>>>====================", null);
            Logger.Logger.Log(null, Level.Notice, $"\nStart of Artifactory Uploader execution: {DateTime.Now}", null);

            if (appSettings.IsTestMode)
                Logger.Logger.Log(null, Level.Alert, $"Artifactory Uploader is running in TEST mode \n", null);


            Logger.Logger.Log(null, Level.Notice, $"Input Parameters used in Artifactory Uploader:\n\t" +
                $"BomFilePath\t\t --> {appSettings.BomFilePath}\n\t" +
                $"JFrogUrl\t\t --> {appSettings.JFrogApi}\n\t" +
                $"Artifactory User\t --> {appSettings.ArtifactoryUploadUser}\n\t" +
                $"LogFolderPath\t\t --> {Path.GetFullPath(FolderPath)}", null);

            if (appSettings.IsTestMode)
                Logger.Logger.Log(null, Level.Notice, $"\tMode\t\t\t --> {appSettings.Mode}\n", null);

            //Validator method to check token validity
            ArtifactoryCredentials artifactoryCredentials = new ArtifactoryCredentials()
            {
                ApiKey = appSettings.ArtifactoryUploadApiKey,
                Email = appSettings.ArtifactoryUploadUser
            };
            NpmJfrogApiCommunication jfrogCommunication = new NpmJfrogApiCommunication(appSettings.JFrogApi, appSettings.JfrogNpmSrcRepo, artifactoryCredentials);
            ArtifactoryValidator artifactoryValidator = new(jfrogCommunication);
            await artifactoryValidator.ValidateArtifactoryCredentials(appSettings);

            //Uploading Package to artifactory
            await PackageUploader.UploadPackageToArtifactory(appSettings);

            Logger.Logger.Log(null, Level.Notice, $"End of Artifactory Uploader execution : {DateTime.Now}\n", null);
        }

        private static string InitiateLogger(CommonAppSettings appSettings)
        {
            string FolderPath;
            if (!string.IsNullOrEmpty(appSettings.LogFolderPath))
            {
                FolderPath = appSettings.LogFolderPath;
                Log4Net.Init(FileConstant.ArtifactoryUploaderLog, appSettings.LogFolderPath, m_Verbose);
            }
            else
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    FolderPath = FileConstant.LogFolder;
                }
                else
                {
                    FolderPath = "/var/log";
                }
                Log4Net.Init(FileConstant.ArtifactoryUploaderLog, FolderPath, m_Verbose);
            }

            return FolderPath;
        }
    }
}
