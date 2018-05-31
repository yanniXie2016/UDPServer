using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDPChat
{
    public class Spawner : MonoBehaviour
    {
        public Vector3 SpawnLocation;
        public GameObject[] prefab;
        private GameObject[] clone;

        void Start()
        {
            clone = new GameObject[prefab.Length];
        }

        public GameObject SpawnVechile(int type)
        {
            GameObject g = null;
            switch (type)
            {
                case 0:
                    clone[0] = Instantiate(prefab[0], SpawnLocation, Quaternion.Euler(0, 0, 0)) as GameObject;
                    g = clone[0];
                    break;
                case 1:
                    clone[1] = Instantiate(prefab[1], SpawnLocation, Quaternion.Euler(0, 0, 0)) as GameObject;
                    g = clone[1];
                    break;
                case 2:
                    clone[2] = Instantiate(prefab[2], SpawnLocation, Quaternion.Euler(0, 0, 0)) as GameObject;
                    g = clone[2];
                    break;
            }
            return g;
        }
    }
}
