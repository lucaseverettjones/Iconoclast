using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    [SerializeField] GameObject particle;
    GameObject[] particles;
    [SerializeField] GameObject[] tabletPiecePrefabs;
    [SerializeField] GameObject[] floorPiecePrefabs;
    [SerializeField] GameObject[] doorPiecePrefabs;
    [SerializeField] GameObject[] wallPiecePrefabs;
    GameObject[] tabletPieces;
    GameObject[] floorPieces;
    GameObject[] wallPieces;
    GameObject[] doorPieces;
    void Start()
    {
        particles = new GameObject[100];
        for(int i = 0; i < particles.Length; i++)
        {
            particles[i] = Instantiate(particle);
            particles[i].SetActive(false);
        }
        tabletPieces = new GameObject[tabletPiecePrefabs.Length];
        for (int i = 0; i < tabletPieces.Length; i++)
        {
            tabletPieces[i] = Instantiate(tabletPiecePrefabs[i]);
            tabletPieces[i].SetActive(false);
        }
        floorPieces = new GameObject[floorPiecePrefabs.Length];
        for (int i = 0; i < floorPiecePrefabs.Length; i++)
        {
            floorPieces[i] = Instantiate(floorPiecePrefabs[i]);
            floorPieces[i].SetActive(false);
        }
        wallPieces = new GameObject[wallPiecePrefabs.Length];
        for (int i = 0; i < wallPiecePrefabs.Length; i++)
        {
            wallPieces[i] = Instantiate(wallPiecePrefabs[i]);
            wallPieces[i].SetActive(false);
        }
        doorPieces = new GameObject[doorPiecePrefabs.Length];
        for (int i = 0; i < doorPiecePrefabs.Length; i++)
        {
            doorPieces[i] = Instantiate(doorPiecePrefabs[i]);
            doorPieces[i].SetActive(false);
        }
    }
    public void SpawnParticle(GameObject parentObject)
    {
        for(int i = 0; i < particles.Length; i++)
        {
            if (particles[i].activeInHierarchy == false)
            {
                particles[i].transform.position = parentObject.transform.position;
                particles[i].SetActive(true);
                StartCoroutine("HideParticle", i);
                i = particles.Length;
            }
        }
    }
    IEnumerator HideParticle(int toHide)
    {
        yield return new WaitForSeconds(1);
        particles[toHide].SetActive(false);
    }
    public void Pieces(string type, GameObject parentObject)
    {
        StartCoroutine(SpawnPieces(parentObject, type));
    }
    IEnumerator SpawnPieces(GameObject parentObject, string type)
    {
        if(type.ToLower() == "tablet")
        {
            for(int i = 0; i < tabletPiecePrefabs.Length; i++)
            {
                tabletPieces[i].transform.position = parentObject.transform.position;
                tabletPieces[i].transform.rotation = parentObject.transform.rotation;
                tabletPieces[i].SetActive(true);
            }
            yield return new WaitForSeconds(5);
            for (int i = 0; i < tabletPiecePrefabs.Length; i++)
            {
                tabletPieces[i].SetActive(false);
            }
        }
        else if (type.ToLower() == "wall")
        {
            for (int i = 0; i < tabletPiecePrefabs.Length; i++)
            {
                wallPieces[i].transform.position = parentObject.transform.position;
                wallPieces[i].transform.rotation = parentObject.transform.rotation;
                wallPieces[i].SetActive(true);
            }
            yield return new WaitForSeconds(5);
            for (int i = 0; i < wallPiecePrefabs.Length; i++)
            {
                wallPieces[i].SetActive(false);
            }
        }
        else if (type.ToLower() == "door")
        {
            for (int i = 0; i < doorPieces.Length; i++)
            {
                doorPieces[i].transform.position = parentObject.transform.position;
                doorPieces[i].transform.rotation = parentObject.transform.rotation;
                doorPieces[i].SetActive(true);
            }
            yield return new WaitForSeconds(5);
            for (int i = 0; i < wallPiecePrefabs.Length; i++)
            {
                wallPieces[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < tabletPiecePrefabs.Length; i++)
            {
                floorPieces[i].transform.position = parentObject.transform.position;
                floorPieces[i].transform.rotation = parentObject.transform.rotation;
                floorPieces[i].SetActive(true);
            }
            yield return new WaitForSeconds(5);
            for (int i = 0; i < wallPiecePrefabs.Length; i++)
            {
                floorPieces[i].SetActive(false);
            }
        }
    }
}
