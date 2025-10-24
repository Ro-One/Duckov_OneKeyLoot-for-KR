using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Buildings;
using Duckov.Buildings.UI;
using Duckov.Economy;
using Duckov.Quests;
using Duckov.UI;
using Saves;
using UnityEngine;

// Token: 0x020001BB RID: 443
public class ItemWishlist : MonoBehaviour
{
    // Token: 0x17000268 RID: 616
    // (get) Token: 0x06000D2B RID: 3371 RVA: 0x00036BB6 File Offset: 0x00034DB6
    // (set) Token: 0x06000D2C RID: 3372 RVA: 0x00036BBD File Offset: 0x00034DBD
    public static ItemWishlist Instance { get; private set; }

    // Token: 0x14000063 RID: 99
    // (add) Token: 0x06000D2D RID: 3373 RVA: 0x00036BC8 File Offset: 0x00034DC8
    // (remove) Token: 0x06000D2E RID: 3374 RVA: 0x00036BFC File Offset: 0x00034DFC
    public static event Action<int> OnWishlistChanged;

    // Token: 0x06000D2F RID: 3375 RVA: 0x00036C30 File Offset: 0x00034E30
    private void Awake()
    {
        ItemWishlist.Instance = this;
        QuestManager.onQuestListsChanged += this.OnQuestListChanged;
        BuildingManager.OnBuildingListChanged += this.OnBuildingListChanged;
        SavesSystem.OnCollectSaveData += this.Save;
        UIInputManager.OnWishlistHoveringItem += this.OnWishlistHoveringItem;
        this.Load();
    }

    // Token: 0x06000D30 RID: 3376 RVA: 0x00036C8D File Offset: 0x00034E8D
    private void OnDestroy()
    {
        QuestManager.onQuestListsChanged -= this.OnQuestListChanged;
        SavesSystem.OnCollectSaveData -= this.Save;
        UIInputManager.OnWishlistHoveringItem -= this.OnWishlistHoveringItem;
    }

    // Token: 0x06000D31 RID: 3377 RVA: 0x00036CC4 File Offset: 0x00034EC4
    private void OnWishlistHoveringItem(UIInputEventData data)
    {
        if (!ItemHoveringUI.Shown)
        {
            return;
        }
        int displayingItemID = ItemHoveringUI.DisplayingItemID;
        if (this.IsManuallyWishlisted(displayingItemID))
        {
            ItemWishlist.RemoveFromWishlist(displayingItemID);
        }
        else
        {
            ItemWishlist.AddToWishList(displayingItemID);
        }
        ItemHoveringUI.NotifyRefreshWishlistInfo();
    }

    // Token: 0x06000D32 RID: 3378 RVA: 0x00036CFC File Offset: 0x00034EFC
    private void Load()
    {
        this.manualWishList.Clear();
        List<int> list = SavesSystem.Load<List<int>>("ItemWishlist_Manual");
        if (list != null)
        {
            this.manualWishList.AddRange(list);
        }
    }

    // Token: 0x06000D33 RID: 3379 RVA: 0x00036D2E File Offset: 0x00034F2E
    private void Save()
    {
        SavesSystem.Save<List<int>>("ItemWishlist_Manual", this.manualWishList);
    }

    // Token: 0x06000D34 RID: 3380 RVA: 0x00036D40 File Offset: 0x00034F40
    private void Start()
    {
        this.CacheQuestItems();
        this.CacheBuildingItems();
    }

    // Token: 0x06000D35 RID: 3381 RVA: 0x00036D4E File Offset: 0x00034F4E
    private void OnQuestListChanged(QuestManager obj)
    {
        this.CacheQuestItems();
    }

    // Token: 0x06000D36 RID: 3382 RVA: 0x00036D56 File Offset: 0x00034F56
    private void OnBuildingListChanged()
    {
        this.CacheBuildingItems();
    }

    // Token: 0x06000D37 RID: 3383 RVA: 0x00036D5E File Offset: 0x00034F5E
    private void CacheQuestItems()
    {
        this._questRequiredItems = QuestManager.GetAllRequiredItems().ToHashSet<int>();
    }

    // Token: 0x06000D38 RID: 3384 RVA: 0x00036D70 File Offset: 0x00034F70
    private void CacheBuildingItems()
    {
        this._buildingRequiredItems.Clear();
        foreach (BuildingInfo buildingInfo in BuildingSelectionPanel.GetBuildingsToDisplay())
        {
            if (
                buildingInfo.RequirementsSatisfied()
                && buildingInfo.TokenAmount + buildingInfo.CurrentAmount < buildingInfo.maxAmount
            )
            {
                foreach (Cost.ItemEntry itemEntry in buildingInfo.cost.items)
                {
                    this._buildingRequiredItems.Add(itemEntry.id);
                }
            }
        }
    }

