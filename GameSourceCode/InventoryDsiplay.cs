using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI
{
	// Token: 0x0200038B RID: 907
	public class InventoryDisplay : MonoBehaviour, IPoolable
	{
		// Token: 0x1700060C RID: 1548
		// (get) Token: 0x06001F85 RID: 8069 RVA: 0x0006E333 File Offset: 0x0006C533
		private bool shortcuts
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700060D RID: 1549
		// (get) Token: 0x06001F86 RID: 8070 RVA: 0x0006E336 File Offset: 0x0006C536
		public bool UsePages
		{
			get
			{
				return this.usePages;
			}
		}

		// Token: 0x1700060E RID: 1550
		// (get) Token: 0x06001F87 RID: 8071 RVA: 0x0006E33E File Offset: 0x0006C53E
		// (set) Token: 0x06001F88 RID: 8072 RVA: 0x0006E346 File Offset: 0x0006C546
		public bool Editable
		{
			get
			{
				return this.editable;
			}
			internal set
			{
				this.editable = value;
			}
		}

		// Token: 0x1700060F RID: 1551
		// (get) Token: 0x06001F89 RID: 8073 RVA: 0x0006E34F File Offset: 0x0006C54F
		// (set) Token: 0x06001F8A RID: 8074 RVA: 0x0006E357 File Offset: 0x0006C557
		public bool ShowOperationButtons
		{
			get
			{
				return this.showOperationButtons;
			}
			internal set
			{
				this.showOperationButtons = value;
			}
		}

		// Token: 0x17000610 RID: 1552
		// (get) Token: 0x06001F8B RID: 8075 RVA: 0x0006E360 File Offset: 0x0006C560
		// (set) Token: 0x06001F8C RID: 8076 RVA: 0x0006E368 File Offset: 0x0006C568
		public bool Movable { get; private set; }

		// Token: 0x17000611 RID: 1553
		// (get) Token: 0x06001F8D RID: 8077 RVA: 0x0006E371 File Offset: 0x0006C571
		// (set) Token: 0x06001F8E RID: 8078 RVA: 0x0006E379 File Offset: 0x0006C579
		public Inventory Target { get; private set; }

		// Token: 0x17000612 RID: 1554
		// (get) Token: 0x06001F8F RID: 8079 RVA: 0x0006E384 File Offset: 0x0006C584
		private PrefabPool<InventoryEntry> EntryPool
		{
			get
			{
				if (this._entryPool == null && this.entryPrefab != null)
				{
					this._entryPool = new PrefabPool<InventoryEntry>(this.entryPrefab, this.contentLayout.transform, null, null, null, true, 10, 10000, null);
				}
				return this._entryPool;
			}
		}

		// Token: 0x140000D3 RID: 211
		// (add) Token: 0x06001F90 RID: 8080 RVA: 0x0006E3D8 File Offset: 0x0006C5D8
		// (remove) Token: 0x06001F91 RID: 8081 RVA: 0x0006E410 File Offset: 0x0006C610
		public event Action<InventoryDisplay, InventoryEntry, PointerEventData> onDisplayDoubleClicked;

		// Token: 0x140000D4 RID: 212
		// (add) Token: 0x06001F92 RID: 8082 RVA: 0x0006E448 File Offset: 0x0006C648
		// (remove) Token: 0x06001F93 RID: 8083 RVA: 0x0006E480 File Offset: 0x0006C680
		public event Action onPageInfoRefreshed;

		// Token: 0x17000613 RID: 1555
		// (get) Token: 0x06001F94 RID: 8084 RVA: 0x0006E4B5 File Offset: 0x0006C6B5
		public Func<Item, bool> Func_ShouldHighlight
		{
			get
			{
				return this._func_ShouldHighlight;
			}
		}

		// Token: 0x17000614 RID: 1556
		// (get) Token: 0x06001F95 RID: 8085 RVA: 0x0006E4BD File Offset: 0x0006C6BD
		public Func<Item, bool> Func_CanOperate
		{
			get
			{
				return this._func_CanOperate;
			}
		}

		// Token: 0x17000615 RID: 1557
		// (get) Token: 0x06001F96 RID: 8086 RVA: 0x0006E4C5 File Offset: 0x0006C6C5
		// (set) Token: 0x06001F97 RID: 8087 RVA: 0x0006E4CD File Offset: 0x0006C6CD
		public bool ShowSortButton
		{
			get
			{
				return this.showSortButton;
			}
			internal set
			{
				this.showSortButton = value;
			}
		}

		// Token: 0x06001F98 RID: 8088 RVA: 0x0006E4D8 File Offset: 0x0006C6D8
		private void RegisterEvents()
		{
			if (this.Target == null)
			{
				return;
			}
			this.UnregisterEvents();
			this.Target.onContentChanged += this.OnTargetContentChanged;
			this.Target.onInventorySorted += this.OnTargetSorted;
			this.Target.onSetIndexLock += this.OnTargetSetIndexLock;
		}

		// Token: 0x06001F99 RID: 8089 RVA: 0x0006E540 File Offset: 0x0006C740
		private void UnregisterEvents()
		{
			if (this.Target == null)
			{
				return;
			}
			this.Target.onContentChanged -= this.OnTargetContentChanged;
			this.Target.onInventorySorted -= this.OnTargetSorted;
			this.Target.onSetIndexLock -= this.OnTargetSetIndexLock;
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x0006E5A4 File Offset: 0x0006C7A4
		private void OnTargetSetIndexLock(Inventory inventory, int index)
		{
			foreach (InventoryEntry inventoryEntry in this.entries)
			{
				if (!(inventoryEntry == null) && inventoryEntry.isActiveAndEnabled && inventoryEntry.Index == index)
				{
					inventoryEntry.Refresh();
				}
			}
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x0006E610 File Offset: 0x0006C810
		private void OnTargetSorted(Inventory inventory)
		{
			if (this.filter == null)
			{
				using (List<InventoryEntry>.Enumerator enumerator = this.entries.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						InventoryEntry inventoryEntry = enumerator.Current;
						inventoryEntry.Refresh();
					}
					return;
				}
			}
			this.LoadEntriesTask().Forget();
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x0006E674 File Offset: 0x0006C874
		private void OnTargetContentChanged(Inventory inventory, int position)
		{
			if (this.Target.Loading)
			{
				return;
			}
			if (this.filter != null)
			{
				this.RefreshCapacityText();
				this.LoadEntriesTask().Forget();
				return;
			}
			this.RefreshCapacityText();
			InventoryEntry inventoryEntry = this.entries.Find((InventoryEntry e) => e != null && e.Index == position);
			if (!inventoryEntry)
			{
				return;
			}
			InventoryEntry inventoryEntry2 = inventoryEntry;
			inventoryEntry2.Refresh();
			inventoryEntry2.Punch();
		}

		// Token: 0x06001F9D RID: 8093 RVA: 0x0006E6EC File Offset: 0x0006C8EC
		private void RefreshCapacityText()
		{
			if (this.Target == null)
			{
				return;
			}
			if (!this.capacityText)
			{
				return;
			}
			this.capacityText.text = string.Format(this.capacityTextFormat, this.Target.Capacity, this.Target.GetItemCount());
		}

		// Token: 0x06001F9E RID: 8094 RVA: 0x0006E74C File Offset: 0x0006C94C
		public void Setup(Inventory target, Func<Item, bool> funcShouldHighLight = null, Func<Item, bool> funcCanOperate = null, bool movable = false, Func<Item, bool> filter = null)
		{
			this.UnregisterEvents();
			this.Target = target;
			this.Clear();
			if (this.Target == null)
			{
				return;
			}
			if (this.Target.Loading)
			{
				return;
			}
			if (funcShouldHighLight == null)
			{
				this._func_ShouldHighlight = ((Item e) => false);
			}
			else
			{
				this._func_ShouldHighlight = funcShouldHighLight;
			}
			if (funcCanOperate == null)
			{
				this._func_CanOperate = ((Item e) => true);
			}
			else
			{
				this._func_CanOperate = funcCanOperate;
			}
			this.displayNameText.text = target.DisplayName;
			this.Movable = movable;
			this.cachedCapacity = target.Capacity;
			this.filter = filter;
			this.RefreshCapacityText();
			this.RegisterEvents();
			this.sortButton.gameObject.SetActive(this.editable && this.showSortButton);
			this.LoadEntriesTask().Forget();
		}

		// Token: 0x06001F9F RID: 8095 RVA: 0x0006E850 File Offset: 0x0006CA50
		private void RefreshGridLayoutPreferredHeight()
		{
			if (this.Target == null)
			{
				this.placeHolder.gameObject.SetActive(true);
				return;
			}
			int num = this.cachedIndexesToDisplay.Count;
			if (this.usePages && num > 0)
			{
				int num2 = this.cachedSelectedPage * this.itemsEachPage;
				int num3 = Mathf.Min(num2 + this.itemsEachPage, this.cachedIndexesToDisplay.Count);
				num = Mathf.Max(0, num3 - num2);
			}
			float preferredHeight = (float)Mathf.CeilToInt((float)num / (float)this.contentLayout.constraintCount) * this.contentLayout.cellSize.y + (float)this.contentLayout.padding.top + (float)this.contentLayout.padding.bottom;
			this.gridLayoutElement.preferredHeight = preferredHeight;
			this.placeHolder.gameObject.SetActive(num <= 0);
		}

		// Token: 0x17000616 RID: 1558
		// (get) Token: 0x06001FA0 RID: 8096 RVA: 0x0006E934 File Offset: 0x0006CB34
		public int MaxPage
		{
			get
			{
				return this.cachedMaxPage;
			}
		}

		// Token: 0x17000617 RID: 1559
		// (get) Token: 0x06001FA1 RID: 8097 RVA: 0x0006E93C File Offset: 0x0006CB3C
		public int SelectedPage
		{
			get
			{
				return this.cachedSelectedPage;
			}
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x0006E944 File Offset: 0x0006CB44
		public void SetPage(int page)
		{
			this.cachedSelectedPage = page;
			Action action = this.onPageInfoRefreshed;
			if (action != null)
			{
				action();
			}
			this.LoadEntriesTask().Forget();
		}

		// Token: 0x06001FA3 RID: 8099 RVA: 0x0006E96C File Offset: 0x0006CB6C
		public void NextPage()
		{
			int num = this.cachedSelectedPage + 1;
			if (num >= this.cachedMaxPage)
			{
				num = 0;
			}
			this.SetPage(num);
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x0006E994 File Offset: 0x0006CB94
		public void PreviousPage()
		{
			int num = this.cachedSelectedPage - 1;
			if (num < 0)
			{
				num = this.cachedMaxPage - 1;
			}
			this.SetPage(num);
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x0006E9C0 File Offset: 0x0006CBC0
		private void CacheIndexesToDisplay()
		{
			this.cachedIndexesToDisplay.Clear();
			int i = 0;
			while (i < this.Target.Capacity)
			{
				if (this.filter == null)
				{
					goto IL_32;
				}
				Item itemAt = this.Target.GetItemAt(i);
				if (this.filter(itemAt))
				{
					goto IL_32;
				}
				IL_3E:
				i++;
				continue;
				IL_32:
				this.cachedIndexesToDisplay.Add(i);
				goto IL_3E;
			}
			int count = this.cachedIndexesToDisplay.Count;
			this.cachedMaxPage = count / this.itemsEachPage + ((count % this.itemsEachPage > 0) ? 1 : 0);
			if (this.cachedSelectedPage >= this.cachedMaxPage)
			{
				this.cachedSelectedPage = Mathf.Max(0, this.cachedMaxPage - 1);
			}
			Action action = this.onPageInfoRefreshed;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x0006EA7C File Offset: 0x0006CC7C
		private UniTask LoadEntriesTask()
		{
			InventoryDisplay.<LoadEntriesTask>d__76 <LoadEntriesTask>d__;
			<LoadEntriesTask>d__.<>t__builder = AsyncUniTaskMethodBuilder.Create();
			<LoadEntriesTask>d__.<>4__this = this;
			<LoadEntriesTask>d__.<>1__state = -1;
			<LoadEntriesTask>d__.<>t__builder.Start<InventoryDisplay.<LoadEntriesTask>d__76>(ref <LoadEntriesTask>d__);
			return <LoadEntriesTask>d__.<>t__builder.Task;
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x0006EABF File Offset: 0x0006CCBF
		public void SetFilter(Func<Item, bool> filter)
		{
			this.filter = filter;
			this.cachedSelectedPage = 0;
			this.LoadEntriesTask().Forget();
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x0006EADA File Offset: 0x0006CCDA
		private void Clear()
		{
			this.EntryPool.ReleaseAll();
			this.entries.Clear();
		}

		// Token: 0x06001FA9 RID: 8105 RVA: 0x0006EAF2 File Offset: 0x0006CCF2
		private void Awake()
		{
			this.sortButton.onClick.AddListener(new UnityAction(this.OnSortButtonClicked));
		}

		// Token: 0x06001FAA RID: 8106 RVA: 0x0006EB10 File Offset: 0x0006CD10
		private void OnSortButtonClicked()
		{
			if (!this.Editable)
			{
				return;
			}
			if (!this.Target)
			{
				return;
			}
			if (this.Target.Loading)
			{
				return;
			}
			this.Target.Sort();
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x0006EB42 File Offset: 0x0006CD42
		private void OnEnable()
		{
			this.RegisterEvents();
		}

		// Token: 0x06001FAC RID: 8108 RVA: 0x0006EB4A File Offset: 0x0006CD4A
		private void OnDisable()
		{
			this.UnregisterEvents();
			this.activeTaskToken++;
		}

		// Token: 0x06001FAD RID: 8109 RVA: 0x0006EB60 File Offset: 0x0006CD60
		private void Update()
		{
			if (this.Target && this.cachedCapacity != this.Target.Capacity)
			{
				this.OnCapacityChanged();
			}
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x0006EB88 File Offset: 0x0006CD88
		private void OnCapacityChanged()
		{
			if (this.Target == null)
			{
				return;
			}
			this.cachedCapacity = this.Target.Capacity;
			this.RefreshCapacityText();
			this.LoadEntriesTask().Forget();
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x0006EBBB File Offset: 0x0006CDBB
		public bool IsShortcut(int index)
		{
			return this.shortcuts && index >= this.shortcutsRange.x && index <= this.shortcutsRange.y;
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x0006EBE8 File Offset: 0x0006CDE8
		private InventoryEntry GetNewInventoryEntry()
		{
			return this.EntryPool.Get(null);
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x0006EBF6 File Offset: 0x0006CDF6
		internal void NotifyItemDoubleClicked(InventoryEntry inventoryEntry, PointerEventData data)
		{
			Action<InventoryDisplay, InventoryEntry, PointerEventData> action = this.onDisplayDoubleClicked;
			if (action == null)
			{
				return;
			}
			action(this, inventoryEntry, data);
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x0006EC0B File Offset: 0x0006CE0B
		public void NotifyPooled()
		{
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x0006EC0D File Offset: 0x0006CE0D
		public void NotifyReleased()
		{
		}

		// Token: 0x06001FB4 RID: 8116 RVA: 0x0006EC10 File Offset: 0x0006CE10
		public void DisableItem(Item item)
		{
			foreach (InventoryEntry inventoryEntry in from e in this.entries
			where e.Content == item
			select e)
			{
				inventoryEntry.Disabled = true;
			}
		}

		// Token: 0x06001FB5 RID: 8117 RVA: 0x0006EC7C File Offset: 0x0006CE7C
		internal bool EvaluateShouldHighlight(Item content)
		{
			if (this.Func_ShouldHighlight != null && this.Func_ShouldHighlight(content))
			{
				return true;
			}
			content == null;
			return false;
		}

		// Token: 0x06001FB7 RID: 8119 RVA: 0x0006ED05 File Offset: 0x0006CF05
		[CompilerGenerated]
		private bool <LoadEntriesTask>g__TaskValid|76_0(ref InventoryDisplay.<>c__DisplayClass76_0 A_1)
		{
			return Application.isPlaying && A_1.token == this.activeTaskToken;
		}

		// Token: 0x06001FB8 RID: 8120 RVA: 0x0006ED20 File Offset: 0x0006CF20
		[CompilerGenerated]
		private List<int> <LoadEntriesTask>g__GetRange|76_1(int begin, int end_exclusive, List<int> list, ref InventoryDisplay.<>c__DisplayClass76_0 A_4)
		{
			if (begin < 0)
			{
				begin = 0;
			}
			if (end_exclusive < 0)
			{
				end_exclusive = 0;
			}
			A_4.indexes = new List<int>();
			if (end_exclusive > list.Count)
			{
				end_exclusive = list.Count;
			}
			if (begin >= end_exclusive)
			{
				return A_4.indexes;
			}
			for (int i = begin; i < end_exclusive; i++)
			{
				A_4.indexes.Add(list[i]);
			}
			return A_4.indexes;
		}

		// Token: 0x04001591 RID: 5521
		[SerializeField]
		private InventoryEntry entryPrefab;

		// Token: 0x04001592 RID: 5522
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04001593 RID: 5523
		[SerializeField]
		private TextMeshProUGUI capacityText;

		// Token: 0x04001594 RID: 5524
		[SerializeField]
		private string capacityTextFormat = "({1}/{0})";

		// Token: 0x04001595 RID: 5525
		[SerializeField]
		private FadeGroup loadingIndcator;

		// Token: 0x04001596 RID: 5526
		[SerializeField]
		private FadeGroup contentFadeGroup;

		// Token: 0x04001597 RID: 5527
		[SerializeField]
		private GridLayoutGroup contentLayout;

		// Token: 0x04001598 RID: 5528
		[SerializeField]
		private LayoutElement gridLayoutElement;

		// Token: 0x04001599 RID: 5529
		[SerializeField]
		private GameObject placeHolder;

		// Token: 0x0400159A RID: 5530
		[SerializeField]
		private Transform entriesParent;

		// Token: 0x0400159B RID: 5531
		[SerializeField]
		private Button sortButton;

		// Token: 0x0400159C RID: 5532
		[SerializeField]
		private Vector2Int shortcutsRange = new Vector2Int(0, 3);

		// Token: 0x0400159D RID: 5533
		[SerializeField]
		private bool editable = true;

		// Token: 0x0400159E RID: 5534
		[SerializeField]
		private bool showOperationButtons = true;

		// Token: 0x0400159F RID: 5535
		[SerializeField]
		private bool showSortButton;

		// Token: 0x040015A0 RID: 5536
		[SerializeField]
		private bool usePages;

		// Token: 0x040015A1 RID: 5537
		[SerializeField]
		private int itemsEachPage = 30;

		// Token: 0x040015A2 RID: 5538
		public Func<Item, bool> filter;

		// Token: 0x040015A5 RID: 5541
		[SerializeField]
		private List<InventoryEntry> entries = new List<InventoryEntry>();

		// Token: 0x040015A6 RID: 5542
		private PrefabPool<InventoryEntry> _entryPool;

		// Token: 0x040015A9 RID: 5545
		private Func<Item, bool> _func_ShouldHighlight;

		// Token: 0x040015AA RID: 5546
		private Func<Item, bool> _func_CanOperate;

		// Token: 0x040015AB RID: 5547
		private int cachedCapacity = -1;

		// Token: 0x040015AC RID: 5548
		private int activeTaskToken;

		// Token: 0x040015AD RID: 5549
		private int cachedMaxPage = 1;

		// Token: 0x040015AE RID: 5550
		private int cachedSelectedPage;

		// Token: 0x040015AF RID: 5551
		private List<int> cachedIndexesToDisplay = new List<int>();
	}
}
