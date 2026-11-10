namespace {{SolutionName}}.Service.Utils.SharedCacheKeys;

public static class ListingSharedCacheKeys
{
    public static string GetInventoryItem(Guid inventoryItemId) => $"inventoryitem:{inventoryItemId}";

    public static string GetUserAvailableItems(Guid userId) => $"listing:user-items:{userId}";

    public static string GetListingDetail(Guid listingId) => $"listing:detail:{listingId}";
}
