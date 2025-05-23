using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefab : MonoBehaviour
{
    [field: SerializeField] public Transform From;
    [field: SerializeField] public Transform To;
    [Space]
    [SerializeField] private List<ModelVariant> Models = new List<ModelVariant>();

    public void Initialize()
    {
        if (Models.Count == 0)
        {
            //Debug.LogError("No models to spawn");
            return;
        }

        float totalWeight = 0;
        foreach (ModelVariant model in Models)
        {
            totalWeight += model.Weight;
        }

        ModelVariant selectedModel = null;
        float random = UnityEngine.Random.Range(0, totalWeight);
        foreach (ModelVariant model in Models)
        {
            random -= model.Weight;
            if (random <= 0)
            {
                selectedModel = model;
                break;
            }
        }

        foreach (ModelVariant model in Models)
        {
            model.Model.SetActive(model == selectedModel);
        }
    }

    [Serializable]
    private class ModelVariant
    {
        [field: SerializeField] public GameObject Model { get; private set; }
        [field: SerializeField] public float Weight { get; private set; }
    }
}
