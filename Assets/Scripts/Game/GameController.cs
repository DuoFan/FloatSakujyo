using FloatSakujyo.Level;
using FloatSakujyo.UI;
using GameExtension;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

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
        Slover midSlover;
        [SerializeField]
        Slover oldSlover;

        [SerializeField]
        Transform iceParent;

        private void Awake()
        {
            Instance = this;

            InitInputController();

            InitCamera();

            var levelData = LevelDataManager.Instance.GetData(0);
            StartCoroutine(StartLevel(levelData));
        }

        protected override void Internal_FreezeGameplay()
        {

        }

        protected override void Internal_ResumeGameplay()
        {

        }

        void InitInputController()
        {
            InputController = new InputController();
            InputController.Init();
        }

        void InitCamera()
        {
            var position = new Vector3(0, 31, -10);
            CameraController.Instance.SetPosition(position);
            var rotation = Quaternion.Euler(new Vector3(60, 0, 0));
            CameraController.Instance.SetRotation(rotation);
        }

        protected IEnumerator StartLevel(LevelData levelData)
        {
            LoadingPanel.Instance?.Show();

            FreezeGameplay();

            yield return CreateLevelEntity(levelData);

            InitIces();

            LevelEntity.OnGameCompleted += OnGameCompleted;
            LevelEntity.OnGameFailed += OnGameFailed;

            LevelEntity.OnNewEden += OnNewEden;
            LevelEntity.OnEdenToMid += OnEdenToMid;
            LevelEntity.OnMidToOld += OnMidToOld;
            LevelEntity.OnOldToDie += OnOldToDie;

            /*var levelPanel = GameUIManager.Instance.OpenLevelPanel();

            levelPanel.SetLevelID(levelData.ID);

            levelPanel.ResetHelperItems();*/

            void PutItems(Item[] items)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    InitItemTransform(items[i]);
                    PutItem(items[i]);
                }
            }

            PutItems(LevelEntity.EdenItems.ToArray());
            PutItems(LevelEntity.MidItems.ToArray());
            PutItems(LevelEntity.OldItems.ToArray());

            while (IsFreezing)
            {
                TryResumeGameplay();
            }

            LoadingPanel.Instance?.Hide();
        }

        private void InitIces()
        {
            var ices = iceParent.GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < ices.Length; i++)
            {
                var ice = ices[i];
                ice.transform.rotation = Random.rotation;
                ice.transform.position = new Vector3(Random.Range(-3, oldSlover.CalculateWaterLevel()), 3, Random.Range(-2, 2));
                oldSlover.AddRigidbody(ices[i]);
            }
        }

        private void OnNewEden(Item item)
        {
            InitItemTransform(item);
            PutItem(item);
        }
        private void OnEdenToMid(Item item)
        {
            item.SetGeneration(ItemGeneration.Mid);
            PutItem(item);
        }
        private void OnMidToOld(Item item)
        {
            midSlover.RemoveRigidbody(item.Rigidbody);
            item.SetGeneration(ItemGeneration.Old);
            PutItem(item);
        }
        private void OnOldToDie(Item item)
        {
            oldSlover.RemoveRigidbody(item.Rigidbody);
            item.DisablePhysics();
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

        void InitItemTransform(Item item)
        {
            float height = 1;
            switch (item.ItemGeneration)
            {
                case ItemGeneration.Eden:
                    height = 1;
                    break;
                case ItemGeneration.Mid:
                    height = 1.5f;
                    break;
                case ItemGeneration.Old:
                    height = 3f;
                    break;
            }
            item.transform.rotation = Random.rotation;
            item.transform.position = new Vector3(Random.Range(-3, 3), height, Random.Range(-2, 2));
        }

        void PutItem(Item item)
        {
            if (item.ItemGeneration == ItemGeneration.Eden)
            {
                item.gameObject.CheckActiveSelf(false);
            }
            else
            {
                item.gameObject.CheckActiveSelf(true);
                Slover slover = item.ItemGeneration == ItemGeneration.Mid ? midSlover : oldSlover;
                item.EnablePhysics();
                slover.AddRigidbody(item.Rigidbody);
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
                    if(item.ItemGeneration == ItemGeneration.Old)
                    {
                        LevelEntity.TryTakeItem(item);
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

