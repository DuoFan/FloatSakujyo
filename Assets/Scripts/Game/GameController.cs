using FloatSakujyo.Level;
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

        ItemGrid[] itemGrids;

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

            var levelData = LevelDataManager.Instance.GetData(0);
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

            var edenItems = LevelEntity.SpawnItems(levelData.FloatItemCount, ItemGeneration.Eden);
            var oldItems = LevelEntity.SpawnItems(levelData.FloatItemCount, ItemGeneration.Old);
            var itemBounds = oldItems[0].GetComponent<Collider>().bounds;

            itemGrids = CalculateGrids(levelData.FloatItemCount, itemBounds, positionArea);

            for (int i = 0; i < oldItems.Length; i++)
            {
                InitItem(oldItems[i], itemGrids[i]);
            }

            for (int i = 0; i < edenItems.Length; i++)
            {
                InitItem(edenItems[i], itemGrids[i]);
            }

            while (IsFreezing)
            {
                TryResumeGameplay();
            }

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
            PutItemToGrid(item, grid);
            if(item.ItemGeneration == ItemGeneration.Old)
            {
                AddItemToSlover(item);
            }
            else
            {
                item.DisablePhysics();
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

        void PutItemToGrid(Item item,ItemGrid grid)
        {
            float height = 1;
            switch (item.ItemGeneration)
            {
                case ItemGeneration.Eden:
                    height = 0.6f;
                    break;
                case ItemGeneration.Old:
                    height = 0.6f;
                    break;
            }

            grid.PutItem(item);
            item.transform.position = new Vector3(grid.Position.x, height, grid.Position.y);
            item.transform.eulerAngles = Vector3.up * UnityEngine.Random.Range(0, 360f) + Vector3.forward * 90;
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

        ItemGrid GetRandomPuttedItemGrid()
        {
            int puttedGridCount = itemGrids.Count(x => x.Item != null);
            if (puttedGridCount > 0)
            {
                var index = UnityEngine.Random.Range(0, puttedGridCount);
                for (int i = 0; i < itemGrids.Length; i++)
                {
                    var grid = itemGrids[i];
                    if(grid.Item != null)
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

            var randomItemGrid = GetRandomPuttedItemGrid();
            if (randomItemGrid != null)
            {
                OnEdenToOld(randomItemGrid.Item);
            }

            int index = slot.AllocateIndexForItem(item);

            bool isSlotFilled = !slot.HasEmptySlot();

            waterLevel.PlaySplash(item.transform.position);

            slot.FillItem(item, index, out var fillAnimation);
            yield return fillAnimation;
            slot.CompleteFillItem();


            LevelEntity.itemCount--;
            if (LevelEntity.itemCount >= LevelEntity.LevelData.FloatItemCount * 2 && randomItemGrid != null)
            {
                var newEdenItem = LevelEntity.SpawnItems(1, ItemGeneration.Eden)[0];
                InitItem(newEdenItem, randomItemGrid);
            }

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

        protected virtual void OnGameCompleted()
        {

        }

        protected virtual void OnGameFailed()
        {

        }
    }
}

