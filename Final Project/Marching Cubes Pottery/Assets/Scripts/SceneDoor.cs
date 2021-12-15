using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : Grabbable
{
    public string otherSceneName;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private void OnDestroy()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoFunc(GameObject arg = null)
    {
        SceneManager.LoadSceneAsync(otherSceneName);
    }
}
