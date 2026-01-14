using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {
[SerializeField] float turnSpeed = 90f;
    private void OnTriggerEnter(Collider other)
    {

    if (other.gameObject.GetComponent<Obstacle>() != null) {
            Destroy(gameObject);
            return;
        }
        // Check if the player collided with the coin
        if (other.gameObject.name != "Player") {
            return;
        }

        // Add to the player's score
        GameManager.instance.IncrementScore();
        // Destroy this coin object
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, turnSpeed * Time.deltaTime);
    }
}
