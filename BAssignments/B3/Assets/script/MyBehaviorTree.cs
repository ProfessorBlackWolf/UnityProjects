﻿using UnityEngine;
using System.Collections;
using TreeSharpPlus;
using System;

namespace RootMotion.FinalIK.Demos
{
    public class MyBehaviorTree : MonoBehaviour
    {
        public Transform sk_Wander1, sk_Wander2, sk_Wander3, orientation;
        public Transform cop_Wander1, cop_Wander2, cop_Wander3, cop_Wander4, cop_Wander5, cop_Wander6, cop_Wander11, cop_Wander22;
        public Transform thief_Wander1, thief_Wander2, thief_Wander3,hide_Point;
        public Transform stealSpot;
        public GameObject shopKeeper, Cop, Thief;
        public InteractionObject ball, button;

        private BehaviorAgent behaviorAgent;
        // Use this for initialization
        void Start()
        {
            behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
            print(cop_Wander1);
            BehaviorManager.Instance.Register(behaviorAgent);
            behaviorAgent.StartBehavior();
        }

        // Update is called once per frame
        void Update()
        {

        }
        protected Node WalkRun(Transform target, GameObject participant, string state)
        {
            Val<Vector3> position = Val.V(() => target.position);
            if(state == "walk")
            {
                print(target);
                participant.GetComponent<UnitySteeringController>().minSpeed += 0.5f;
                participant.GetComponent<UnitySteeringController>().maxSpeed += 2.2f;
            }
            else
            {
                participant.GetComponent<UnitySteeringController>().minSpeed += 4.0f;
                participant.GetComponent<UnitySteeringController>().maxSpeed += 6.0f;
            }
            
            return new Sequence(
                participant.GetComponent<BehaviorMecanim>().Node_GoTo(position)
                );
        }
        protected Node ST_ApproachAndWait(Transform target, Transform orientation, GameObject participant)
        {
            Val<Vector3> position = Val.V(() => target.position);
            Val<Vector3> orient = Val.V(() => orientation.position);
            return new Sequence(
                participant.GetComponent<BehaviorMecanim>().Node_GoTo(position),
                participant.GetComponent<BehaviorMecanim>().Node_OrientTowards(orient),
                new LeafWait(1000)
                );
        }
        //Run giving problem--think later
        protected Node BuildTreeRoot()
        {
            Func<bool> act = () => ( Math.Abs(this.Cop.transform.position.z - this.Thief.transform.position.z + this.Cop.transform.position.x - this.Thief.transform.position.x) > 5);
            Func<bool> near = () => (Math.Abs(this.Cop.transform.position.z - this.Thief.transform.position.z + this.Cop.transform.position.x - this.Thief.transform.position.x) > 5);
            Func<bool> goNearButton = () => (Math.Abs(this.shopKeeper.transform.position.x - this.sk_Wander3.position.x + this.shopKeeper.transform.position.z - this.sk_Wander3.position.z) < 6);
            //Func<bool> picked = () => (this.Thief.ch)
            Node Cop_Roaming = new Sequence(
                this.ST_ApproachAndWait(this.cop_Wander11, this.orientation, this.Cop)
                );
            Node Thief_Roaming = new Sequence(
                    this.ST_ApproachAndWait(this.stealSpot, this.sk_Wander3, this.Thief),
                    this.Thief.GetComponent<BehaviorMecanim>().Node_StartInteraction(FullBodyBipedEffector.LeftHand, this.ball));
            Node ShopKeeper_Roaming = new DecoratorLoop(
                new Sequence(
                    this.ST_ApproachAndWait(this.sk_Wander1, this.orientation, this.shopKeeper),
                    this.ST_ApproachAndWait(this.sk_Wander2, this.orientation, this.shopKeeper),
                    this.ST_ApproachAndWait(this.sk_Wander1, this.orientation, this.shopKeeper),
                    this.ST_ApproachAndWait(this.sk_Wander3, this.orientation, this.shopKeeper)
                //new LeafAssert(Thief_Status)
                ));
  
            Node escape = new Sequence(this.ST_ApproachAndWait(this.thief_Wander1, this.orientation, this.Thief),
                this.ST_ApproachAndWait(this.hide_Point, this.orientation, this.Thief)
                );
            Node trigger = new DecoratorLoop(new LeafAssert(act));
            Node thiefEscape = new DecoratorForceStatus(RunStatus.Success, new SequenceParallel(trigger, escape));
            Node buttonTrigger = new DecoratorLoop(new LeafAssert(goNearButton));
            Node Panic = new Sequence(
                   this.ST_ApproachAndWait(this.sk_Wander1, this.orientation, this.shopKeeper),
                   this.ST_ApproachAndWait(this.sk_Wander3, this.orientation, this.shopKeeper),
                   this.shopKeeper.GetComponent<BehaviorMecanim>().Node_StartInteraction(FullBodyBipedEffector.RightHand, this.button)
               );
            if (buttonTrigger.LastStatus == RunStatus.Success)
            {
                Panic = new Sequence(
                    this.ST_ApproachAndWait(this.sk_Wander3, this.orientation, this.shopKeeper),
                    this.shopKeeper.GetComponent<BehaviorMecanim>().Node_StartInteraction(FullBodyBipedEffector.RightHand, this.button)
                );
            }

            
            // Node RunTrigger = new DecoratorLoop(new LeafAssert(picked));

           /* Node trigger = new DecoratorLoop(new LeafAssert(act));
            Node thief = new DecoratorLoop(new DecoratorForceStatus(RunStatus.Success, new SequenceParallel(trigger, Thief_Roaming)));
            Node story = new SelectorParallel(ShopKeeper_Roaming, Thief_Roaming, Cop_Roaming);
            if (buttonTrigger.LastStatus == RunStatus.Success && trigger.LastStatus == RunStatus.Failure)
            {
               
            }*/
            Node Cop_shoot = new Sequence(this.ST_ApproachAndWait(this.cop_Wander11, this.Thief.transform, this.Cop),
                   this.Cop.GetComponent<BehaviorMecanim>().Node_HandAnimation("PISTOLAIM", true),
                   //new LeafWait(1000),
                   this.Cop.GetComponent<BehaviorMecanim>().Node_HandAnimation("PISTOLFIRE", true)
                   );
            Node Cop_talk = new Sequence(
                this.ST_ApproachAndWait(this.stealSpot, this.shopKeeper.transform, this.Cop),
                new SequenceShuffle(this.Cop.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("SURPRISED", 500), new LeafWait(500),
                this.shopKeeper.GetComponent<BehaviorMecanim>().ST_PlayHandGesture("POINTING", 500), new LeafWait(500),
                this.Cop.GetComponent<BehaviorMecanim>().ST_PlayBodyGesture("ACKNOWLEDGE", 500), new LeafWait(500),
                this.shopKeeper.GetComponent<BehaviorMecanim>().ST_PlayFaceGesture("HEADNOD", 500), new LeafWait(500)),
                this.Cop.GetComponent<BehaviorMecanim>().ST_PlayBodyGesture("TALKING ON PHONE", 500), new LeafWait(1000)
                //this.ST_ApproachAndWait(this.cop_Wander1, this.Cop1.transform, this.Cop)
                );
            Node cop_trigger = new DecoratorLoop(new LeafAssert(near));
            Node copAction = new DecoratorForceStatus(RunStatus.Success, new SequenceParallel(cop_trigger, Cop_talk));
            Node Theif_Die = new Sequence(this.Thief.GetComponent<BehaviorMecanim>().Node_BodyAnimation("DIE",true));
            Node reset = new Sequence(this.Cop.GetComponent<BehaviorMecanim>().Node_HandAnimation("PISTOLFIRE", false), 
                this.Cop.GetComponent<BehaviorMecanim>().Node_HandAnimation("PISTOLAIM", false));
            Node root = new Sequence(
            new SelectorParallel(Thief_Roaming, ShopKeeper_Roaming, Cop_Roaming),
            new SequenceParallel(
                new Sequence(new DecoratorForceStatus(RunStatus.Success, new SequenceParallel(Panic, thiefEscape)),
                    Cop_shoot,
                    Theif_Die,
                    new LeafWait(500),
                    reset),
                new Sequence(new DecoratorForceStatus(RunStatus.Success, new Sequence( copAction)))
                )
            );
            return root;

        }
    }
}
