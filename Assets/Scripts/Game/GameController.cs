using DG.Tweening;
using FloatSakujyo.Level;
using FloatSakujyo.SaveData;
using FloatSakujyo.UI;
using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class GameController : GameControllerBase
    {
        public static GameController Instance { get; private set; }
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

        Queue<Item> edenItemQueue;

        public List<Item> OldItems
        {
            get
            {
                List<Item> result = new List<Item>();
                return result;
            }
        }

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
        Transform backupContent;

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
            var levelData = LevelDataManager.Instance.GetData(levelID);
            StartCoroutine(StartLevel(levelData));
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
            /*var position = new Vector3(0, 27.6f, -10);
            CameraController.Instance.SetPosition(position);
            var rotation = Quaternion.Euler(new Vector3(60, 0, 0));
            CameraController.Instance.SetRotation(rotation);*/

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
        }

        protected IEnumerator StartLevel(LevelData levelData)
        {
            LoadingPanel.Instance?.Show();

            FreezeGameplay();

            yield return CreateLevelEntity(levelData);

            InitIces();

            /*var levelPanel = GameUIManager.Instance.OpenLevelPanel();

            levelPanel.SetLevelID(levelData.ID);

            levelPanel.ResetHelperItems();*/

            itemGridCount = Mathf.Min(levelData.FloatItemCount, LevelEntity.TotalItemCount);
            var oldItems = LevelEntity.SpawnItems(itemGridCount, ItemGeneration.Old);

            var edenItemCount = Mathf.Min(levelData.FloatItemCount, LevelEntity.TotalItemCount - itemGridCount);

            var edenItems = LevelEntity.SpawnItems(edenItemCount, ItemGeneration.Eden);

            var itemBounds = oldItems[0].GetComponent<Collider>().bounds;

            itemGrids = CalculateGrids(itemGridCount, itemBounds, positionArea);

            edenItemQueue = new Queue<Item>();

            for (int i = 0; i < oldItems.Length; i++)
            {
                InitItem(oldItems[i], itemGrids[i]);
            }

            for (int i = 0; i < edenItems.Length; i++)
            {
                InitItem(edenItems[i], GetRandomEmptyItemGrid());
            }

            while (IsFreezing)
            {
                TryResumeGameplay();
            }

            isFaild = false;
            isCompleted = false;

            LoadingPanel.Instance?.Hide();
        }

        ItemGrid[] CalculateGrids(int itemAmount, Bounds itemBounds,Vector3 positionArea)
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


        private void InitIces()
        {
            var iceBounds = icePrototype.GetComponent<MeshFilter>().sharedMesh.bounds;
            var iceGrids = CalculateGrids(iceCount, iceBounds, positionArea * 0.7f);
            for (int i = 0; i < iceCount; i++)
            {
                var ice = GameObject.Instantiate(icePrototype);
                ice.transform.rotation = UnityEngine.Random.rotation;
                var gridPos = iceGrids[i].Position;
                ice.transform.position = new Vector3(gridPos.x, 3, gridPos.y);
                var iceRigidbody = ice.GetComponent<Rigidbody>();
                iceRigidbody.isKinematic = true;
                slover.AddRigidbody(iceRigidbody);
            }
        }

        private void InitItem(Item item,ItemGrid grid)
        {
            float height = 1.5f;
            item.transform.position = new Vector3(grid.Position.x, height, grid.Position.y);
            item.transform.eulerAngles = Vector3.up * UnityEngine.Random.Range(0, 360f) + Vector3.forward * 90;

            if (item.ItemGeneration == ItemGeneration.Old)
            {
                AddItemToSlover(item);
            }
            else
            {
                grid.PutItem(item);
                item.DisablePhysics();
                edenItemQueue.Enqueue(item);
            }
        }
        private void OnEdenToOld(Item item)
        {
            var itemGird = FindItemGird(item);
            itemGird.ReleaseItem();
            item.SetGeneration(ItemGeneration.Old);
            itemGird.PutItem(item);
            AddItemToSlover(item);
        }
        private void OnOldToDie(Item item)
        {
            slover.RemoveRigidbody(item.Rigidbody);
            item.DisablePhysics();
        }

        ItemGrid FindItemGird(Item item)
        {
            for (int i = 0; i < itemGrids.Length; i++)
            {
                var grid = itemGrids[i];
                if(grid.Item == item)
                {
                    return grid;
                }
            }
            return null;
        }

        IEnumerator CreateLevelEntity(LevelData levelData)
        {
            var handle = LevelUtils.CreateLevelEntity(levelData);
            yield return handle.WaitForCompletion();

            LevelEntity = handle.Object;
            var sloter = LevelEntity.ColorGroupSloter;

            colorGroupSlotView.Init(sloter);

            InputController.SetLevelEntity(LevelEntity);

            yield break;
        }

        void AddItemToSlover(Item item)
        {
            item.Rigidbody.mass = itemMass;
            item.Rigidbody.centerOfMass = new Vector3(item.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x, 0, 0);
            item.EnablePhysics();
            item.AddComponent<Ice>();
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
                    if(grid.Item == null)
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
            if(edenItemQueue.Count > 0)
            {
                var edenItem = edenItemQueue.Dequeue();
                edenItemGrid = FindItemGird(edenItem);
                OnEdenToOld(edenItem);
            }

            if (LevelEntity.itemCount > itemGridCount * 2 && edenItemGrid != null)
            {
                var newEdenItem = LevelEntity.SpawnItems(1, ItemGeneration.Eden)[0];
                InitItem(newEdenItem, edenItemGrid);
            }

            int index = slot.AllocateIndexForItem(item);

            bool isSlotFilled = !slot.HasEmptySlot();

            waterLevel.PlaySplash(item.transform.position);

            slot.FillItem(item, index, out var fillAnimation);
            yield return fillAnimation;
            slot.CompleteFillItem();

            LevelEntity.itemCount--;

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

            if (LevelEntity.itemCount == 0 && !isFaild && !isCompleted)
            {
                yield return new WaitUntil(() => colorGroupSloter.GeneratingSlotGroupCount <= 0);

                isCompleted = true;
                OnGameCompleted();
            }

            yield break;
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

        }

        public void Rearrange()
        {

        }

        public void ClearNoneSlotGroupForRestore()
        {
            var noneGroupSlot = LevelEntity.ColorGroupSloter.FindColorGroupSlot(ItemColor.None);
            ClearGroupSlot(noneGroupSlot);

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
            var levelData = LevelEntity.LevelData;
            DisposeLevel();
            StartCoroutine(StartLevel(levelData));
        }

        public void NextLevel()
        {
            DisposeLevel();
            var levelData = LevelDataManager.Instance.GetData(GameDataManager.Instance.GetLevelID());
            StartCoroutine(StartLevel(levelData));
        }

        void DisposeLevel()
        {
            LevelEntity.Dispose();
            slover.ClearRigidbodys();
        }

        public float GetLevelProgress()
        {
            return LevelEntity.GetProgress();
        }

        public float GetReadablelProgress()
        {
            return GetLevelProgress() * 100;
        }

        protected virtual void OnGameCompleted()
        {
            var level = LevelDataManager.Instance.GetNextLevel(LevelEntity.LevelData);
            if (level == null)
            {
                level = LevelDataManager.Instance.GetFirstLevel();
            }
            GameDataManager.Instance.SetLevelID(level.ID);

            FreezeGameplay();

            /*if (cityConfigData != null)
            {
                GameDataManager.Instance.UnlockBuilding(cityConfigData.ID, buildingIndex);
                GameDataManager.Instance.SetDisplayCityID(cityConfigData.ID);
                GameUIManager.Instance.BuildProgressPanel.CompleteBuild();
            }
            else
            {
                GameUIManager.Instance.OpenCompletePanel();
            }*/

            GameUIManager.Instance.OpenCompletePanel();
        }

        protected virtual void OnGameFailed()
        {
            FreezeGameplay();
            GameUIManager.Instance.OpenRestorePanel();
        }
    }
}

