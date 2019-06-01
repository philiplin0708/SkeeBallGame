using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public int sensorValue = 0;
    public Game gm;

    void OnTriggerEnter(Collider other)
    {   
        if (gm.gameStatus == Game.GameStatus.Play)
        {
            //if ball collide, ball disappear 
            Destroy(other.gameObject);

            //and base on the drag/weight value, we give 
            //a random value to add into score, however, 
            //if the ball is heavier, then player will 
            //need to have more force to throw the ball, 
            //if the heavier ball goes into the hall, 
            //player will get higher bonus mark

            if (other.gameObject.GetComponent<Rigidbody>().drag > 0.2f && other.gameObject.GetComponent<Rigidbody>().drag <= 0.5f)
            {
                sensorValue += Random.Range(3, 5);
            }
            else if (other.gameObject.GetComponent<Rigidbody>().drag > 0.6f && other.gameObject.GetComponent<Rigidbody>().drag <= 1.0f)
            {
                sensorValue += Random.Range(6, 8);
            }
            else
            {
                sensorValue += Random.Range(1, 2);
            }
            
            gm.UpdateScore(sensorValue);
        }
    }
}
