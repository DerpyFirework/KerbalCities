using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalCities
{
    // Adds Baiberbanur as a launchsite
    // in future, this will be an option in the difficulty settings
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class KSC2:MonoBehaviour
    {
        void Awake()
        {
            // get Kerbins PQS
            PQS bodyPQS = FlightGlobals.GetHomeBody().pqsController;

            // search for the KSC2 object and check it exists
            Transform KSC2 = bodyPQS.transform.Find("KSC2");
            if (KSC2 == null)
                return;

            // try setting the launch platform convex
            // this is needed as the concave collider causes vessels to bug on physics load
            // an alternative solution would be ideal
            Transform platform = KSC2.Find("launchpad/Launchpad Platform");
            if (platform != null)
                platform.GetComponent<MeshCollider>().convex = true;

            // create the spawnpoint
            LaunchSite.SpawnPoint spawnPoint = new LaunchSite.SpawnPoint();
            spawnPoint.name = "Baikerbanur";

            // create the launchsite
            LaunchSite launchSite = new LaunchSite("Baikerbanur", bodyPQS.name, KSP.Localization.Localizer.GetStringByTag("#autoLOC_6002142"), new LaunchSite.SpawnPoint[] { spawnPoint }, "KSC2/launchpad/PlatformPlane", EditorFacility.VAB);
            launchSite.nodeType = KSP.UI.Screens.Mapview.MapNode.SiteType.LaunchSite;

            // finalize the LaunchSite
            launchSite.Setup(KSC2.gameObject.GetComponent<PQSCity>(), new PQS[] { bodyPQS });
            PSystemSetup.Instance.AddLaunchSite(launchSite);
        }
    }
}
