using System.Collections.Generic;
using UnityEngine;

public class Placement : MonoBehaviour
{
    public float faceAngle = 90f;
    public GameObject[] unitPrefabs;
    public AudioSource placeSound;
    public AudioSource removeSound;
    public float allyMaxX = 0f;

    int selected = 0;
    Dictionary<GameObject, GameObject> units = new Dictionary<GameObject, GameObject>();

    void Start()
    {
        Unit.battle = false;

        foreach (GameObject block in GameObject.FindGameObjectsWithTag("Ground"))
        {
            if (!IsAllyZone(block))
                block.GetComponentInChildren<Renderer>().material.color = new Color(1f, 0.4f, 0.4f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) selected = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selected = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selected = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) selected = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) selected = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) selected = 5;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject block = hit.collider.gameObject;

                if (units.ContainsKey(block))
                {
                    Destroy(units[block]);
                    units.Remove(block);
                    removeSound.Play();
                }
                else if (block.CompareTag("Ground") && IsAllyZone(block))
                {
                    Vector3 pos = block.transform.position;
                    pos.y = hit.collider.bounds.max.y;
                    units.Add(block, Instantiate(unitPrefabs[selected], pos, Quaternion.Euler(0, faceAngle, 0)));
                    placeSound.Play();
                }
            }
        }
    }

    bool IsAllyZone(GameObject block)
    {
        return block.transform.position.x < allyMaxX;
    }

    public void StartBattle()
    {
        Unit.battle = true;
        enabled = false;
    }
}