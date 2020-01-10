using System;
using UnityEngine;

namespace SQT.Plugins
{
    public class CPUReconciler : MonoBehaviour, SQT.Core.Plugin, SQT.Core.ReconcilerFactoryPlugin
    {
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

        public void ModifyReconcilerFactory(ref SQT.Core.ReconcilerFactory reconcilerFactory)
        {
            reconcilerFactory = new SQT.Core.CPU.Reconciler.Factory();
        }
    }
}
