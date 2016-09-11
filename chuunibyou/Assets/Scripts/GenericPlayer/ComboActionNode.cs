using UnityEngine;
using System.Collections;

namespace chuunibyou
{
    
    public abstract class ComboActionNode
    {
        public ComboActionNode[] next;
        public ComboManager manager;
        public ComboActionNode prev;

        public bool isDone {get; protected set;}
        public float nextActionInputWindowTime { get; protected set; }

        public ComboActionNode()
        {
            // set them all to null
            next = new ComboActionNode[(int)ComboActionType.Count];
            for (int i = 0; i < next.Length; ++i)
                next[i] = null;

            // reset
            isDone = false;
            // default 500ms
            nextActionInputWindowTime = 0.5f;
        }

        public void Begin()
        {
            // reset
            isDone = false;

            // call child handler
            OnBegin();
        }

        protected virtual void OnBegin()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnEnd()
        {
        }

        public virtual void OnWeaponTriggerEnter(Collider other)
        {
        }
    }

    public class ComboAction_Empty: ComboActionNode 
    {
        private float delay;

        // do nothing for 2 second
        protected override void OnBegin()
        {
            delay = 2.0f;
            nextActionInputWindowTime = 1.0f; // 1 sec delay between
            Debug.Log("ComboAction_Empty Begin: " + Time.fixedTime);
        }

        public override void OnEnd()
        {
            Debug.Log("ComboAction_Empty End: " + Time.fixedTime);
        }

        public override void OnFixedUpdate()
        {
            delay -= Time.fixedDeltaTime;
            if (delay <= 0.0f)
            {
                Debug.Log("ComboAction_Empty Delay expire: " + Time.fixedTime);
                isDone = true;
            }
        }
    }
}