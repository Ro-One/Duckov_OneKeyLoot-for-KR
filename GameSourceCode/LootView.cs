using System;
using System.Collections.Generic;
using Duckov.UI.Animations;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI
{
	// Token: 0x020003B3 RID: 947
	public class LootView : View
	{
		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x06002238 RID: 8760 RVA: 0x000777D6 File Offset: 0x000759D6
		public static LootView Instance
		{
			get
			{
				return View.GetViewInstance<LootView>();
			}
		}

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x06002239 RID: 8761 RVA: 0x000777DD File Offset: 0x000759DD
		private CharacterMainControl Character
		{
			get
			{
				return LevelManager.Instance.MainCharacter;
			}
		}

		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x0600223A RID: 8762 RVA: 0x000777E9 File Offset: 0x000759E9
		private Item CharacterItem
		{
			get
			{
				if (this.Character == null)
				{
					return null;
				}
				return this.Character.CharacterItem;
			}
		}

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x0600223B RID: 8763 RVA: 0x00077806 File Offset: 0x00075A06
		public Inventory TargetInventory
		{
			get
			{
				if (this.targetLootBox != null)
				{
					return this.targetLootBox.Inventory;
				}
				if (this.targetInventory)
				{
					return this.targetInventory;
				}
				return null;
			}
		}

		// Token: 0x0600223C RID: 8764 RVA: 0x00077837 File Offset: 0x00075A37
		public static bool HasInventoryEverBeenLooted(Inventory inventory)
		{
			return !(LootView.Instance == null) && LootView.Instance.lootedInventories != null && !(inventory == null) && LootView.Instance.lootedInventories.Contains(inventory);
		}

		// Token: 0x0600223D RID: 8765 RVA: 0x00077874 File Offset: 0x00075A74
		protected override void Awake()
		{
			base.Awake();
			InteractableLootbox.OnStartLoot += this.OnStartLoot;
			this.pickAllButton.onClick.AddListener(new UnityAction(this.OnPickAllButtonClicked));
			CharacterMainControl.OnMainCharacterStartUseItem += this.OnMainCharacterStartUseItem;
			LevelManager.OnMainCharacterDead += this.OnMainCharacterDead;
			this.storeAllButton.onClick.AddListener(new UnityAction(this.OnStoreAllButtonClicked));
		}

		// Token: 0x0600223E RID: 8766 RVA: 0x000778F4 File Offset: 0x00075AF4
		private void OnStoreAllButtonClicked()
		{
			if (this.TargetInventory == null)
			{
				return;
			}
			if (this.TargetInventory != PlayerStorage.Inventory)
			{
				return;
			}
			if (this.CharacterItem == null)
			{
				return;
			}
			Inventory inventory = this.CharacterItem.Inventory;
			if (inventory == null)
			{
				return;
			}
			int lastItemPosition = inventory.GetLastItemPosition();
			for (int i = 0; i <= lastItemPosition; i++)
			{
				if (!inventory.lockedIndexes.Contains(i))
				{
					Item itemAt = inventory.GetItemAt(i);
					if (!(itemAt == null))
					{
						if (!this.TargetInventory.AddAndMerge(itemAt, 0))
						{
							break;
						}
						if (i == 0)
						{
							AudioManager.PlayPutItemSFX(itemAt, false);
						}
					}
				}
			}
		}

		// Token: 0x0600223F RID: 8767 RVA: 0x00077993 File Offset: 0x00075B93
		protected override void OnDestroy()
		{
			this.UnregisterEvents();
			InteractableLootbox.OnStartLoot -= this.OnStartLoot;
			LevelManager.OnMainCharacterDead -= this.OnMainCharacterDead;
			base.OnDestroy();
		}

		// Token: 0x06002240 RID: 8768 RVA: 0x000779C3 File Offset: 0x00075BC3
		private void OnMainCharacterStartUseItem(Item _item)
		{
			if (base.open)
			{
				base.Close();
			}
		}

		// Token: 0x06002241 RID: 8769 RVA: 0x000779D3 File Offset: 0x00075BD3
		private void OnMainCharacterDead(DamageInfo dmgInfo)
		{
			if (base.open)
			{
				base.Close();
			}
		}

		// Token: 0x06002242 RID: 8770 RVA: 0x000779E3 File Offset: 0x00075BE3
		private void OnEnable()
		{
			this.RegisterEvents();
		}

		// Token: 0x06002243 RID: 8771 RVA: 0x000779EB File Offset: 0x00075BEB
		private void OnDisable()
		{
			this.UnregisterEvents();
			InteractableLootbox interactableLootbox = this.targetLootBox;
			if (interactableLootbox != null)
			{
				interactableLootbox.StopInteract();
			}
			this.targetLootBox = null;
		}

		// Token: 0x06002244 RID: 8772 RVA: 0x00077A0B File Offset: 0x00075C0B
		public void Show()
		{
			base.Open(null);
		}

		// Token: 0x06002245 RID: 8773 RVA: 0x00077A14 File Offset: 0x00075C14
		private void OnStartLoot(InteractableLootbox lootbox)
		{
			this.targetLootBox = lootbox;
			if (this.targetLootBox == null || this.targetLootBox.Inventory == null)
			{
				Debug.LogError("Target loot box could not be found");
				return;
			}
			base.Open(null);
			if (this.TargetInventory != null)
			{
				this.lootedInventories.Add(this.TargetInventory);
			}
		}

		// Token: 0x06002246 RID: 8774 RVA: 0x00077A7B File Offset: 0x00075C7B
		private void OnStopLoot(InteractableLootbox lootbox)
		{
			if (lootbox == this.targetLootBox)
			{
				this.targetLootBox = null;
				base.Close();
			}
		}

		// Token: 0x06002247 RID: 8775 RVA: 0x00077A98 File Offset: 0x00075C98
		public static void LootItem(Item item)
		{
			if (item == null)
			{
				return;
			}
			if (LootView.Instance == null)
			{
				return;
			}
			LootView.Instance.targetInventory = item.Inventory;
			LootView.Instance.Open(null);
		}

		// Token: 0x06002248 RID: 8776 RVA: 0x00077AD0 File Offset: 0x00075CD0
		protected override void OnOpen()
		{
			base.OnOpen();
			this.UnregisterEvents();
			base.gameObject.SetActive(true);
			this.characterSlotCollectionDisplay.Setup(this.CharacterItem, true);
			if (PetProxy.PetInventory)
			{
				this.petInventoryDisplay.gameObject.SetActive(true);
				this.petInventoryDisplay.Setup(PetProxy.PetInventory, null, null, false, null);
			}
			else
			{
				this.petInventoryDisplay.gameObject.SetActive(false);
			}
			this.characterInventoryDisplay.Setup(this.CharacterItem.Inventory, null, null, true, null);
			if (this.targetLootBox != null)
			{
				this.lootTargetInventoryDisplay.ShowSortButton = this.targetLootBox.ShowSortButton;
				this.lootTargetInventoryDisplay.Setup(this.TargetInventory, null, null, true, null);
				this.lootTargetDisplayName.text = this.TargetInventory.DisplayName;
				if (this.TargetInventory.GetComponent<InventoryFilterProvider>())
				{
					this.lootTargetFilterDisplay.gameObject.SetActive(true);
					this.lootTargetFilterDisplay.Setup(this.lootTargetInventoryDisplay);
					this.lootTargetFilterDisplay.Select(0);
				}
				else
				{
					this.lootTargetFilterDisplay.gameObject.SetActive(false);
				}
				this.lootTargetFadeGroup.Show();
			}
			else if (this.targetInventory != null)
			{
				this.lootTargetInventoryDisplay.ShowSortButton = false;
				this.lootTargetInventoryDisplay.Setup(this.TargetInventory, null, null, true, null);
				this.lootTargetFadeGroup.Show();
				this.lootTargetFilterDisplay.gameObject.SetActive(false);
			}
			else
			{
				this.lootTargetFadeGroup.SkipHide();
			}
			bool active = this.TargetInventory != null && this.TargetInventory == PlayerStorage.Inventory;
			this.storeAllButton.gameObject.SetActive(active);
			this.fadeGroup.Show();
			this.RefreshDetails();
			this.RefreshPickAllButton();
			this.RegisterEvents();
			this.RefreshCapacityText();
		}

		// Token: 0x06002249 RID: 8777 RVA: 0x00077CC8 File Offset: 0x00075EC8
		protected override void OnClose()
		{
			base.OnClose();
			this.fadeGroup.Hide();
			this.detailsFadeGroup.Hide();
			InteractableLootbox interactableLootbox = this.targetLootBox;
			if (interactableLootbox != null)
			{
				interactableLootbox.StopInteract();
			}
			this.targetLootBox = null;
			this.targetInventory = null;
			if (SplitDialogue.Instance && SplitDialogue.Instance.isActiveAndEnabled)
			{
				SplitDialogue.Instance.Cancel();
			}
			this.UnregisterEvents();
		}

		// Token: 0x0600224A RID: 8778 RVA: 0x00077D38 File Offset: 0x00075F38
		private void OnTargetInventoryContentChanged(Inventory inventory, int arg2)
		{
			this.RefreshPickAllButton();
			this.RefreshCapacityText();
		}

		// Token: 0x0600224B RID: 8779 RVA: 0x00077D48 File Offset: 0x00075F48
		private void RefreshCapacityText()
		{
			if (this.targetLootBox != null)
			{
				this.lootTargetCapacityText.text = this.lootTargetCapacityTextFormat.Format(new
				{
					itemCount = this.TargetInventory.GetItemCount(),
					capacity = this.TargetInventory.Capacity
				});
			}
		}

		// Token: 0x0600224C RID: 8780 RVA: 0x00077D94 File Offset: 0x00075F94
		private void RegisterEvents()
		{
			this.UnregisterEvents();
			ItemUIUtilities.OnSelectionChanged += this.OnSelectionChanged;
			this.lootTargetInventoryDisplay.onDisplayDoubleClicked += this.OnLootTargetItemDoubleClicked;
			this.characterInventoryDisplay.onDisplayDoubleClicked += this.OnCharacterInventoryItemDoubleClicked;
			this.petInventoryDisplay.onDisplayDoubleClicked += this.OnCharacterInventoryItemDoubleClicked;
			this.characterSlotCollectionDisplay.onElementDoubleClicked += this.OnCharacterSlotItemDoubleClicked;
			if (this.TargetInventory)
			{
				this.TargetInventory.onContentChanged += this.OnTargetInventoryContentChanged;
			}
			UIInputManager.OnNextPage += this.OnNextPage;
			UIInputManager.OnPreviousPage += this.OnPreviousPage;
		}

		// Token: 0x0600224D RID: 8781 RVA: 0x00077E5A File Offset: 0x0007605A
		private void OnPreviousPage(UIInputEventData data)
		{
			if (this.TargetInventory == null)
			{
				return;
			}
			if (!this.lootTargetInventoryDisplay.UsePages)
			{
				return;
			}
			this.lootTargetInventoryDisplay.PreviousPage();
		}

		// Token: 0x0600224E RID: 8782 RVA: 0x00077E84 File Offset: 0x00076084
		private void OnNextPage(UIInputEventData data)
		{
			if (this.TargetInventory == null)
			{
				return;
			}
			if (!this.lootTargetInventoryDisplay.UsePages)
			{
				return;
			}
			this.lootTargetInventoryDisplay.NextPage();
		}

		// Token: 0x0600224F RID: 8783 RVA: 0x00077EB0 File Offset: 0x000760B0
		private void UnregisterEvents()
		{
			ItemUIUtilities.OnSelectionChanged -= this.OnSelectionChanged;
			if (this.lootTargetInventoryDisplay)
			{
				this.lootTargetInventoryDisplay.onDisplayDoubleClicked -= this.OnLootTargetItemDoubleClicked;
			}
			if (this.characterInventoryDisplay)
			{
				this.characterInventoryDisplay.onDisplayDoubleClicked -= this.OnCharacterInventoryItemDoubleClicked;
			}
			if (this.petInventoryDisplay)
			{
				this.petInventoryDisplay.onDisplayDoubleClicked -= this.OnCharacterInventoryItemDoubleClicked;
			}
			if (this.characterSlotCollectionDisplay)
			{
				this.characterSlotCollectionDisplay.onElementDoubleClicked -= this.OnCharacterSlotItemDoubleClicked;
			}
			if (this.TargetInventory)
			{
				this.TargetInventory.onContentChanged -= this.OnTargetInventoryContentChanged;
			}
			UIInputManager.OnNextPage -= this.OnNextPage;
			UIInputManager.OnPreviousPage -= this.OnPreviousPage;
		}

		// Token: 0x06002250 RID: 8784 RVA: 0x00077FA4 File Offset: 0x000761A4
		private void OnCharacterSlotItemDoubleClicked(ItemSlotCollectionDisplay collectionDisplay, SlotDisplay slotDisplay)
		{
			if (slotDisplay == null)
			{
				return;
			}
			Slot target = slotDisplay.Target;
			if (target == null)
			{
				return;
			}
			Item content = target.Content;
			if (content == null)
			{
				return;
			}
			if (this.TargetInventory == null)
			{
				return;
			}
			if (content.Sticky && !this.TargetInventory.AcceptSticky)
			{
				return;
			}
			AudioManager.PlayPutItemSFX(content, false);
			content.Detach();
			if (this.TargetInventory.AddAndMerge(content, 0))
			{
				this.RefreshDetails();
				return;
			}
			Item x;
			if (!target.Plug(content, out x))
			{
				Debug.LogError("Failed plugging back!");
			}
			if (x != null)
			{
				Debug.Log("Unplugged item should be null!");
			}
			this.RefreshDetails();
		}

		// Token: 0x06002251 RID: 8785 RVA: 0x00078050 File Offset: 0x00076250
		private void OnCharacterInventoryItemDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
		{
			Item content = entry.Content;
			if (content == null)
			{
				return;
			}
			Inventory inInventory = content.InInventory;
			if (this.TargetInventory == null)
			{
				return;
			}
			if (content.Sticky && !this.TargetInventory.AcceptSticky)
			{
				return;
			}
			AudioManager.PlayPutItemSFX(content, false);
			content.Detach();
			if (this.TargetInventory.AddAndMerge(content, 0))
			{
				this.RefreshDetails();
				return;
			}
			if (!inInventory.AddAndMerge(content, 0))
			{
				Debug.LogError("Failed sending back item");
			}
			this.RefreshDetails();
		}

		// Token: 0x06002252 RID: 8786 RVA: 0x000780D7 File Offset: 0x000762D7
		private void OnSelectionChanged()
		{
			this.RefreshDetails();
		}

		// Token: 0x06002253 RID: 8787 RVA: 0x000780DF File Offset: 0x000762DF
		private void RefreshDetails()
		{
			if (ItemUIUtilities.SelectedItem != null)
			{
				this.detailsFadeGroup.Show();
				this.detailsDisplay.Setup(ItemUIUtilities.SelectedItem);
				return;
			}
			this.detailsFadeGroup.Hide();
		}

		// Token: 0x06002254 RID: 8788 RVA: 0x00078118 File Offset: 0x00076318
		private void OnLootTargetItemDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
		{
			Item item = entry.Item;
			if (item == null)
			{
				return;
			}
			if (!item.IsInPlayerCharacter())
			{
				if (this.targetLootBox != null && this.targetLootBox.needInspect && !item.Inspected)
				{
					data.Use();
					return;
				}
				data.Use();
				bool flag = false;
				LevelManager instance = LevelManager.Instance;
				bool? flag2;
				if (instance == null)
				{
					flag2 = null;
				}
				else
				{
					CharacterMainControl mainCharacter = instance.MainCharacter;
					if (mainCharacter == null)
					{
						flag2 = null;
					}
					else
					{
						Item characterItem = mainCharacter.CharacterItem;
						flag2 = ((characterItem != null) ? new bool?(characterItem.TryPlug(item, true, null, 0)) : null);
					}
				}
				bool? flag3 = flag2;
				flag |= flag3.Value;
				if (flag3 == null || !flag3.Value)
				{
					flag |= ItemUtilities.SendToPlayerCharacterInventory(item, false);
				}
				if (flag)
				{
					AudioManager.PlayPutItemSFX(item, false);
					this.RefreshDetails();
				}
			}
		}

		// Token: 0x06002255 RID: 8789 RVA: 0x000781F4 File Offset: 0x000763F4
		private void RefreshPickAllButton()
		{
			if (this.TargetInventory == null)
			{
				return;
			}
			this.pickAllButton.gameObject.SetActive(false);
			bool interactable = this.TargetInventory.GetItemCount() > 0;
			this.pickAllButton.interactable = interactable;
		}

		// Token: 0x06002256 RID: 8790 RVA: 0x0007823C File Offset: 0x0007643C
		private void OnPickAllButtonClicked()
		{
			if (this.TargetInventory == null)
			{
				return;
			}
			List<Item> list = new List<Item>();
			list.AddRange(this.TargetInventory);
			foreach (Item item in list)
			{
				if (!(item == null) && (!this.targetLootBox.needInspect || item.Inspected))
				{
					LevelManager instance = LevelManager.Instance;
					bool? flag;
					if (instance == null)
					{
						flag = null;
					}
					else
					{
						CharacterMainControl mainCharacter = instance.MainCharacter;
						if (mainCharacter == null)
						{
							flag = null;
						}
						else
						{
							Item characterItem = mainCharacter.CharacterItem;
							flag = ((characterItem != null) ? new bool?(characterItem.TryPlug(item, true, null, 0)) : null);
						}
					}
					bool? flag2 = flag;
					if (flag2 == null || !flag2.Value)
					{
						ItemUtilities.SendToPlayerCharacterInventory(item, false);
					}
				}
			}
			AudioManager.Post("UI/confirm");
		}

		// Token: 0x04001741 RID: 5953
		[SerializeField]
		private ItemSlotCollectionDisplay characterSlotCollectionDisplay;

		// Token: 0x04001742 RID: 5954
		[SerializeField]
		private InventoryDisplay characterInventoryDisplay;

		// Token: 0x04001743 RID: 5955
		[SerializeField]
		private InventoryDisplay petInventoryDisplay;

		// Token: 0x04001744 RID: 5956
		[SerializeField]
		private InventoryDisplay lootTargetInventoryDisplay;

		// Token: 0x04001745 RID: 5957
		[SerializeField]
		private InventoryFilterDisplay lootTargetFilterDisplay;

		// Token: 0x04001746 RID: 5958
		[SerializeField]
		private FadeGroup fadeGroup;

		// Token: 0x04001747 RID: 5959
		[SerializeField]
		private Button pickAllButton;

		// Token: 0x04001748 RID: 5960
		[SerializeField]
		private TextMeshProUGUI lootTargetDisplayName;

		// Token: 0x04001749 RID: 5961
		[SerializeField]
		private TextMeshProUGUI lootTargetCapacityText;

		// Token: 0x0400174A RID: 5962
		[SerializeField]
		private string lootTargetCapacityTextFormat = "({itemCount}/{capacity})";

		// Token: 0x0400174B RID: 5963
		[SerializeField]
		private Button storeAllButton;

		// Token: 0x0400174C RID: 5964
		[SerializeField]
		private FadeGroup lootTargetFadeGroup;

		// Token: 0x0400174D RID: 5965
		[SerializeField]
		private ItemDetailsDisplay detailsDisplay;

		// Token: 0x0400174E RID: 5966
		[SerializeField]
		private FadeGroup detailsFadeGroup;

		// Token: 0x0400174F RID: 5967
		[SerializeField]
		private InteractableLootbox targetLootBox;

		// Token: 0x04001750 RID: 5968
		private Inventory targetInventory;

		// Token: 0x04001751 RID: 5969
		private HashSet<Inventory> lootedInventories = new HashSet<Inventory>();
	}
}
