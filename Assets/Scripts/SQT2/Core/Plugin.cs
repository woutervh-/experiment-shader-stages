using System;
using UnityEngine;

namespace SQT2.Core
{
    public abstract class Plugin : MonoBehaviour
    {
        public event EventHandler OnChange;

        void OnValidate()
        {
            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnPluginStart()
        {
            //
        }

        public void OnPluginStop()
        {
            //
        }
    }
}
