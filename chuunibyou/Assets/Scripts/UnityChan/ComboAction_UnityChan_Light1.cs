using UnityEngine;
using System.Collections;

namespace chuunibyou
{
    public class ComboAction_UnityChan_Light1: ComboActionNode 
    {
        private float delay;

        protected override void OnBegin()
        {
            // play animation for 0.5s
            delay = 0.3f;
            nextActionInputWindowTime = 0.5f;
        }

        public override void OnEnd()
        {
        }

        public override void OnFixedUpdate()
        {
            delay -= Time.fixedDeltaTime;
            if (delay <= 0.0f)
            {
                isDone = true;
            }
        }
    }
}