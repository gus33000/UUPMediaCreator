using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Marketplace.StoreEdgeFd
{
    public enum AlternateIdTypes
    {
        PublisherId,
        ContentId,
        InAppOfferToken,
        XboxTitleId,
        PackageFamilyName,
        ProductId,
        SkuId
    }

    public class FulfillmentData
    {
        [JsonPropertyName("ProductId")]
        public string ProductId { get; set; }

        [JsonPropertyName("WuBundleId")]
        public string WuBundleId { get; set; }

        [JsonPropertyName("WuCategoryId")]
        public string WuCategoryId { get; set; }

        [JsonPropertyName("PackageFamilyName")]
        public string PackageFamilyName { get; set; }

        [JsonPropertyName("SkuId")]
        public string SkuId { get; set; }

        [JsonPropertyName("Content")]
        public object Content { get; set; }
    }

    public static class RequestHandler
    {
        public static Products DeserializeProducts(string result)
        {
            Products products = JsonSerializer.Deserialize<Products>(result);

            int counter = 0;
            foreach (Sku Sku in products.Payload.Skus)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Sku.FulfillmentData))
                        products.Payload.Skus[counter].FulfillmentDataDeserialized =
                            JsonSerializer.Deserialize<FulfillmentData>(Sku.FulfillmentData);
                }
                catch { };
                counter++;
            }

            return products;
        }

        public static async Task<Products> GetProducts(string CatalogId, AlternateIdTypes AlternateIdType)
        {
            string result = await GetProductsStr(CatalogId, AlternateIdType);

            Products products = DeserializeProducts(result);

            return products;
        }

        private static CorrelationVector CV = new();

        public static async Task<string> GetProductsStr(string CatalogId, AlternateIdTypes AlternateIdType)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsStore/22106.1401.2.0");

            client.DefaultRequestHeaders.Add("MS-CV", CV.Increment());
            client.DefaultRequestHeaders.Add("OSIsGenuine", "True");
            client.DefaultRequestHeaders.Add("OSIsSMode", "False");

            string url = $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/products/{CatalogId}?appversion=22106.1401.2.0&idType={AlternateIdType}&market=US&locale=en-US";

            string result = await client.GetStringAsync(url);

            return result;
        }
    }

    public class Products
    {
        public string type { get; set; }
        public string Path { get; set; }
        public DateTime ExpiryUtc { get; set; }
        public Payload Payload { get; set; }
    }

    public class Payload
    {
        public string type { get; set; }
        public bool Accessible { get; set; }
        public bool IsDeviceCompanionApp { get; set; }
        public DateTime LastUpdateDateUtc { get; set; }
        public Sku[] Skus { get; set; }
        public string NavItemId { get; set; }
        public string NavId { get; set; }
        public Trailer[] Trailers { get; set; }
        public string RevisionId { get; set; }
        public string PdpBackgroundColor { get; set; }
        public bool ContainsDownloadPackage { get; set; }
        public string[] KeyIds { get; set; }
        public string[] AllowedPlatforms { get; set; }
        public bool XboxXpa { get; set; }
        public Detailsdisplayconfiguration DetailsDisplayConfiguration { get; set; }
        public bool IsMicrosoftProduct { get; set; }
        public bool HasParentBundles { get; set; }
        public bool HasAlternateEditions { get; set; }
        public Productpartd ProductPartD { get; set; }
        public int VideoProductType { get; set; }
        public bool IsMsixvc { get; set; }
        public Image[] Images { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public string Description { get; set; }
        public bool IsUniversal { get; set; }
        public string Language { get; set; }
        public float Price { get; set; }
        public string DisplayPrice { get; set; }
        public float AverageRating { get; set; }
        public int RatingCount { get; set; }
        public bool HasFreeTrial { get; set; }
        public string ProductType { get; set; }
        public string ProductFamilyName { get; set; }
        public string MediaType { get; set; }
        public string[] ContentIds { get; set; }
        public string CollectionItemType { get; set; }
        public int NumberOfSeasons { get; set; }
        public int DurationInSeconds { get; set; }
        public bool IsCompatible { get; set; }
        public bool DeveloperOptOutOfSDCardInstall { get; set; }
        public bool HasAddOns { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public bool HideFromCollections { get; set; }
        public bool IsDownloadable { get; set; }
        public bool HideFromDownloadsAndUpdates { get; set; }
        public bool GamingOptionsXboxLive { get; set; }
        public Actionoverride[] ActionOverrides { get; set; }
        public Badge1[] Badges { get; set; }
        public bool IsGamingAppOnly { get; set; }
        public string PromoMessage { get; set; }
        public DateTime PromoEndDateUtc { get; set; }
        public string VoiceTitle { get; set; }
        public string StrikethroughPrice { get; set; }
        public string[] Platforms { get; set; }
        public string PrivacyUrl { get; set; }
        public Supporturi[] SupportUris { get; set; }
        public string[] Features { get; set; }
        public string[] SupportedLanguages { get; set; }
        public string PublisherCopyrightInformation { get; set; }
        public string AdditionalLicenseTerms { get; set; }
        public Productrating[] ProductRatings { get; set; }
        public string[] PermissionsRequired { get; set; }
        public string[] PackageAndDeviceCapabilities { get; set; }
        public string Version { get; set; }
        public string CategoryId { get; set; }
        public string SubcategoryId { get; set; }
        public string DeviceFamilyDisallowedReason { get; set; }
        public string BuiltFor { get; set; }
        public Systemrequirements SystemRequirements { get; set; }
        public string InstallationTerms { get; set; }
        public Warningmessage[] WarningMessages { get; set; }
        public string[] Categories { get; set; }
        public string Subtitle { get; set; }
        public string DeveloperName { get; set; }
        public string PublisherName { get; set; }
        public string PublisherId { get; set; }
        public string[] PackageFamilyNames { get; set; }
        public string SubcategoryName { get; set; }
        public Alternateid[] AlternateIds { get; set; }
        public DateTime ReleaseDateUtc { get; set; }
        public bool IsPurchaseEnabled { get; set; }
        public Capabilitiestable[] CapabilitiesTable { get; set; }
        public string[] Capabilities { get; set; }
        public string AvailableDevicesDisplayText { get; set; }
        public string AvailableDevicesNarratorText { get; set; }
        public long ApproximateSizeInBytes { get; set; }
        public long MaxInstallSizeInBytes { get; set; }
        public string[] SkuDisplayGroups { get; set; }
        public string[] Notes { get; set; }
        public Appextension AppExtension { get; set; }
        public bool IsSoftBlocked { get; set; }
        public string AddOnPriceRange { get; set; }
        public string RecurrencePolicy { get; set; }
        public string IncompatibleReason { get; set; }
        public bool CapabilityXboxEnhanced { get; set; }
        public string AppWebsiteUrl { get; set; }
        public string PlaintextPassName { get; set; }
        public string GlyphTextPassName { get; set; }
        public string SubscriptionDiscountMessageTemplate { get; set; }
        public Primarypackageidentity PrimaryPackageIdentity { get; set; }
        public Models Models { get; set; }
        public bool IsLtidCompatible { get; set; }
    }

    public class Detailsdisplayconfiguration
    {
        public string type { get; set; }
    }

    public class Productpartd
    {
        public string type { get; set; }
        public string Pdp { get; set; }
        public string[] ModuleTags { get; set; }
    }

    public class Systemrequirements
    {
        public Minimum Minimum { get; set; }
        public Recommended Recommended { get; set; }
    }

    public class Minimum
    {
        public string type { get; set; }
        public string Title { get; set; }
        public Item[] Items { get; set; }
        public string EmptySectionMessage { get; set; }
    }

    public class Item
    {
        public string type { get; set; }
        public string Level { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValidationHint { get; set; }
        public bool IsValidationPassed { get; set; }
        public string HelpLink { get; set; }
        public string HelpTitle { get; set; }
    }

    public class Recommended
    {
        public string type { get; set; }
        public string Title { get; set; }
        public Item1[] Items { get; set; }
        public string EmptySectionMessage { get; set; }
    }

    public class Item1
    {
        public string type { get; set; }
        public string Level { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValidationHint { get; set; }
        public bool IsValidationPassed { get; set; }
        public string HelpLink { get; set; }
        public string HelpTitle { get; set; }
    }

    public class Appextension
    {
        public string type { get; set; }
        public bool AppWithExt { get; set; }
        public string[] AppExtCategoryNames { get; set; }
        public string[] LocalizedCategoryNames { get; set; }
        public string[] AppExtTypes { get; set; }
        public string[] AppExtTypeStrings { get; set; }
        public string AppExtMessage { get; set; }
    }

    public class Primarypackageidentity
    {
        public string type { get; set; }
        public object[] Pfns { get; set; }
        public object[] ContentIds { get; set; }
        public string ProductId { get; set; }
        public bool IsDownloadable { get; set; }
    }

    public class Models
    {
        public string type { get; set; }
        public Mixedrealitymodule MixedRealityModule { get; set; }
    }

    public class Mixedrealitymodule
    {
        public string type { get; set; }
        public Item2[] Items { get; set; }
        public string Schema { get; set; }
    }

    public class Item2
    {
        public string type { get; set; }
        public string Title { get; set; }
        public string[] Lines { get; set; }
    }

    public class Sku
    {
        public string type { get; set; }
        public string[] Actions { get; set; }
        public string AvailabilityId { get; set; }
        public string SkuType { get; set; }
        public float Price { get; set; }
        public string DisplayPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string ResourceSetId { get; set; }
        public bool IsPaymentInstrumentRequired { get; set; }
        public string MSAPurchaseType { get; set; }
        public int RemainingDaysInTrial { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SkuButtonTitle { get; set; }
        public Availability[] Availabilities { get; set; }
        public bool IsPreorder { get; set; }
        public bool IsRental { get; set; }
        public DateTime FirstAvailableDate { get; set; }
        public bool IsRepurchasable { get; set; }
        public string SkuId { get; set; }
        public Skudisplayrank[] SkuDisplayRanks { get; set; }
        public string SkuTitle { get; set; }
        public string Description { get; set; }
        public Image[] Images { get; set; }
        public Badge[] Badges { get; set; }
        public string StrikethroughPrice { get; set; }
        public string PromoMessage { get; set; }
        public DateTime PromoEndDateUtc { get; set; }
        public string FulfillmentData { get; set; }
        public FulfillmentData FulfillmentDataDeserialized { get; set; }
        public Packagerequirement[] PackageRequirements { get; set; }
        public string[] HardwareRequirements { get; set; }
        public string[] HardwareWarnings { get; set; }
        public int ConsumableQuantity { get; set; }
        public string TimedTrialMessage { get; set; }
        public int DurationInSeconds { get; set; }
        public string RecurrencePolicyId { get; set; }
        public string RecurrencePolicyTitle { get; set; }
        public string[] BundledSkus { get; set; }
    }

    public class Availability
    {
        public string type { get; set; }
        public string AvailabilityId { get; set; }
        public float Price { get; set; }
        public string DisplayPrice { get; set; }
        public float RecurrencePrice { get; set; }
        public bool RemediationRequired { get; set; }
        public DateTime AvailabilityEndDate { get; set; }
        public DateTime PreorderReleaseDate { get; set; }
        public int DisplayRank { get; set; }
        public Conditions Conditions { get; set; }
        public string[] Actions { get; set; }
        public bool IsGamesWithGold { get; set; }
        public string StrikethroughPrice { get; set; }
        public string PromoMessage { get; set; }
        public Remediation[] Remediations { get; set; }
        public Affirmation Affirmation { get; set; }
    }

    public class Conditions
    {
        public string type { get; set; }
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string[] ResourceSetIds { get; set; }
        public Clientconditions ClientConditions { get; set; }
    }

    public class Clientconditions
    {
        public string type { get; set; }
        public Allowedplatform[] AllowedPlatforms { get; set; }
    }

    public class Allowedplatform
    {
        public string type { get; set; }
        /*public BigInteger MaxVersion { get; set; }
        public BigInteger MinVersion { get; set; }*/
        public string PlatformName { get; set; }
    }

    public class Affirmation
    {
        public string type { get; set; }
        public string Description { get; set; }
    }

    public class Remediation
    {
        public string type { get; set; }
        public string RemediationId { get; set; }
        public string RemediationType { get; set; }
        public string RedirectUrl { get; set; }
        public string Description { get; set; }
        public string UpsellProductId { get; set; }
        public string UpsellTitle { get; set; }
        public string UpsellProductName { get; set; }
    }

    public class Skudisplayrank
    {
        public string type { get; set; }
        public int Rank { get; set; }
    }

    public class Badge
    {
        public string type { get; set; }
        public string DeferredRequestKey { get; set; }
        public string StyleKey { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
    }

    public class Packagerequirement
    {
        public string type { get; set; }
        public string[] HardwareRequirements { get; set; }
        public string[] SupportedArchitectures { get; set; }
        public Platformdependency[] PlatformDependencies { get; set; }
        public string[] HardwareDependencies { get; set; }
    }

    public class Platformdependency
    {
        public string type { get; set; }
        public string PlatformName { get; set; }
        public long MinVersion { get; set; }
        public long MaxTested { get; set; }
    }

    public class Trailer
    {
        public string type { get; set; }
        public string Title { get; set; }
        public string VideoPurpose { get; set; }
        public string Url { get; set; }
        public string AudioEncoding { get; set; }
        public string VideoEncoding { get; set; }
        public Image Image { get; set; }
        public int Bitrate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string VideoPositionInfo { get; set; }
        public int SortOrder { get; set; }
    }

    public class Image
    {
        public string type { get; set; }
        public string ImageType { get; set; }
        public string Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public string Caption { get; set; }
        public string ImagePositionInfo { get; set; }
    }

    public class Actionoverride
    {
        public string type { get; set; }
        public string ActionType { get; set; }
        public Case[] Cases { get; set; }
    }

    public class Case
    {
        public string type { get; set; }
        public Conditions1 Conditions { get; set; }
        public bool Visibility { get; set; }
        public string Uri { get; set; }
    }

    public class Conditions1
    {
        public string type { get; set; }
        public object[] ClassicAppKeys { get; set; }
        public Platform Platform { get; set; }
    }

    public class Platform
    {
        public string type { get; set; }
        public string PlatformName { get; set; }
        public long MinVersion { get; set; }
    }

    public class Badge1
    {
        public string type { get; set; }
        public string DeferredRequestKey { get; set; }
        public string StyleKey { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
    }

    public class Supporturi
    {
        public string type { get; set; }
        public string Uri { get; set; }
    }

    public class Productrating
    {
        public string type { get; set; }
        public string RatingSystem { get; set; }
        public string RatingSystemShortName { get; set; }
        public string RatingSystemId { get; set; }
        public string RatingSystemUrl { get; set; }
        public string RatingValue { get; set; }
        public string RatingId { get; set; }
        public string RatingValueLogoUrl { get; set; }
        public int RatingAge { get; set; }
        public bool RestrictMetadata { get; set; }
        public bool RestrictPurchase { get; set; }
        public string[] RatingDescriptors { get; set; }
        public string[] RatingDisclaimers { get; set; }
        public string[] InteractiveElements { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public object[] RatingDescriptorLogoUrls { get; set; }
    }

    public class Warningmessage
    {
        public string type { get; set; }
        public string Header { get; set; }
        public string Text { get; set; }
        public string Target { get; set; }
    }

    public class Alternateid
    {
        public string type { get; set; }
        public string AlternateIdType { get; set; }
        public string AlternateIdValue { get; set; }
        public string AlternatedIdTypeString { get; set; }
    }

    public class Capabilitiestable
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}