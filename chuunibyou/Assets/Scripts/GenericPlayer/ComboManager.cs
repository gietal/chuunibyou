using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace chuunibyou
{
    public enum ComboActionType 
    {
        LightAttack = 0,
        HeavyAttack,
        Count
    }

    public abstract class ComboManager : MonoBehaviour 
    {
        public enum Status
        {
            Idle = 0,
            WaitingForNextComboInput,
            RunningCombo
        }

        ComboActionNode initialNode = new ComboAction_Empty();
        ComboActionNode currentNode;

        List<ComboActionType> actionsBuffer = new List<ComboActionType>();

        public bool isBusy  { get; protected set; }

        public Status status
        {
            get
            {
                if (isBusy)
                    return Status.RunningCombo;
                // !isBusy
                if (actionsBuffer.Count > 0)
                    return Status.WaitingForNextComboInput;
                // !isBusy && actionsBuffer.Count == 0
                return Status.Idle;
            }
        }

        private float nextActionInputWindowTime = 0.0f;

        void Awake()
        {
            currentNode = initialNode;
            RegisterCombos();
        }

        void Start()
        {
            Reset();
        }

        public void RegisterCombo(ComboActionNode comboNode, params ComboActionType[] actions)
        {
            Debug.AssertFormat(comboNode != null, "cannot register null combo node");
            Debug.AssertFormat(actions.Length != 0, "cannot register combo with no actions");

            ComboActionNode curNode = initialNode;

            string comboString = "";

            // walk till the last node
            for (int i = 0; i < actions.Length - 1; ++i)
            {
                ComboActionType action = actions[i];

                #if DEBUG
                comboString += action.ToString() + ", ";
                Debug.AssertFormat(curNode.next[(int)action] != null, this.name + " combo ({0}) is null, please register this combo first", comboString);
                #endif

                curNode = curNode.next[(int)action];
            }
            ComboActionType lastAction = actions[actions.Length -1]; // get the last action

            #if DEBUG
            comboString += lastAction.ToString();
            // need to separate it like this, because we're checking for null
            // but we will grab the value from the null for the call to Debug.assertformat as a parameter
            // so need to make sure that it's not null first
            if(curNode.next[(int)lastAction] != null)
                Debug.AssertFormat(false, this.name + " combo ({0}) already exist: {1}", comboString, curNode.next[(int)lastAction].GetType().ToString());
            #endif

            // set the next action to be the given node
            curNode.next[(int)lastAction] = comboNode;

            // set the previous action node of the given to be the current node
            comboNode.prev = curNode;

            // save the manager
            comboNode.manager = this;

            Debug.Log(String.Format("{0} registered combo [{1}] -> [{2}]", this.name, comboString, comboNode.GetType().ToString()));
        }

        public void DoAction(ComboActionType action)
        {
            // if busy dont bother
            if (status == Status.RunningCombo)
                return;
            
            // get the next action
            var nextAction = currentNode.next[(int)action];

            // check if action possible
            if (nextAction == null)
            {
                // action not possible, reset
                Debug.Log("no more action available, Resetting");
                Reset();
                return;
            }
                
            // move current node to the next action
            actionsBuffer.Add(action);
            currentNode = nextAction;
            isBusy = true;
            Debug.Log(String.Format(this.name + " combo: [{0}] -> {1}", GetActionsBufferString(), currentNode.GetType().ToString()));

            // begin the action
            currentNode.Begin();

        }
                        
        public void Reset()
        {
            Debug.Log("combo Reset");
            currentNode = initialNode;
            isBusy = false;
            actionsBuffer.Clear();
        }

        public String GetActionsBufferString()
        {
            string output = String.Empty;
            if (actionsBuffer.Count == 0)
                return output;
            
            for(int i = 0; i < actionsBuffer.Count - 1; ++i)
                output += actionsBuffer[i].ToString() + ", ";
            output += actionsBuffer[actionsBuffer.Count - 1].ToString();

            return output;
        }

        void FixedUpdate()
        {
            switch (status)
            {
                case Status.RunningCombo:
                    {
                        // update the active node
                        currentNode.OnFixedUpdate();

                        // check if there's an active action and if it's done
                        if (currentNode.isDone)
                        {
                            // current action is done
                            currentNode.OnEnd();
                            isBusy = false;

                            // fire timer to reset after some time if user doesnt input 
                            //StartCoroutine(ResetDelayed(currentNode.nextActionInputWindowTime));
                            nextActionInputWindowTime = currentNode.nextActionInputWindowTime;
                        }
                        break;
                    }
                case Status.WaitingForNextComboInput:
                    {
                        // reduce wait timer
                        nextActionInputWindowTime -= Time.fixedDeltaTime;
                        if (nextActionInputWindowTime <= 0.0f)
                        {
                            Debug.Log("next action window time expire");
                            Reset();
                        }
                        break;
                    }
                case Status.Idle:
                    {
                        // do nothing
                        break;
                    }
            }


        }

        IEnumerator ResetDelayed(float seconds)
        {
            Debug.Log("ResetDelayed start");
            yield return new WaitForSeconds(seconds);
            Debug.Log("ResetDelayed going to call Reset()");
            Reset();
            Debug.Log("ResetDelayed end");
        }

        // child must implement this to register the combos
        protected abstract void RegisterCombos();
            
    }
}