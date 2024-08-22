using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public class CodeFragmentManager : MultiManager<CodeFragment, string>
    {
        protected override CodeFragment AddNewManager()
        {
            return new CodeFragment();
        }

        protected override CodeFragment AddNewManager(string obj)
        {
            var codeFragment = new CodeFragment();
            codeFragment.Set(obj);
            return codeFragment;
        }

        protected override void BeforeAdd()
        {

        }
    }
}
