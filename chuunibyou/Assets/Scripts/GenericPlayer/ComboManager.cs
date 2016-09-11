using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        void Awake()
        {
            RegisterCombos();
        }

        void Start()
        {
            Reset();
        }

        public void RegisterCombo(ComboActionType[] actions, ComboActionNode comboNode)
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
                Debug.AssertFormat(curNode.next[(int)action] != null, "combo ({0}) is null, please register this combo before continuing", comboString);
                #endif

                curNode = curNode.next[(int)action];
            }
            ComboActionType lastAction = actions[actions.Length -1]; // get the last action

            #if DEBUG
            comboString += lastAction.ToString();
            Debug.AssertFormat(curNode.next[(int)lastAction] == null, "combo ({0}) already exist: {1}", comboString, curNode.next[(int)lastAction].GetType().ToString());
            #endif

            // set the next action to be the given node
            curNode.next[(int)lastAction] = comboNode;

            // set the previous action node of the given to be the current node
            comboNode.prev = curNode;

            // save the manager
            comboNode.manager = this;
        }

        public void DoAction(ComboActionType action)
        {
            // if busy dont bother
            if (isBusy)
                return;
            
            // get the next action
            var nextAction = currentNode.next[(int)action];

            // check if action possible
            if (nextAction == null)
            {
                // action not possible, reset
                Reset();
            }

            // kill corutine
            StopCoroutine("ResetDelayed");

            // perform this action
            currentNode = nextAction;
            currentNode.OnBegin();
            isBusy = true;

                   
        }
                        
        public void Reset()
        {
            currentNode = initialNode;
            isBusy = false;
            actionsBuffer.Clear();
        }

        void FixedUpdate()
        {
            // check if there's an active action and if it's done
            if (isBusy && currentNode.IsDone())
            {
                // current action is done
                isBusy = false;

                // fire timer to reset after some time if user doesnt input 
                StartCoroutine(ResetDelayed(currentNode.GetNextActionInputWindowTime()));
            }
        }

        IEnumerator ResetDelayed(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Reset();
        }

        // child must implement this to register the combos
        protected abstract void RegisterCombos();
            
    }
}