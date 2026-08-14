using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColliderEventSystem
{
    /// <summary>
    /// Shared logic for ColliderEvent (zone-based) and ConditionWatcher (always-on). Holds the ordered
    /// Condition/Action lists, folds Conditions left-to-right with each condition's And/Or Operator,
    /// waits for Minimum Hold Time, runs Actions once, then applies After Trigger.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class ColliderEventBase : MonoBehaviour
    {
        [SerializeField] private List<ConditionBase> conditions = new List<ConditionBase>();
        [SerializeField] private List<ActionBase> actions = new List<ActionBase>();
        [SerializeField] private List<ActionBase> exitActions = new List<ActionBase>();

        public List<ConditionBase> Conditions => conditions;
        public List<ActionBase> Actions => actions;
        public List<ActionBase> ExitActions => exitActions;

        [Tooltip("How many seconds the conditions must stay true, without interruption, before the Actions run. 0 fires as soon as they're true.")]
        public float holdTime = 0f;

        [Tooltip("What happens to this GameObject after the Actions have run.")]
        public AfterTrigger afterTrigger = AfterTrigger.DoNothing;

        [Tooltip("If true, logs to the console when this fires and when it exits.")]
        public bool debugLogging = true;

        /// <summary>
        /// The GameObject that triggered this, when relevant. Set by the owning subclass. Passed to
        /// Conditions/Actions whose RequiresCollisionObjectData is true.
        /// </summary>
        protected GameObject collidingObject;

        /// <summary>
        /// Whether Update() should currently be folding the condition list. Controlled by BeginChecking()/
        /// StopChecking(), not polled - subclasses push state changes instead of Update() polling for them.
        /// </summary>
        protected bool Checking { get; private set; }

        /// <summary>
        /// True from the moment Actions have run (with AfterTrigger.ExecuteExitActions) until Exit Actions fire.
        /// </summary>
        protected bool HasFired { get; private set; }

        private bool m_ActionsExecuting;
        private float m_HoldTimer;

        /// <summary>
        /// When true, Update() re-checks the condition fold while HasFired is true, and treats it flipping
        /// back to false as the exit trigger (used by ConditionWatcher, which has no physical exit event).
        /// ColliderEvent leaves this false - its exit is pushed from OnTriggerExit instead.
        /// </summary>
        protected virtual bool UseConditionFalseAsExit => false;

        protected virtual void Start()
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i]) conditions[i].OnAwake();
            }

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i]) actions[i].OnAwake();
            }

            for (int i = 0; i < exitActions.Count; i++)
            {
                if (exitActions[i]) exitActions[i].OnAwake();
            }
        }

        protected virtual void Update()
        {
            if (!Checking || m_ActionsExecuting)
            {
                return;
            }

            if (HasFired)
            {
                if (UseConditionFalseAsExit && !EvaluateConditions())
                {
                    RunExitActionsAndReset();
                }

                return;
            }

            if (EvaluateConditions())
            {
                m_HoldTimer += Time.deltaTime;

                if (m_HoldTimer >= holdTime)
                {
                    StartCoroutine(RunActionsCoroutine());
                }
            }
            else
            {
                m_HoldTimer = 0f;
            }
        }

        /// <summary>
        /// Starts (or resumes) evaluating the condition list every frame.
        /// </summary>
        protected void BeginChecking()
        {
            Checking = true;
        }

        /// <summary>
        /// Stops evaluating the condition list and resets the hold timer.
        /// </summary>
        protected void StopChecking()
        {
            Checking = false;
            m_HoldTimer = 0f;
        }

        /// <summary>
        /// Called by ColliderEvent's OnTriggerExit when the colliding object leaves while AfterTrigger is
        /// ExecuteExitActions and Actions have already run. Runs Exit Actions and resets state.
        /// </summary>
        protected void TriggerExit()
        {
            if (HasFired && afterTrigger == AfterTrigger.ExecuteExitActions)
            {
                RunExitActionsAndReset();
            }
        }

        /// <summary>
        /// Called after AfterTrigger.DoNothing (or Exit Actions) has finished resetting. ColliderEvent
        /// overrides this to require leaving and re-entering the zone before firing again; ConditionWatcher
        /// overrides it to require the Conditions to go false before it can fire again.
        /// </summary>
        protected virtual void OnFiredAndReset() { }

        protected bool EvaluateConditions()
        {
            bool result = true;

            for (int i = 0; i < conditions.Count; i++)
            {
                ConditionBase condition = conditions[i];
                if (!condition) continue;

                bool met = condition.RequiresCollisionObjectData
                    ? condition.Evaluate(collidingObject)
                    : condition.Evaluate();

                if (i == 0)
                {
                    result = met;
                }
                else if (condition.Operator == LogicOperator.And)
                {
                    result = result && met;
                }
                else
                {
                    result = result || met;
                }
            }

            return result;
        }

        private IEnumerator RunActionsCoroutine()
        {
            m_ActionsExecuting = true;

            if (debugLogging)
            {
                Debug.Log(gameObject.name + " (Collider Event System) fired.", this);
            }

            float waitTime = 0f;

            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (!action) continue;

                if (action.RequiresCollisionObjectData)
                {
                    action.Execute(collidingObject);
                }
                else
                {
                    action.Execute();
                }

                if (action.Duration > waitTime)
                {
                    waitTime = action.Duration;
                }
            }

            if (afterTrigger == AfterTrigger.SetInactive ||
                afterTrigger == AfterTrigger.Destroy ||
                afterTrigger == AfterTrigger.DestroyParent)
            {
                yield return new WaitForSeconds(waitTime);
            }

            switch (afterTrigger)
            {
                case AfterTrigger.SetInactive:
                    gameObject.SetActive(false);
                    break;

                case AfterTrigger.Destroy:
                    Destroy(gameObject);
                    break;

                case AfterTrigger.DestroyParent:
                    Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
                    break;

                case AfterTrigger.DoNothing:
                    m_ActionsExecuting = false;
                    ResetAfterFiring();
                    break;

                case AfterTrigger.ExecuteExitActions:
                    m_ActionsExecuting = false;
                    HasFired = true;
                    break;
            }
        }

        private void RunExitActionsAndReset()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i]) actions[i].CancelExecution();
            }

            if (debugLogging)
            {
                Debug.Log(gameObject.name + " (Collider Event System) exited.", this);
            }

            for (int i = 0; i < exitActions.Count; i++)
            {
                ActionBase action = exitActions[i];
                if (!action) continue;

                if (action.RequiresCollisionObjectData)
                {
                    action.Execute(collidingObject);
                }
                else
                {
                    action.Execute();
                }
            }

            ResetAfterFiring();
        }

        private void ResetAfterFiring()
        {
            HasFired = false;
            m_HoldTimer = 0f;

            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i]) conditions[i].ResetState();
            }

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i]) actions[i].ResetState();
            }

            OnFiredAndReset();
        }
    }
}
