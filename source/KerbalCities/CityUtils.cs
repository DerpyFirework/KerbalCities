using System;
using UnityEngine;

namespace KerbalCities
{
    class CityUtils
    {
        // find a named game object in the hierarchy of another
        public static GameObject FindChild(string name, GameObject parent)
        {
            // cycle through all transforms on all children
            Transform[] ts = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in ts)
            {
                // skip if it's not the one
                if (transform.name != name)
                    continue;

                // return the transform
                return transform.gameObject;
            }
            return null;
        }

        // change the texture on an object
        public static void ChangeTexture(string textureURL, GameObject obj)
        {
            // make sure the texture and object exist
            if(!GameDatabase.Instance.ExistsTexture(textureURL) | obj == null)
                return;

            // get the texture and the renderer
            Texture2D texture = GameDatabase.Instance.GetTexture(textureURL, false);
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

            // make sure the renderer exists and change the texture
            if (renderer != null)
                renderer.material.mainTexture = texture;
        }
    }
}