    // Token: 0x06000D39 RID: 3385 RVA: 0x00036DFF File Offset: 0x00034FFF
    private IEnumerable<int> IterateAll()
    {
        foreach (int num in this.manualWishList)
        {
            yield return num;
        }
        List<int>.Enumerator enumerator = default(List<int>.Enumerator);
        IEnumerable<int> allRequiredItems = QuestManager.GetAllRequiredItems();
        foreach (int num2 in allRequiredItems)
        {
            yield return num2;
        }
        IEnumerator<int> enumerator2 = null;
        yield break;
        yield break;
    }

    // Token: 0x06000D3A RID: 3386 RVA: 0x00036E0F File Offset: 0x0003500F
    public bool IsQuestRequired(int itemTypeID)
    {
        return this._questRequiredItems.Contains(itemTypeID);
    }

    // Token: 0x06000D3B RID: 3387 RVA: 0x00036E1D File Offset: 0x0003501D
    public bool IsManuallyWishlisted(int itemTypeID)
    {
        return this.manualWishList.Contains(itemTypeID);
    }

    // Token: 0x06000D3C RID: 3388 RVA: 0x00036E2B File Offset: 0x0003502B
    public bool IsBuildingRequired(int itemTypeID)
    {
        return this._buildingRequiredItems.Contains(itemTypeID);
    }

    // Token: 0x06000D3D RID: 3389 RVA: 0x00036E3C File Offset: 0x0003503C
    public static void AddToWishList(int itemTypeID)
    {
        if (ItemWishlist.Instance == null)
        {
            return;
        }
        if (ItemWishlist.Instance.manualWishList.Contains(itemTypeID))
        {
            return;
        }
        ItemWishlist.Instance.manualWishList.Add(itemTypeID);
        Action<int> onWishlistChanged = ItemWishlist.OnWishlistChanged;
        if (onWishlistChanged == null)
        {
            return;
        }
        onWishlistChanged(itemTypeID);
    }

    // Token: 0x06000D3E RID: 3390 RVA: 0x00036E8A File Offset: 0x0003508A
    public static bool RemoveFromWishlist(int itemTypeID)
    {
        if (ItemWishlist.Instance == null)
        {
            return false;
        }
        bool flag = ItemWishlist.Instance.manualWishList.Remove(itemTypeID);
        if (flag)
        {
            Action<int> onWishlistChanged = ItemWishlist.OnWishlistChanged;
            if (onWishlistChanged == null)
            {
                return flag;
            }
            onWishlistChanged(itemTypeID);
        }
        return flag;
    }

    // Token: 0x06000D3F RID: 3391 RVA: 0x00036EC0 File Offset: 0x000350C0
    public static ItemWishlist.WishlistInfo GetWishlistInfo(int itemTypeID)
    {
        if (ItemWishlist.Instance == null)
        {
            return default(ItemWishlist.WishlistInfo);
        }
        bool isManuallyWishlisted = ItemWishlist.Instance.IsManuallyWishlisted(itemTypeID);
        bool isQuestRequired = ItemWishlist.Instance.IsQuestRequired(itemTypeID);
        bool isBuildingRequired = ItemWishlist.Instance.IsBuildingRequired(itemTypeID);
        return new ItemWishlist.WishlistInfo
        {
            itemTypeID = itemTypeID,
            isManuallyWishlisted = isManuallyWishlisted,
            isQuestRequired = isQuestRequired,
            isBuildingRequired = isBuildingRequired,
        };
    }

    // Token: 0x04000B4A RID: 2890
    private List<int> manualWishList = new List<int>();

    // Token: 0x04000B4B RID: 2891
    private HashSet<int> _questRequiredItems = new HashSet<int>();

    // Token: 0x04000B4C RID: 2892
    private HashSet<int> _buildingRequiredItems = new HashSet<int>();

    // Token: 0x04000B4E RID: 2894
    private const string SaveKey = "ItemWishlist_Manual";

    // Token: 0x020004CC RID: 1228
    public struct WishlistInfo
    {
        // Token: 0x04001CC9 RID: 7369
        public int itemTypeID;

        // Token: 0x04001CCA RID: 7370
        public bool isManuallyWishlisted;

        // Token: 0x04001CCB RID: 7371
        public bool isQuestRequired;

        // Token: 0x04001CCC RID: 7372
        public bool isBuildingRequired;
    }
}
