using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalCities
{
    class CityFlag:MonoBehaviour
    {
        // a list of all transforms controlled by this instance
        public List<GameObject> flagObjects = new List<GameObject>();

        // register to events
        public void Start()
        {
            GameEvents.onGameStateCreated.Add(GameLoad);
            GameEvents.onFlagSelect.Add(SelectFlag);
        }

        // unregister from the events
        public void OnDisable()
        {
            GameEvents.onGameStateCreated.Remove(GameLoad);
            GameEvents.onFlagSelect.Remove(SelectFlag);
        }

        // set the flag on game load
        public void GameLoad(Game game)
        {
            SelectFlag(game.flagURL);
        }

        // set the flag when changed from the flagpole
        public void SelectFlag(string flagURL)
        {
            if (!GameDatabase.Instance.ExistsTexture(flagURL))
                flagURL = "Squad/Flags/Default";
            foreach (GameObject obj in flagObjects)
                CityUtils.ChangeTexture(flagURL, obj);
        }
    }
}
