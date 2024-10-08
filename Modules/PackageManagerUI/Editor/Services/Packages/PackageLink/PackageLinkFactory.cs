// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Linq;

namespace UnityEditor.PackageManager.UI.Internal
{
    internal class PackageLinkFactory
    {
        [NonSerialized]
        private UpmCache m_UpmCache;
        [NonSerialized]
        private AssetStoreCache m_AssetStoreCache;
        [NonSerialized]
        private ApplicationProxy m_Application;
        [NonSerialized]
        private IOProxy m_IOProxy;
        public void ResolveDependencies(UpmCache upmCache,
            AssetStoreCache assetStoreCache,
            ApplicationProxy application,
            IOProxy iOProxy)
        {
            m_UpmCache = upmCache;
            m_AssetStoreCache = assetStoreCache;
            m_Application = application;
            m_IOProxy = iOProxy;
        }

        private string GetDocumentationUrl(PackageInfo packageInfo, bool isUnityPackage)
        {
            if (packageInfo == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(packageInfo.documentationUrl))
                return packageInfo.documentationUrl;

            if (packageInfo.ExtractUrlFromDescription(m_Application.docsUrlWithShortUnityVersion, out var result))
                return result;

            if (!isUnityPackage)
                return string.Empty;

            var shortVersionId = packageInfo.GetShortVersionId();
            if (string.IsNullOrEmpty(shortVersionId))
                return string.Empty;

            return $"https://docs.unity3d.com/Packages/{shortVersionId}/index.html";
        }

        private string GetChangelogUrl(PackageInfo packageInfo, bool isUnityPackage)
        {
            if (packageInfo == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(packageInfo.changelogUrl))
                return packageInfo.changelogUrl;

            if (!isUnityPackage)
                return string.Empty;

            var shortVersionId = packageInfo.GetShortVersionId();
            if (string.IsNullOrEmpty(shortVersionId))
                return string.Empty;
            return $"https://docs.unity3d.com/Packages/{shortVersionId}/changelog/CHANGELOG.html";
        }

        private string GetLicensesUrl(PackageInfo packageInfo, bool isUnityPackage)
        {
            if (packageInfo == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(packageInfo.licensesUrl))
                return packageInfo.licensesUrl;

            if (!isUnityPackage)
                return string.Empty;

            var shortVersionId = packageInfo.GetShortVersionId();
            if (string.IsNullOrEmpty(shortVersionId))
                return string.Empty;
            return $"https://docs.unity3d.com/Packages/{shortVersionId}/license/index.html";
        }

        private string GetQuickStartUrl(PackageInfo packageInfo)
        {
            var upmReserved = m_UpmCache.ParseUpmReserved(packageInfo);
            return upmReserved?.GetString("quickstart") ?? string.Empty;
        }

        private string GetOfflineDocumentationPath(PackageInfo packageInfo)
        {
            if (!string.IsNullOrEmpty(packageInfo?.resolvedPath))
            {
                try
                {
                    var docsFolder = m_IOProxy.PathsCombine(packageInfo.resolvedPath, "Documentation~");
                    if (!m_IOProxy.DirectoryExists(docsFolder))
                        docsFolder = m_IOProxy.PathsCombine(packageInfo.resolvedPath, "Documentation");
                    if (!m_IOProxy.DirectoryExists(docsFolder))
                    {
                        var readMeFile = m_IOProxy.PathsCombine(packageInfo.resolvedPath, "README.md");
                        return m_IOProxy.FileExists(readMeFile) ? readMeFile : string.Empty;
                    }
                    else
                    {
                        var mdFiles = m_IOProxy.DirectoryGetFiles(docsFolder, "*.md", System.IO.SearchOption.TopDirectoryOnly);
                        var docsMd = mdFiles.FirstOrDefault(d => m_IOProxy.GetFileName(d).ToLower() == "index.md")
                            ?? mdFiles.FirstOrDefault(d => m_IOProxy.GetFileName(d).ToLower() == "tableofcontents.md") ?? mdFiles.FirstOrDefault();
                        if (!string.IsNullOrEmpty(docsMd))
                            return docsMd;
                    }
                }
                catch (System.IO.IOException) { }
            }
            return string.Empty;
        }

        private string GetOfflineChangelogPath(PackageInfo packageInfo)
        {
            if (!string.IsNullOrEmpty(packageInfo?.resolvedPath))
            {
                try
                {
                    var changelogFile = m_IOProxy.PathsCombine(packageInfo.resolvedPath, "CHANGELOG.md");
                    return m_IOProxy.FileExists(changelogFile) ? changelogFile : string.Empty;
                }
                catch (System.IO.IOException) { }
            }
            return string.Empty;
        }

        private string GetOfflineLicensesPath(PackageInfo packageInfo)
        {
            if (!string.IsNullOrEmpty(packageInfo?.resolvedPath))
            {
                try
                {
                    // Attempt preferred Markdown extension
                    var markdownLicense = m_IOProxy.PathsCombine(packageInfo.resolvedPath, "LICENSE.md");
                    if (m_IOProxy.FileExists(markdownLicense))
                    {
                        return markdownLicense;
                    }

                    // Follow up with GitHub preferred naming
                    var githubLicense = m_IOProxy.PathsCombine(packageInfo.resolvedPath, "LICENSE");
                    if (m_IOProxy.FileExists(githubLicense))
                    {
                        return githubLicense;
                    }

                    return string.Empty;
                }
                catch (System.IO.IOException) { }
            }
            return string.Empty;
        }

