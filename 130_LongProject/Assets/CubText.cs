using UnityEngine;

public class CubText : MonoBehaviour
{
    [SerializeField] private float health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Current health: " + health);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Player")
        {
            health--;
            Debug.Log("Current health: " + health);
        }
    }
}
