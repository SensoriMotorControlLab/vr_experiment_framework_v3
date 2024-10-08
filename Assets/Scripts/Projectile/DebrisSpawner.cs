using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    public List<GameObject> debrisPrefabs = new List<GameObject>(); // List of debris prefabs
    public Transform spawnPoint; // The spawn point
    public float spawnRate = 1.0f; // The rate at which debris is spawned
    public int debrisCount = 0; // The number of debris to spawn
    public float speed = 1.0f; // Speed of the debris
    public float spawnAreaWidth = 10f; // Width of the spawn area
    public float spawnAreaHeight = 5f; // Height of the spawn area
    private List<GameObject> debrisList = new List<GameObject>(); // List of spawned debris

    private GameObject despawner;
    void Start()
    {
        despawner = GameObject.FindWithTag("Despawner");
        InvokeRepeating("SpawnDebris", 0.0f, spawnRate);
    }

    void SpawnDebris()
    {

        for (int i = 0; i < debrisCount; i++)
        {
            GameObject selectedPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Count)];

            // Calculate random spawn position within the defined rectangle area
            Vector3 spawnPosition = spawnPoint.position + new Vector3(
                Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2),
                0, // Assuming no vertical offset; adjust if needed
                Random.Range(-spawnAreaHeight / 2, spawnAreaHeight / 2)
            );
            
            //get the child of the selected prefab and rotate it to a random angle at y but keep x and z at the same angle
            GameObject model = selectedPrefab.transform.GetChild(0).gameObject;
            model.transform.rotation = Quaternion.Euler(model.transform.eulerAngles.x, Random.Range(0, 360), model.transform.eulerAngles.z);

            GameObject debris = Instantiate(selectedPrefab, spawnPosition, spawnPoint.rotation);
            debrisList.Add(debris);

            DebrisMovement debrisMovement = debris.GetComponent<DebrisMovement>();
            if (debrisMovement != null)
            {
                debrisMovement.SetDirection(despawner.transform.localPosition.normalized);
                debrisMovement.SetSpeed(speed);
            }
        }
    }

    public void DestroyDebris()
    {
        foreach (GameObject debris in debrisList)
        {
            Destroy(debris);
        }
        debrisList.Clear();
    }
}

