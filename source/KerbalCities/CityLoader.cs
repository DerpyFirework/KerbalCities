using System;
using System.Collections.Generic;
using UnityEngine;
namespace KerbalCities
{
    // This KSPAddon does pretty much all the work for this plugin
    // It runs once on the main menu, and then all work is handled by preexisting KSP code and a custom flag handler
    // As a result, performance impact should be as low as I can reasonably get it
    // See config_example.cfg for an example of how CITY nodes should be structured
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class CityLoader:MonoBehaviour
    {
        void Awake()
        {
            foreach (ConfigNode city in GameDatabase.Instance.GetConfigNodes("CITY"))
            {
                // check for a name, we need one
                if (!city.HasValue("name"))
                    continue;

                // create the new city
                GameObject cityObject = new GameObject();
                cityObject.name = city.GetValue("name");
                PQSCity2 pqsCity = cityObject.AddComponent<PQSCity2>();
                pqsCity.objectName = cityObject.name;

                // read and set values, using defaults when none are defined
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
                if (city.HasValue("snapToSurface"))
                    pqsCity.snapToSurface = city.GetValue("snapToSurface").Equals("true", StringComparison.OrdinalIgnoreCase);
                if (city.HasValue("displayName"))
                    pqsCity.displayobjectName = city.GetValue("displayName");
                else
                    pqsCity.displayobjectName = cityObject.name;

                // all the subnodes
                ConfigNode[] lods = city.GetNodes("LOD");
                String[] flagTransforms = city.GetValues("flagTransform");
                ConfigNode[] launchSites = city.GetNodes("LAUNCHSITE");

                // load the LODs and models
                List<PQSCity2.LodObject> lodObjects = new List<PQSCity2.LodObject>();
                foreach (ConfigNode lod in lods)
                {
                    // create a new LodObject
                    PQSCity2.LodObject lodObject = new PQSCity2.LodObject();

                    // define the distance to stop rendering
                    if (lod.HasValue("visibleRange"))
                        lodObject.visibleRange = float.Parse(lod.GetValue("visibleRange"));
                    else
                        lodObject.visibleRange = 25000;

                    // read the models attached to this LOD
                    ConfigNode[] models = lod.GetNodes("MODEL");
                    List<GameObject> objects = new List<GameObject>();
                    foreach (ConfigNode model in models)
                    {
                        // make sure the model exists
                        if (!model.HasValue("model") | !GameDatabase.Instance.ExistsModel(model.GetValue("model")))
                            continue;

                        // get an instance of the model
                        GameObject modelInstance = GameDatabase.Instance.GetModel(model.GetValue("model"));

                        // attach it to the city
                        modelInstance.transform.parent = cityObject.transform;

                        // set the transform values if defined
                        if (model.HasValue("position"))
                            modelInstance.transform.localPosition = ConfigNode.ParseVector3(model.GetValue("position"));
                        if (model.HasValue("rotation"))
                            modelInstance.transform.localEulerAngles = ConfigNode.ParseVector3(model.GetValue("rotation"));
                        if (model.HasValue("scale"))
                            modelInstance.transform.localScale = ConfigNode.ParseVector3(model.GetValue("scale"));

                        // move to the local scenery layer
                        modelInstance.SetLayerRecursive(15);

                        // add the model to the list
                        //objects.Add(modelInstance);
                        modelInstance.SetActive(true);

                        // force all colliders to be concave
                        MeshCollider[] mColliders = modelInstance.GetComponentsInChildren<MeshCollider>(true);
                        foreach (MeshCollider collider in mColliders)
                            collider.convex = false;                  
                    }

                    // add the object list to the lodObject
                    lodObject.objects = objects.ToArray();

                    // add the lodObject to the lodObject list
                    lodObjects.Add(lodObject);
                }

                // add the LODs to the PQSCity
                pqsCity.objects = lodObjects.ToArray();

                // set the flag transforms
                if (flagTransforms.Length != 0)
                {
                    // add the flag handler component
                    CityFlag cityFlag = cityObject.AddComponent<CityFlag>();

                    // add each transform to the handler
                    foreach (string flagTransform in flagTransforms)
                        cityFlag.flagObjects.Add(CityUtils.FindChild(flagTransform, cityObject));
                }

                // finalize the PQSCity
                cityObject.transform.parent = body.pqsController.transform;
                pqsCity.Orientate();

                // add a CommNet antenna if defined
                if (city.HasNode("COMMNET"))
                {
                    // reference the confignode for future reference
                    ConfigNode commNet = city.GetNode("COMMNET");

                    // create the CommNetHome
                    CommNet.CommNetHome cNetHome = cityObject.AddComponent<CommNet.CommNetHome>();

                    // set the internal name
                    cNetHome.nodeName = body.name + ": " + cityObject.name;

                    // get the values from the config
                    // body.GetDisplayName() is returning the value with ^N appended for some reason
                    if (commNet.HasValue("nodeName"))
                        cNetHome.displaynodeName = body.GetDisplayName() + ": " + commNet.GetValue("nodeName");
                    else
                        cNetHome.displaynodeName = body.GetDisplayName() + ": " + pqsCity.displayobjectName;
                    if (commNet.HasValue("antennaPower"))
                        cNetHome.antennaPower = double.Parse(commNet.GetValue("antennaPower"));
                    if (commNet.HasValue("isKSC"))
                        cNetHome.isKSC = commNet.GetValue("isKSC").Equals("true",StringComparison.OrdinalIgnoreCase);
                    if (commNet.HasValue("isPermanent"))
                        cNetHome.isPermanent = commNet.GetValue("isPermanent").Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (city.HasValue("commNetTransform"))
                        cNetHome.nodeTransform = CityUtils.FindChild(city.GetValue("commNetTransform"), cityObject).transform;
                    else
                        cNetHome.nodeTransform = cityObject.transform;
                }

                // setup the launch sites (Making History required)
                foreach (ConfigNode site in launchSites)
                {
                    // we need a name
                    if (!site.HasValue("name"))
                        continue;

                    // create the spawnpoint
                    LaunchSite.SpawnPoint spawnPoint = new LaunchSite.SpawnPoint();
                    spawnPoint.name = site.GetValue("name");

                    // select the facility
                    EditorFacility facility = EditorFacility.VAB;
                    if (site.HasValue("facility") && site.GetValue("facility") == "SPH")
                        facility = EditorFacility.SPH;

                    // find and reparent the spawnTransform
                    // reparenting is done to ensure that it works
                    Transform spawnTransform = CityUtils.FindChild(site.GetValue("transform"), cityObject).transform;
                    spawnTransform.SetParent(cityObject.transform);

                    // create the launchsite
                    LaunchSite launchSite = new LaunchSite(site.GetValue("name"), body.pqsController.name, site.GetValue("title"), new LaunchSite.SpawnPoint[] { spawnPoint }, cityObject.name + "/" + spawnTransform.name, facility);

                    // change the mapIcon
                    // todo: remove mapIcon
                    if (facility == EditorFacility.VAB)
                        launchSite.nodeType = KSP.UI.Screens.Mapview.MapNode.SiteType.LaunchSite;
                    else
                        launchSite.nodeType = KSP.UI.Screens.Mapview.MapNode.SiteType.Runway;

                    // finalize the LaunchSite
                    launchSite.Setup(pqsCity, new PQS[] { body.pqsController });
                    PSystemSetup.Instance.AddLaunchSite(launchSite);
                }
            }
        }
    }
}
