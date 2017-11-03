using UnityEngine;

namespace Entities
{
    interface ITargetable
    {
        Transform TargetTransform { get; set; }
    }
}
