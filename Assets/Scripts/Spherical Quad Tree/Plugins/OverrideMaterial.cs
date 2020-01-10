using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class OverrideMaterial : MonoBehaviour, SQT.Core.Plugin, SQT.Core.MaterialPlugin
    {
        public Material material;

        public event EventHandler OnChange;

        void OnValidate()
        {
            if (OnChange != null)
            {
                OnChange.Invoke(this, EventArgs.Empty);
            }
        }

        public void StartPlugin() { }

        public void StopPlugin() { }

        public void ModifyMaterial(ref Material material)
        {
            material = this.material;
        }
    }
}
