using CarJack.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTester : MonoBehaviour
{
    public DrivableCar CarToTest;
    private void Awake()
    {
        var camera = FindObjectOfType<CarCamera>();
        camera.SetTarget(CarToTest);
        CarToTest.Driving = true;
    }
}
