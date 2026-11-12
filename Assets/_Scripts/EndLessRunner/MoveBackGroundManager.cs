using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackGroundManager : MonoBehaviour
{
    [SerializeField] private List<MoveBackground> backgrounds;
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float maxMultiplier;

    void Start()
    {
        backgrounds = new List<MoveBackground>(GetComponentsInChildren<MoveBackground>());
    }

    public void UpdateSpeed()
    {
        foreach (MoveBackground moveBg in backgrounds)
        {
            moveBg.speed *=speedMultiplier;
        }
    }

    private float cc=0;
    
    public void Update()
    {
        cc+= Time.deltaTime;
        if(cc > 2f)
        {
            cc = 0;
            UpdateSpeed();
            EPlayerController.Instance.AnimationSpeed *= speedMultiplier;
        }
    }
}