        public virtual PackageLink CreateUpmDocumentationLink(IPackageVersion version)
        {
            var packageInfo = m_UpmCache.GetBestMatchPackageInfo(version.name, version.isInstalled, version.versionString);
            var isUnityPackage = version.HasTag(PackageTag.Unity);

            return new PackageUpmDocumentationLink(version)
            {
                url = GetDocumentationUrl(packageInfo, isUnityPackage),
                offlinePath = GetOfflineDocumentationPath(packageInfo),
                analyticsEventName = "viewDocs",
                displayName = L10n.Tr("Documentation")
            };
        }

        public virtual PackageLink CreateUpmChangelogLink(IPackageVersion version)
        {
            var packageInfo = m_UpmCache.GetBestMatchPackageInfo(version.name, version.isInstalled, version.versionString);
            var isUnityPackage = version.HasTag(PackageTag.Unity);

            return new PackageUpmChangelogLink(version)
            {
                url = GetChangelogUrl(packageInfo, isUnityPackage),
                offlinePath = GetOfflineChangelogPath(packageInfo),
                analyticsEventName = "viewChangelog",
                displayName = L10n.Tr("Changelog")
            };
        }

        public virtual PackageLink CreateVersionHistoryChangelogLink(IPackageVersion version)
        {
            var packageInfo = m_UpmCache.GetBestMatchPackageInfo(version.name, version.isInstalled, version.versionString);
            var isUnityPackage = version.HasTag(PackageTag.Unity);

            return new PackageUpmVersionHistoryChangelogLink(version)
            {
                url = GetChangelogUrl(packageInfo, isUnityPackage),
                offlinePath = GetOfflineChangelogPath(packageInfo),
                analyticsEventName = "viewChangelog",
                displayName = L10n.Tr("Changelog")
            };
        }

        public virtual PackageLink CreateUpmLicenseLink(IPackageVersion version)
        {
            var packageInfo = m_UpmCache.GetBestMatchPackageInfo(version.name, version.isInstalled, version.versionString);
            var isUnityPackage = version.HasTag(PackageTag.Unity);

            return new PackageUpmLicenseLink(version)
            {
                url = GetLicensesUrl(packageInfo, isUnityPackage),
                offlinePath = GetOfflineLicensesPath(packageInfo),
                analyticsEventName = "viewLicense",
                displayName = L10n.Tr("Licenses")
            };
        }

        public virtual PackageLink CreateUpmQuickStartLink(IPackageVersion version)
        {
            var packageInfo = version != null && version.HasTag(PackageTag.Feature) ? m_UpmCache.GetBestMatchPackageInfo(version.name, version.isInstalled, version.versionString) : null;

            return new PackageLink(version)
            {
                url = GetQuickStartUrl(packageInfo),
                offlinePath = string.Empty,
                analyticsEventName = "viewQuickstart",
                displayName = L10n.Tr("QuickStart")
            };
        }

        public virtual PackageLink CreateProductLink(IPackageVersion version)
        {
            var productInfo = m_AssetStoreCache.GetProductInfo(version.package.product?.id);

            return new PackageLink(version)
            {
                url = productInfo?.assetStoreProductUrl ?? string.Empty,
                offlinePath = string.Empty,
                analyticsEventName = "viewProductInAssetStore",
                displayName = L10n.Tr("View in Asset Store")
            };
        }

        public virtual PackageLink CreateAuthorLink(IPackageVersion version)
        {
            var productInfo = m_AssetStoreCache.GetProductInfo(version?.package.product?.id);

            return new PackageLink(version)
            {
                url = productInfo?.assetStorePublisherUrl ?? string.Empty,
                offlinePath = string.Empty,
                analyticsEventName = "viewAuthorLink",
                displayName = productInfo?.publisherName ?? string.Empty
            };
        }

        public virtual PackageLink CreatePublisherSupportLink(IPackageVersion version)
        {
            var productInfo = m_AssetStoreCache.GetProductInfo(version.package.product?.id);

            return new PackageLink(version)
            {
                url = productInfo?.publisherSupportUrl ?? string.Empty,
                offlinePath = string.Empty,
                analyticsEventName = "viewPublisherSupport",
                displayName = L10n.Tr("Publisher Support")
            };
        }

        public virtual PackageLink CreatePublisherWebsiteLink(IPackageVersion version)
        {
            var productInfo = m_AssetStoreCache.GetProductInfo(version.package.product?.id);

            return new PackageLink(version)
            {
                url = productInfo?.publisherWebsiteUrl ?? string.Empty,
                offlinePath = string.Empty,
                analyticsEventName = "viewPublisherWebsite",
                displayName = L10n.Tr("Publisher Website")
            };
        }

        public virtual PackageLink CreateUseCasesLink(IPackageVersion version)
        {
            return new PackageLink(version)
            {
                url = EditorGameServiceExtension.GetUseCasesUrl(version) ?? string.Empty,
                offlinePath = string.Empty,
                analyticsEventName = "viewUseCases",
                displayName = L10n.Tr("Use Cases")
            };
        }

        public virtual PackageLink CreateDashboardLink(IPackageVersion version)
        {
            return new PackageLink(version)
            {
                url = EditorGameServiceExtension.GetDashboardUrl(version) ?? string.Empty,
                offlinePath = string.Empty,
                analyticsEventName = "viewDashboard",
                displayName = L10n.Tr("Go to Dashboard")
            };
        }
    }
}
