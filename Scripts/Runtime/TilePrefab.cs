using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class TilePrefab : MonoBehaviour
{
    [field: SerializeField] public Transform From;
    [field: SerializeField] public Transform To;
    [Space]
    [SerializeField] private List<ModelGroup> ModelGroups = new List<ModelGroup>();

    public void Initialize()
    {
        foreach (var group in ModelGroups)
        {
            InitializeGroup(group);
        }
    }

    private void InitializeGroup(ModelGroup group)
    {
        if (group.Models.Count == 0)
        {
            return;
        }

        if (UnityEngine.Random.Range(0f, 1f) > group.SpawnChance)
        {
            foreach (ModelVariant model in group.Models)
            {
                model.Model.SetActive(false);
            }
            return;
        }

        float totalWeight = 0;
        foreach (ModelVariant model in group.Models)
        {
            totalWeight += model.Weight;
        }

        ModelVariant selectedModel = null;
        float random = UnityEngine.Random.Range(0, totalWeight);
        foreach (ModelVariant model in group.Models)
        {
            random -= model.Weight;
            if (random <= 0)
            {
                selectedModel = model;
                break;
            }
        }

        foreach (ModelVariant model in group.Models)
        {
            model.Model.SetActive(model == selectedModel);
        }
    }

    private void OnValidate()
    {
        foreach (var group in ModelGroups)
        {
            foreach (ModelVariant model in group.Models)
            {
                model.Validate();
            }   
        }
    }

    [Serializable]
    private class ModelGroup
    {
        [field: SerializeField, Range(0f, 1f)] public float SpawnChance { get; private set; } = 1f;
        [field: SerializeField] public List<ModelVariant> Models { get; private set; } = new List<ModelVariant>();
    }
    
    [Serializable]
    private class ModelVariant
    {
        [HideInInspector, SerializeField] public string inspectorName;
        [field: SerializeField] public GameObject Model { get; private set; }
        [field: SerializeField] public float Weight { get; private set; }

        public void Validate()
        {
            inspectorName = Model != null ? Model.name : "None";
        }
    }
}
