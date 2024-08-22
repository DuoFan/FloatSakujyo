using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IGameController
    {
        bool IsFreezing { get; }
        void FreezeGameplay();
        bool TryResumeGameplay();
    }
    public abstract class GameControllerBase : MonoBehaviour,IGameController
    {
        int freezeGameplayePair;
        public bool IsFreezing => freezeGameplayePair > 0;

        public virtual void FreezeGameplay()
        {
            int _freezeGameplayePair = freezeGameplayePair;
            freezeGameplayePair++;
            if (_freezeGameplayePair == 0)
            {
                Internal_FreezeGameplay();
            }
        }

        protected abstract void Internal_FreezeGameplay();

        public bool TryResumeGameplay()
        {
            if (freezeGameplayePair == 0)
            {
                string error = $"调用错误,TryResumeGameplay之前必须调用FreezeGameplay";
                GameExtension.Logger.Error(error);
                return true;
            }

            freezeGameplayePair--;
            if (freezeGameplayePair == 0)
            {
                Internal_ResumeGameplay();
            }
            return freezeGameplayePair == 0;
        }

        protected abstract void Internal_ResumeGameplay();
    }
}
