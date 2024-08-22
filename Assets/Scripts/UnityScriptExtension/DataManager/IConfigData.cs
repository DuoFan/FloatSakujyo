using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension 
{
    public interface IConfigData : IIDProvider
    {
        [JsonIgnore]
        string DataName { get; }
    }
}


