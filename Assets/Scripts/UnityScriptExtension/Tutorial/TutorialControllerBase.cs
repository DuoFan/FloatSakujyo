using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public abstract class TutorialControllerBase
    {
        static ITutorialDataManager tutorialDataManager;

        public static void SetTutorialDataManager(ITutorialDataManager _tutorialDataManager)
        {
            tutorialDataManager = _tutorialDataManager;
        }

        protected const int ENDING = -100;
        public abstract string TutorialKey { get; }
        public abstract IEnumerator StartTutorial();

        public IEnumerator StartTutorialWhenNotCompleted()
        {
            if (IsCompleted)
            {
                yield break;
            }
            yield return StartTutorial();
        }

        public bool IsCompleted => tutorialDataManager.GetTutorialStep(TutorialKey) == ENDING;
    }

    public interface ITutorialDataManager
    {
        TutorialHistoryData GetTutorialHistoryData();

        TutorialStepHandle GetTutorialStepHandle(string tutorialKey)
        {
            var tutorialStepHistory = GetTutorialHistoryData().TutorialStepHistory;
            bool isNeverStart = !tutorialStepHistory.ContainsKey(tutorialKey);
            if (isNeverStart)
            {
                tutorialStepHistory[tutorialKey] = TutorialStepHandle.NEVER_START;
            }
            TutorialStepHandle tutorialStepHandle = new TutorialStepHandle((step) => SetTutorialStep(tutorialKey, step),
                () => GetTutorialStep(tutorialKey));
            return tutorialStepHandle;
        }

        private void SetTutorialStep(string tutorialKey, int step)
        {
            var tutorialStep = GetTutorialHistoryData().TutorialStepHistory;
            tutorialStep[tutorialKey] = step;
        }

        int GetTutorialStep(string tutorialKey)
        {
            if (!GetTutorialHistoryData().TutorialStepHistory.TryGetValue(tutorialKey, out int step))
            {
                step = TutorialStepHandle.NEVER_START;
            }
            return step;
        }
    }
}
