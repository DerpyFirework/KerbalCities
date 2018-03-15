using System;
using System.Collections.Generic;
using UnityEngine;
namespace KerbalCities
{   // full documentation coming when this all works
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class CityLoader:MonoBehaviour
    {
        void Awake()
        {
            foreach (ConfigNode city in GameDatabase.Instance.GetConfigNodes("CITY"))
            {
                if (!city.HasValue("name"))
                    continue;
                GameObject cityObject = new GameObject();
                cityObject.name = city.GetValue("name");
                PQSCity2 pqsCity = cityObject.AddComponent<PQSCity2>();
                if (city.HasValue("verticalOffset"))
                {
                    pqsCity.alt = double.Parse(city.GetValue("verticalOffset"));
                    pqsCity.snapHeightOffset = double.Parse(city.GetValue("verticalOffset"));
                }
                CelestialBody body = FlightGlobals.GetHomeBody();
                if (city.HasValue("body"))
                    body = FlightGlobals.GetBodyByName(city.GetValue("body"));
                pqsCity.sphere = body.pqsController;
                if (city.HasValue("lat"))
                    pqsCity.lat = double.Parse(city.GetValue("lat"));
                if (city.HasValue("lon"))
                    pqsCity.lon = double.Parse(city.GetValue("lon"));
                if (city.HasValue("rotation"))
                    pqsCity.rotation = double.Parse(city.GetValue("rotation"));
                if(city.HasValue("snapToSurface"))
                    pqsCity.snapToSurface = city.GetValue("snapToSurface").Equals("true", StringComparison.OrdinalIgnoreCase);
                ConfigNode[] lods = city.GetNodes("LOD");
                String[] flagTransforms = city.GetValues("flagTransform");
                ConfigNode[] launchSites = city.GetNodes("LAUNCHSITE");
                List<PQSCity2.LodObject> lodObjects = new List<PQSCity2.LodObject>();
                foreach (ConfigNode lod in lods)
                {
                    PQSCity2.LodObject lodObject = new PQSCity2.LodObject();
                    if (lod.HasValue("visibleRange"))
                        lodObject.visibleRange = float.Parse(lod.GetValue("visibleRange"));
                    else
                        lodObject.visibleRange = 25000;
                    ConfigNode[] models = lod.GetNodes("MODEL");
                    List<GameObject> objects = new List<GameObject>();
                    foreach (ConfigNode model in models)
                    {
                        if (!model.HasValue("model") | !GameDatabase.Instance.ExistsModel(model.GetValue("model")))
                            continue;
                        GameObject modelInstance = GameDatabase.Instance.GetModel(model.GetValue("model"));
                        modelInstance.transform.parent = cityObject.transform;
                        if (model.HasValue("position"))
                            modelInstance.transform.localPosition = ConfigNode.ParseVector3(model.GetValue("position"));
                        else
                            modelInstance.transform.localPosition = Vector3.zero;
                        if (model.HasValue("rotation"))
                            modelInstance.transform.localEulerAngles = ConfigNode.ParseVector3(model.GetValue("rotation"));
                        else
                            modelInstance.transform.localEulerAngles = Vector3.zero;
                        if (model.HasValue("scale"))
                            modelInstance.transform.localScale = ConfigNode.ParseVector3(model.GetValue("scale"));
                        else
                            modelInstance.transform.localScale = Vector3.one;
                        modelInstance.SetLayerRecursive(15);
                        objects.Add(modelInstance);
                        //foreach (MeshCollider collider in modelInstance.GetComponentInChildren<MeshCollider>(true))
                            //collider.convex = false;
                        if (body == FlightGlobals.GetHomeBody())
                            modelInstance.SetActive(true);                       
                    }
                    lodObject.objects = objects.ToArray();
                    lodObjects.Add(lodObject);
                }
                pqsCity.objects = lodObjects.ToArray();
                foreach (string flagTransform in flagTransforms)
                {
                }
                foreach (ConfigNode site in launchSites)
                {
                }
                cityObject.transform.parent = body.pqsController.transform;
                pqsCity.Orientate();
            }
        }
    }
}
