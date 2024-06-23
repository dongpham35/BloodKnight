using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseObject : MonoBehaviour
{
    public float Blood;
    public float Damage;
    public float Speed;
    public float Roll_Speed;
    public float Amor;

    public float currenHealth;

    public Slider healthBar;

    public bool isHurt;

    private void Start()
    {
        currenHealth = Blood;
        healthBar.maxValue = Blood;
        healthBar.value = currenHealth;
    }

    public void OnBeAttacked(float dame)
    {

        if (!isHurt)
        {
            float damageBeTacken = dame / (1 + Amor / 100);
            Debug.Log("St: " + damageBeTacken);
            currenHealth = Mathf.Clamp(currenHealth - damageBeTacken, 0, Blood);
            healthBar.value = currenHealth;
            isHurt = true;
        }
    }
}
