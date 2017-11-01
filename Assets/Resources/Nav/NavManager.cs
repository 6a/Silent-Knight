using RAIN.Navigation.NavMesh;
using System.Collections;
using System.Threading;
using UnityEngine;

public class NavManager : MonoBehaviour
{
    public float Progress { get; private set; }

    void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void UpdateNavMesh()
    {
        NavMeshRig tRig = GetComponent<NavMeshRig>();

        // Unregister any navigation mesh we may already have (probably none if you are using this)
        tRig.NavMesh.UnregisterNavigationGraph();

        tRig.NavMesh.StartCreatingContours(0);
        while (tRig.NavMesh.Creating)
        {
            tRig.NavMesh.CreateContours();

            Thread.Sleep(10);
        }

        tRig.NavMesh.RegisterNavigationGraph();
    }

}
