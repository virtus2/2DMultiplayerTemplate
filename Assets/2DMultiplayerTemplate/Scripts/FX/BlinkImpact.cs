using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class BlinkImpact : MonoBehaviour
    {
        [Header("Variables")]
        [ColorUsage(showAlpha: true, hdr: true)]
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private AnimationCurve amountAnimationCurve;
        [SerializeField] private string impactAmountParameterName = "_ImpactAmount";
        [SerializeField] private string impactColorParameterName = "_ImpactColor";

        private SpriteRenderer[] spriteRenderers;
        private Dictionary<SpriteRenderer, List<Material>> materials = new Dictionary<SpriteRenderer, List<Material>>();
        private bool isPlaying;
        private float elapsedTime = 0f;
        private float currentImpactAmount = 0f;

        private void Awake()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            int materialsCount = 0;
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                materialsCount += spriteRenderers[i].materials.Length;
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (materials.TryGetValue(spriteRenderers[i], out List<Material> list))
                {
                    for (int j = 0; j < spriteRenderers[i].materials.Length; j++)
                    {
                        list.Add(spriteRenderers[i].materials[j]);
                    }
                }
                else
                {
                    list = new List<Material>();

                    for (int j = 0; j < spriteRenderers[i].materials.Length; j++)
                    {
                        list.Add(spriteRenderers[i].materials[j]);
                    }

                    materials.Add(spriteRenderers[i], list);
                }
            }
        }
        private void Update()
        {
            if (isPlaying)
            {
                if (elapsedTime <= duration)
                {
                    float t = amountAnimationCurve.Evaluate(elapsedTime / duration);
                    currentImpactAmount = Mathf.Lerp(1f, 0f, t);
                    foreach (var materialList in materials.Values)
                    {
                        foreach (var material in materialList)
                        {
                            material.SetFloat(impactAmountParameterName, currentImpactAmount);
                        }
                    }
                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    elapsedTime = 0f;
                    isPlaying = false;
                    currentImpactAmount = 0f;
                }
            }
        }

        public void Play()
        {
            Play(true);
        }

        public void Play(bool play)
        {
            if (isPlaying && play)
                return;

            isPlaying = play;

            foreach (var materialList in materials.Values)
            {
                foreach (var material in materialList)
                {
                    material.SetColor(impactColorParameterName, color);
                }
            }
        }
    }
}