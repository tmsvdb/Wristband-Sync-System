using UnityEngine;
using System.Collections;

public abstract class SingletonComponent<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    /**
       Returns the instance of this singleton.
    */
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    Debug.LogError("An instance of " + typeof(T) +
                       " is needed in the scene, but there is none.");
                }

                
            }

            return instance;
        }
    }

    // On awake we want to check if an instance of the datamanager allready exists
    // if this instance exists keep using it
    // if there is no instance yet make a new one
    //
    // THE GAME OBJECT LINKED TO THIS CLASS WILL NOT BE DESTROYED 
    // THIS MEANS LOADING A NEW SCENE PASSES THE DATAMANGER OBJECT
    protected void Awake()
    {
        DontOverrideOnLoad();
    }

    protected static bool isIndestructable = false;

    protected void DontOverrideOnLoad()
    {
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }

        instance = this as T;

        if (transform.parent == null && !isIndestructable)
        {
            isIndestructable = true;
            DontDestroyOnLoad(this.gameObject);
        } 
    }
}
