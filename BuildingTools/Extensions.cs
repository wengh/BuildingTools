
using System;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.PlayerProfiles;
using UnityEngine;

namespace BuildingTools
{
    public class Lazy<T>
    {
        private Func<T> make;
        public Lazy(Func<T> make)
        {
            this.make = make;
        }

        private T _value;
        private bool _made;

        public T Value
        {
            get
            {
                if (!_made)
                {
                    _value = make();
                    _made = true;
                }
                return _value;
            }
        }
    }
}
