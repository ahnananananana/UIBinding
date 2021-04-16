using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HDV.UIBinding
{
    public class TestData : MonoBehaviour
    {
        [SerializeField] float _testFloat;
        [SerializeField] int _testInt;
        [SerializeField] Sprite _testImage;

        [UIBindable]
        public float TestFloat { get => _testFloat; set => _testFloat = value; }
        [UIBindable]
        public int TestInt { get => _testInt; set => _testInt = value; }
        [UIBindable]
        public Sprite TestImage { get => _testImage; set => _testImage = value; }
    }
}
