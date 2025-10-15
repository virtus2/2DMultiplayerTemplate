using System.Collections.Generic;
using UnityEngine;

public static class AnimatorHashCache
{
    public const string Parameter_Weapon_Attack = "Attack";
    public const string Parameter_Weapon_FacingRight = "FacingRight";

    private static Dictionary<string, int> parameterHash = new Dictionary<string, int>();

    public static int GetParameterHash(string key)
    {
        if (parameterHash.TryGetValue(key, out var hash))
        {
            return hash;
        }

        hash = Animator.StringToHash(key);
        parameterHash.Add(key, hash);
        return hash;
    }
}

