using UnityEngine;
using System.Collections;

namespace chuunibyou
{
    
    public abstract class ComboActionNode 
    {
        public ComboActionNode[] next;
        public ComboManager manager;
        public ComboActionNode prev;

        public ComboActionNode()
        {
            // set them all to null
            next = new ComboActionNode[(int)ComboActionType.Count];
            for (int i = 0; i < next.Length; ++i)
                next[i] = null;
        }

        public virtual void OnBegin()
        {
        }

        public virtual void OnWeaponTriggerEnter(Collider other)
        {
        }

        public virtual bool IsDone()
        {
            return true;
        }

        public virtual float GetNextActionInputWindowTime()
        {
            // default 500ms idle to reset
            return 0.5f;
        }
    }

    public class ComboAction_Empty: ComboActionNode 
    {
        // do nothing
    }
}