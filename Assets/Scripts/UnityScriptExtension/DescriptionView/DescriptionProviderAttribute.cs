using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameExtension;

namespace GameExtension
{
    public class DescriptionProviderAttribute : Attribute
    {
        public string description;
        public DescriptionProviderAttribute(string _description)
        {
            description = _description;
        }
    }
}

