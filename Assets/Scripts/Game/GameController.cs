using DG.Tweening;
using FloatSakujyo.Level;
using FloatSakujyo.SaveData;
using FloatSakujyo.UI;
using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FloatSakujyo.Game
{
    public class GameController : GameControllerBase
    {
        public static GameController Instance { get; private set; }

        public LevelData LevelData { get; private set; }

        public LevelEntity LevelEntity { get; private set; }

        public InputController InputController { get; private set; }

        [SerializeField]
        Canvas colorGroupSlotViewCanvas;
        public Canvas ColorGroupSlotViewCanvas => colorGroupSlotViewCanvas;

        [SerializeField]
        protected ColorGroupSlotView colorGroupSlotView;

        [SerializeField]
        Slover slover;

        [SerializeField]
        GameObject icePrototype;
        [SerializeField]
        int iceCount;

        [SerializeField]
        Vector2 positionArea;

        int itemGridCount;
        ItemGrid[] itemGrids;

        List<Item> edenItemQueue;

        public HashSet<Item> OldItems { get; private set; }

        [SerializeField]
        float itemMass;

        [SerializeField]
        WaterLevel waterLevel;

        [SerializeField]
        float waveInterval;

        bool isFaild;
        bool isCompleted;

        [SerializeField]
        float refAspect1;
        [SerializeField]
        float refAspect2;
        float runtimeAspectRatio;

        [SerializeField]
        Vector2 refCameraOrthoSize;
        [SerializeField]
        Vector2 refColorGroupSlotViewWidth;
        [SerializeField]
        Vector2 refNoneColorGroupSlotViewWidth;
        [SerializeField]
        Vector3 refNoneColorGroupSlotOffset;

        Transform backupContent;
        Transform iceContent;

        ColorGroupSloter colorGroupSloter;

        public event Action OnItemTook;

        int subLevelID;
        int tookItemCount;
        public int TotalItemCount { get; private set; }
        protected override void Internal_FreezeGameplay()
        {

        }

        protected override void Internal_ResumeGameplay()
        {

        }

        private void Awake()
        {
            Instance = this;

            InitAspectRatio();

            InitInputController();

            InitCamera();

            InitSlover();

            InitColorGroupSlotView();

            waterLevel.Init(waveInterval);

            var levelID = GameDataManager.Instance.GetLevelID();
            LevelData = LevelDataManager.Instance.GetData(levelID);
            var levelSubID = GameDataManager.Instance.GetSubLevelID();
            StartCoroutine(StartLevel(levelSubID));
        }

        void InitAspectRatio()
        {
            var runtimeAspect = (float)Screen.width / Screen.height;
            runtimeAspectRatio = (runtimeAspect - refAspect1) / (refAspect2 - refAspect1);
        }

        void InitInputController()
        {
            InputController = new InputController();
            InputController.Init();
        }

        void InitCamera()
        {
            var position = new Vector3(0, 45, -10);
            CameraController.Instance.SetPosition(position);
            var rotation = Quaternion.Euler(new Vector3(70, 0, 0));
            CameraController.Instance.SetRotation(rotation);

            var runtimeCameraOrthoSize = refCameraOrthoSize.x + runtimeAspectRatio * (refCameraOrthoSize.y - refCameraOrthoSize.x);
            CameraController.Instance.SetOrthoSize(runtimeCameraOrthoSize);
        }

        void InitSlover()
        {
            slover.SetBuoyancyForce(itemMass * 9.81f * 2);
        }

        private void InitColorGroupSlotView()
        {
            colorGroupSlotViewCanvas.worldCamera = CameraController.Instance.Camera;

            var runtimeColorGroupSlotViewWidth = refColorGroupSlotViewWidth.x + runtimeAspectRatio * (refColorGroupSlotViewWidth.y - refColorGroupSlotViewWidth.x);
            colorGroupSlotView.InitSlotUIPosition(runtimeColorGroupSlotViewWidth);

            var runtimeNoneColorGroupSlotViewWidth = refNoneColorGroupSlotViewWidth.x + runtimeAspectRatio * (refNoneColorGroupSlotViewWidth.y - refNoneColorGroupSlotViewWidth.x);
            colorGroupSlotView.InitNoneColorGroupSlotUIPosition(runtimeNoneColorGroupSlotViewWidth);

            Vector3 runtimeNoneColorGroupSlotOffset = runtimeAspectRatio * refNoneColorGroupSlotOffset;
            colorGroupSlotView.InitNoneColorGroupSlotOffset(runtimeNoneColorGroupSlotOffset);
        }

        protected IEnumerator StartLevel(int _subLevelID)
        {
            LoadingPanel.Instance?.Show();

            subLevelID = _subLevelID;

            var subLevelData = LevelData.SubLevelDatas[subLevelID];

            colorGroupSloter = new ColorGroupSloter(2, 6, true);

            FreezeGameplay();

            yield return CreateLevelEntity(subLevelData);

            colorGroupSlotView.Init(colorGroupSloter);

            TotalItemCount = subLevelData.TotalItemCount;

            tookItemCount = 0;

            backupContent = new GameObject("BackupContent").transform;
            backupContent.SetParent(LevelEntity.transform);

            itemGridCount = Mathf.Min(subLevelData.FloatItemCount, subLevelData.TotalItemCount);
            var oldItems = LevelEntity.SpawnItems(itemGridCount, ItemGeneration.Old);

            var edenItemCount = Mathf.Min(subLevelData.FloatItemCount, subLevelData.TotalItemCount - itemGridCount);

            var edenItems = LevelEntity.SpawnItems(edenItemCount, ItemGeneration.Eden);

            itemGrids = new ItemGrid[itemGridCount];
            for (int i = 0; i < itemGrids.Length; i++)
            {
                itemGrids[i] = new ItemGrid(Vector2.zero);
            }

            edenItemQueue = new List<Item>();
            OldItems = new HashSet<Item>();

            for (int i = 0; i < oldItems.Length; i++)
            {
                InitItem(oldItems[i], itemGrids[i]);
            }

            for (int i = 0; i < edenItems.Length; i++)
            {
                InitItem(edenItems[i], GetRandomEmptyItemGrid());
            }

            InitIces();

            OnItemTook = null;

            var levelPanel = GameUIManager.Instance.OpenLevelPanel();

            levelPanel.OnLevelStart();

            if (true)
            {
                var itemUnlockProgressPanel = GameUIManager.Instance.OpenItemUnlockProgressPanel();
                itemUnlockProgressPanel.OnLevelStart();
            }
            else
            {
                GameUIManager.Instance.CloseItemUnlockProgressPanel();
            }

            while (IsFreezing)
            {
                TryResumeGameplay();
            }

            isFaild = false;
            isCompleted = false;

            LoadingPanel.Instance?.Hide();
        }

        ItemGrid[] CalculateGrids(int itemAmount, Bounds itemBounds, Vector3 positionArea)
        {
            ItemGrid[] grids = new ItemGrid[itemAmount];

            var widthCount = Mathf.Ceil(Mathf.Sqrt(itemAmount));
            var heightCount = widthCount;
            if (widthCount * heightCount != itemAmount)
            {
                widthCount = 0;
                heightCount = 100;
                //找到两个数x,y,x * y = itemAmount,并且x,y尽量接近
                for (int i = itemAmount / 2; i >= 1; i--)
                {
                    if (itemAmount % i == 0)
                    {
                        var x = i;
                        var y = itemAmount / i;
                        if (Mathf.Abs(x - y) < Mathf.Abs(widthCount - heightCount))
                        {
                            widthCount = Mathf.Max(x, y);
                            heightCount = Mathf.Min(x, y);
                        }
                    }
                }
            }

            float gridWidth = positionArea.x / widthCount;
            float gridHeight = positionArea.y / heightCount;

            float itemWidth = itemBounds.size.y;
            float itemHeight = itemBounds.size.x;

            float startX = -positionArea.x / 2 + gridWidth / 2;
            float startY = -positionArea.y / 2 + gridHeight / 2;

            int columns = Mathf.FloorToInt(positionArea.x / gridWidth);
            for (int i = 0; i < itemAmount; i++)
            {
                int column = i % columns;
                int row = i / columns;

                float posX = startX + column * gridWidth;
                float posY = startY + row * gridHeight;

                posX = Mathf.Clamp(posX, startX, startX + positionArea.x - itemWidth);
                posY = Mathf.Clamp(posY, startY, startY + positionArea.y - itemHeight);

                grids[i] = new ItemGrid(new Vector2(posX, posY));
            }

            return grids;
        }

        Vector2 CalculateGridPositionByIndex(int itemAmount, Bounds itemBounds, Vector3 positionArea, int gridIndex)
        {
            var widthCount = Mathf.Ceil(Mathf.Sqrt(itemAmount));
            var heightCount = widthCount;
            if (widthCount * heightCount != itemAmount)
            {
                widthCount = 0;
                heightCount = 100;
                //找到两个数x,y,x * y = itemAmount,并且x,y尽量接近
                for (int i = itemAmount / 2; i >= 1; i--)
                {
                    if (itemAmount % i == 0)
                    {
                        var x = i;
                        var y = itemAmount / i;
                        if (Mathf.Abs(x - y) < Mathf.Abs(widthCount - heightCount))
                        {
                            widthCount = Mathf.Max(x, y);
                            heightCount = Mathf.Min(x, y);
                        }
                    }
                }
            }

            float gridWidth = positionArea.x / widthCount;
            float gridHeight = positionArea.y / heightCount;

            //水平放置物体,所以item的宽度和长度要互换
            float itemWidth = itemBounds.size.y;
            float itemHeight = itemBounds.size.x;

            float startX = -positionArea.x / 2 + gridWidth / 2;
            float startY = -positionArea.y / 2 + gridHeight / 2;

            int columns = Mathf.FloorToInt(positionArea.x / gridWidth);

            var column = gridIndex % columns;
            var row = gridIndex / columns;

            float posX = startX + column * gridWidth;
            float posY = startY + row * gridHeight;

            posX = Mathf.Clamp(posX, startX, startX + positionArea.x - itemWidth);
            posY = Mathf.Clamp(posY, startY, startY + positionArea.y - itemHeight);

            return new Vector2(posX, posY);
        }

        private void InitIces()
        {
            iceContent = new GameObject("IceContent").transform;
            iceContent.SetParent(transform);
            var iceBounds = icePrototype.GetComponent<MeshFilter>().sharedMesh.bounds;
            for (int i = 0; i < iceCount; i++)
            {
                var ice = GameObject.Instantiate(icePrototype);
                ice.transform.rotation = UnityEngine.Random.rotation;
                var gridPos = CalculateGridPositionByIndex(iceCount, iceBounds, positionArea * 0.7f, i);
                ice.transform.position = new Vector3(gridPos.x, 3, gridPos.y);
                ice.transform.SetParent(iceContent);
                var iceRigidbody = ice.GetComponent<Rigidbody>();
                iceRigidbody.isKinematic = true;
                slover.AddRigidbody(iceRigidbody);
            }
        }

        private void InitItem(Item item, ItemGrid grid)
        {
            var gridIndex = Array.IndexOf(itemGrids, grid);
            grid.SetPosition(CalculateGridPositionByIndex(itemGrids.Length, item.GetComponent<Collider>().bounds, positionArea, gridIndex));

            float height = 1.5f;
            item.transform.position = new Vector3(grid.Position.x, height, grid.Position.y);
            item.transform.eulerAngles = Vector3.up * UnityEngine.Random.Range(0, 360f) + Vector3.forward * 90;

            if (item.ItemGeneration == ItemGeneration.Old)
            {
                AddItemToSlover(item);
                SetItemOld(item);
            }
            else
            {
                SetItemEden(item);
                grid.PutItem(item);
                item.DisablePhysics();
            }
        }
        private void OnEdenToOld(Item item)
        {
            edenItemQueue.Remove(item);
            var itemGird = FindItemGird(item);
            itemGird.ReleaseItem();
            SetItemOld(item);
            AddItemToSlover(item);
        }
        private void OnOldToDie(Item item)
        {
            OldItems.Remove(item);
            slover.RemoveRigidbody(item.Rigidbody);
            item.DisablePhysics();
        }

        void SetItemEden(Item item)
        {
            item.SetGeneration(ItemGeneration.Eden);
            edenItemQueue.Add(item);
        }

        void SetItemOld(Item item)
        {
            item.SetGeneration(ItemGeneration.Old);
            OldItems.Add(item);
        }

        ItemGrid FindItemGird(Item item)
        {
            for (int i = 0; i < itemGrids.Length; i++)
            {
                var grid = itemGrids[i];
                if (grid.Item == item)
                {
                    return grid;
                }
            }
            return null;
        }

        IEnumerator CreateLevelEntity(SubLevelData levelData)
        {
            var colorGroupQueue = LevelUtils.GenerateColorGroupQueue(levelData);

            colorGroupSloter.SetColorGroupQueues(colorGroupQueue);

            var handle = LevelUtils.CreateLevelEntity(levelData, colorGroupSloter);
            yield return handle.WaitForCompletion();

            LevelEntity = handle.Object;

            InputController.SetLevelEntity(LevelEntity);

            yield break;
        }

        void AddItemToSlover(Item item)
        {
            item.Rigidbody.mass = itemMass;
            item.Rigidbody.centerOfMass = new Vector3(0, item.GetComponent<MeshFilter>().sharedMesh.bounds.extents.y * 1.5f, 0);
            item.EnablePhysics();
            item.transform.name += "(Floating)";
            slover.AddRigidbody(item.Rigidbody);
        }

        public bool TryTakeItem(Item item)
        {
            var colorGroupSloter = LevelEntity.ColorGroupSloter;

            var noneSlot = colorGroupSloter.FindColorGroupSlot(ItemColor.None);
            //noneSlot没有空槽位时说明游戏即将失败
            if (!noneSlot.HasEmptySlot())
            {
                return false;
            }

            var slot = colorGroupSloter.FindUseableSlotForColor(item.ItemColor);
            if (slot == null)
            {
                return false;
            }

            StartCoroutine(IETakeItem(slot, item));

            return true;
        }

        ItemGrid GetRandomEmptyItemGrid()
        {
            int emptyGridCount = itemGrids.Count(x => x.Item == null);
            if (emptyGridCount > 0)
            {
                var index = UnityEngine.Random.Range(0, emptyGridCount);
                for (int i = 0; i < itemGrids.Length; i++)
                {
                    var grid = itemGrids[i];
                    if (grid.Item == null)
                    {
                        if (index == 0)
                        {
                            return grid;
                        }
                        index--;
                    }
                }
            }
            return null;
        }

        IEnumerator IETakeItem(ColorGroupSlot slot, Item item)
        {
            var colorGroupSloter = LevelEntity.ColorGroupSloter;

            OnOldToDie(item);

            ItemGrid edenItemGrid = null;
            if (edenItemQueue.Count > 0)
            {
                edenItemGrid = FindItemGird(edenItemQueue[0]);
                OnEdenToOld(edenItemQueue[0]);
                TryCreateNewEdenItem(edenItemGrid);
            }

            int index = slot.AllocateIndexForItem(item);

            bool isSlotFilled = !slot.HasEmptySlot();

            waterLevel.PlaySplash(item.transform.position);

            item.Take();
            slot.FillItem(item, index, out var fillAnimation);
            yield return fillAnimation;
            slot.CompleteFillItem();

            OnItemTook?.Invoke();

            if (isSlotFilled)
            {
                if (slot.FillingItemCount > 0)
                {
                    yield return new WaitUntil(() => slot.FillingItemCount <= 0);
                }

                colorGroupSloter.TryCompleteSlot();

                if (slot.ItemColor == ItemColor.None && !isFaild)
                {
                    //等待生成的槽位完成
                    yield return new WaitUntil(() => colorGroupSloter.GeneratingSlotGroupCount <= 0);

                    //槽位满了则失败
                    if (!slot.HasEmptySlot() && !isFaild)
                    {
                        isFaild = true;
                        OnGameFailed();
                    }
                }
            }

            tookItemCount++;

            if (tookItemCount == LevelData.SubLevelDatas[subLevelID].TotalItemCount && !isFaild && !isCompleted)
            {
                isCompleted = true;

                yield return new WaitUntil(() => colorGroupSloter.CompletingSlotGroupCount <= 0);

                OnGameCompleted();
            }

            yield break;
        }

        void TryCreateNewEdenItem(ItemGrid itemGrid)
        {
            if (LevelEntity.SpawnIndex > 0 && itemGrid != null)
            {
                var newEdenItem = LevelEntity.SpawnItems(1, ItemGeneration.Eden)[0];
                InitItem(newEdenItem, itemGrid);
            }
        }


        private void Update()
        {
            if (LevelEntity == null)
            {
                return;
            }

            if (!IsFreezing)
            {
                var item = InputController.TryCatchItem();
                if (item != null)
                {
                    if (item.ItemGeneration == ItemGeneration.Old)
                    {
                        TryTakeItem(item);
                    }
                }
            }
        }

        void Restore()
        {
            isFaild = false;
            //有可能在失败后填满了某一个盒子
            LevelEntity.ColorGroupSloter.TryCompleteSlot();
        }

        public void CompleteGroup()
        {
            var sloter = LevelEntity.ColorGroupSloter;
            ColorGroupSlot slot = null;
            for (int i = 0; i < sloter.ColorGroupSlots.Length; i++)
            {
                var _slot = sloter.ColorGroupSlots[i];
                if (_slot == null || _slot.ItemColor == ItemColor.None || !_slot.Useable)
                {
                    continue;
                }

                if (slot == null || _slot.EmptySlotCount > slot.EmptySlotCount)
                {
                    slot = _slot;
                }
            }

            if (slot == null)
            {
                return;
            }

            var emptySlotCount = slot.EmptySlotCount;
            while (emptySlotCount > 0)
            {
                Item item = null;
                foreach (var _item in OldItems)
                {
                    if (_item.ItemColor == slot.ItemColor)
                    {
                        item = _item;
                        break;
                    }
                }

                if (item == null)
                {
                    foreach (var _item in edenItemQueue)
                    {
                        if (_item.ItemColor == slot.ItemColor)
                        {
                            item = _item;
                            break;
                        }
                    }
                }


                if (item == null)
                {
                    item = LevelEntity.SpawnItemByItemColor(slot.ItemColor, ItemGeneration.Old);
                    var randomIndex = UnityEngine.Random.Range(0, itemGrids.Length);
                    InitItem(item, itemGrids[randomIndex]);
                }
                else if (item.ItemGeneration == ItemGeneration.Eden)
                {
                    var edenItemGrid = FindItemGird(item);
                    OnEdenToOld(item);
                    TryCreateNewEdenItem(edenItemGrid);
                }

                TryTakeItem(item);

                emptySlotCount--;
            }
        }

        public void Rearrange()
        {
            if (edenItemQueue.Count <= 0)
            {
                return;
            }

            List<Item> waitToRearrangeItems = new List<Item>();
            waitToRearrangeItems.AddRange(OldItems);
            waitToRearrangeItems.AddRange(edenItemQueue);

            int oldItemCount = OldItems.Count;
            OldItems.Clear();
            edenItemQueue.Clear();
            slover.ClearRigidbodys();
            for (int i = 0; i < itemGrids.Length; i++)
            {
                itemGrids[i].ReleaseItem();
            }

            List<ItemColor> urgentColors = FindUrgentColors();

            while (oldItemCount > 0 && urgentColors.Count > 0)
            {
                var itemColor = urgentColors[0];
                urgentColors.RemoveAt(0);
                for (int i = waitToRearrangeItems.Count - 1; i >= 0 && oldItemCount > 0; i--)
                {
                    var item = waitToRearrangeItems[i];
                    if (item.ItemColor == itemColor)
                    {
                        oldItemCount--;
                        waitToRearrangeItems.RemoveAt(i);
                        SetItemOld(item);
                        AddItemToSlover(item);
                    }
                }
            }

            while (oldItemCount > 0)
            {
                var randomIndex = UnityEngine.Random.Range(0, waitToRearrangeItems.Count);
                var item = waitToRearrangeItems[randomIndex];
                waitToRearrangeItems.RemoveAt(randomIndex);
                oldItemCount--;
                SetItemOld(item);
                AddItemToSlover(item);
            }

            for (int i = 0; i < waitToRearrangeItems.Count; i++)
            {
                SetItemEden(waitToRearrangeItems[i]);
                itemGrids[i].PutItem(waitToRearrangeItems[i]);
            }
        }

        //找到当前最紧急的颜色
        public List<ItemColor> FindUrgentColors()
        {
            var sloter = LevelEntity.ColorGroupSloter;

            var slots = sloter.ColorGroupSlots.Where(x => x != null && x.ItemColor != ItemColor.None && x.EmptySlotCount > 0).ToArray();
            Array.Sort(slots, (x, y) => y.EmptySlotCount.CompareTo(x.EmptySlotCount));

            List<ItemColor> urgentColors = new List<ItemColor>();
            for (int i = 0; i < slots.Length; i++)
            {
                urgentColors.Add(slots[i].ItemColor);
            }

            return urgentColors;
        }

        public void ClearNoneSlotGroup()
        {
            var noneGroupSlot = LevelEntity.ColorGroupSloter.FindColorGroupSlot(ItemColor.None);
            ClearGroupSlot(noneGroupSlot);
        }

        public void ClearNoneSlotGroupForRestore()
        {
            ClearNoneSlotGroup();

            Restore();

            TryResumeGameplay();
        }

        public void ClearGroupSlot(ColorGroupSlot groupSlot)
        {
            var items = groupSlot.Items.Clone() as Item[];
            for (int i = 0; i < groupSlot.Items.Length; i++)
            {
                if (groupSlot.Items[i] != null)
                {
                    groupSlot.RemoveItem(i);
                }
            }
            BackupItems(items);
        }

        //后备物品，用于后续盒子使用
        void BackupItems(IEnumerable<Item> items)
        {
            var time = 0.5f;
            foreach (Item item in items)
            {
                if (item != null)
                {
                    item.transform.SetParent(backupContent);
                    item.transform.DOScale(Vector3.zero, time);
                    LevelEntity.ColorGroupSloter.AddBackupItem(item);
                }
            }
        }


        public virtual void Restart()
        {
            DisposeLevel();
            StartCoroutine(StartLevel(subLevelID));
        }

        public void NextLevel()
        {
            DisposeLevel();
            LevelData = LevelDataManager.Instance.GetData(GameDataManager.Instance.GetLevelID());
            var subLevelID = GameDataManager.Instance.GetSubLevelID();
            StartCoroutine(StartLevel(subLevelID));
        }

        void DisposeLevel()
        {
            foreach (var item in OldItems)
            {
                slover.RemoveRigidbody(item.Rigidbody);
            }
            foreach (var item in edenItemQueue)
            {
                if (!item.Rigidbody.isKinematic)
                {
                    slover.RemoveRigidbody(item.Rigidbody);
                }
            }
            LevelEntity.Dispose();

            var rigidbodies = iceContent.GetComponentsInChildren<Rigidbody>();
            for (int i = rigidbodies.Length - 1; i >= 0; i--)
            {
                slover.RemoveRigidbody(rigidbodies[i]);
            }
            Destroy(iceContent.gameObject);

            colorGroupSloter.Dispose();
        }

        public float GetLevelProgress()
        {
            return tookItemCount / (float)TotalItemCount;
        }

        public float GetReadablelProgress()
        {
            return GetLevelProgress() * 100;
        }

        public int GetReadableLevelID()
        {
            //LevelData.ID - 1是因为LevelData.ID是从1开始的
            return (LevelData.ID - 1) * 3 + subLevelID + 1;
        }

        protected virtual void OnGameCompleted()
        {
            if (subLevelID < LevelData.SubLevelDatas.Length - 1)
            {
                GameDataManager.Instance.SetSubLevelID(subLevelID + 1);
            }
            else
            {
                var level = LevelDataManager.Instance.GetNextLevel(LevelData);
                if (level == null)
                {
                    level = LevelDataManager.Instance.GetFirstLevel();
                }
                GameDataManager.Instance.SetLevelID(level.ID);
            }

            FreezeGameplay();

            if (true)
            {
                GameUIManager.Instance.ItemUnlockProgressPanel.UnlockItem();
            }
            else
            {
                GameUIManager.Instance.OpenCompletePanel();
            }
        }

        protected virtual void OnGameFailed()
        {
            FreezeGameplay();
            GameUIManager.Instance.OpenRestorePanel();
        }

        public void BackToMainPage()
        {
            SceneManager.LoadScene("MainPage");
        }
    }
}

