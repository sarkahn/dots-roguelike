using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

using RLTKTutorial.Common.Tests;
using Unity.Collections;

namespace RLTKTutorial.Part1_5A.Tests
{
    [TestFixture]
    public class TurnSystemTests : ECSTestsFixture
    { 

        [Test]
        public void TurnSystemDistributesEnergyUntilThreshold()
        {
            var em = m_Manager;

            AddSystem<GameTurnSystem>();

            var slowUnit = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            em.SetComponentData<Speed>(slowUnit, 25);

            var fastUnit = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            em.SetComponentData<Speed>(fastUnit, 45);

            // Turn System updates until any actor is at or above 100 energy.
            Update();

            Assert.AreEqual(135, em.GetComponentData<Energy>(fastUnit).value);
            Assert.AreEqual(75, em.GetComponentData<Energy>(slowUnit).value);
        }

        [Test]
        public void TurnSystemStopsDistributingEnergyWhenAUnitCanAct()
        {
            var em = m_Manager;

            AddSystem<GameTurnSystem>();

            var slowUnit = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            em.SetComponentData<Speed>(slowUnit, 25);

            var fastUnit = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            em.SetComponentData<Speed>(fastUnit, 45);

            Update();

            Assert.AreEqual(135, em.GetComponentData<Energy>(fastUnit).value);
            Assert.AreEqual(75, em.GetComponentData<Energy>(slowUnit).value);

            //Assert.IsTrue(em.HasComponent<TakingATurn>(fastUnit));

            // Turn system should do nothing while any unit is taking it's turn
            // but hasn't performed any actions
            Update();
            Update();
            Update();

            Assert.AreEqual(135, em.GetComponentData<Energy>(fastUnit).value);
            Assert.AreEqual(75, em.GetComponentData<Energy>(slowUnit).value);
        }

        [Test]
        public void TurnSystemContinuesDistributingTurnsAfterActorActs()
        {
            var em = m_Manager;

            AddSystem<GameTurnSystem>();
            AddSystem<EndSimulationEntityCommandBufferSystem>();

            var slowUnit = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            em.SetComponentData<Speed>(slowUnit, 25);

            var fastUnit = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            em.SetComponentData<Speed>(fastUnit, 45);

            // Distribute energy until someone can take a turn
            Update();

            // Perform an action
            em.AddComponentData(fastUnit, new ActionPerformed { cost = 100 });

            // Process action
            Update();

            // Continue distributing energy
            Update();

            Assert.AreEqual(100, em.GetComponentData<Energy>(slowUnit).value);
            //Assert.IsTrue(em.HasComponent<TakingATurn>(slowUnit));
        }

        [Test]
        public void TurnSystemHandsOutTurnsAtEnergyThreshold()
        {
            AddSystem<GameTurnSystem>();

            var em = m_Manager;

            // Note: No Speed, so energy will not be increased by turn system
            var actor = em.CreateEntity(typeof(Actor), typeof(Energy));

            em.SetComponentData<Energy>(actor, 100);

            Update();

            //Assert.IsTrue(em.HasComponent<TakingATurn>(actor));
        }

        [Test]
        public void TurnSystemProcessesPerformedAction()
        {
            AddSystem<GameTurnSystem>();

            var em = m_Manager;

            // No speed, energy will not be increased by turn system
            var actor = em.CreateEntity(typeof(Actor), typeof(Energy));

            em.SetComponentData<Energy>(actor, 100);

            Update();

            em.AddComponentData(actor, new ActionPerformed
            {
                cost = 75
            });

            Update();

            //Assert.IsFalse(em.HasComponent<TakingATurn>(actor));
            Assert.IsFalse(em.HasComponent<ActionPerformed>(actor));
            Assert.AreEqual(25, em.GetComponentData<Energy>(actor).value);
        }

        [Test]
        public void TurnSystemHandsOutActionsAccordingToSpeed()
        {
            AddSystem<GameTurnSystem>();
            AddSystem<EndSimulationEntityCommandBufferSystem>();

            var em = m_Manager;
            var fast = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            var faster = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            var fastest = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            var fastestest = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));

            em.SetComponentData<Speed>(faster, 75);
            em.SetComponentData<Speed>(fast, 65);
            em.SetComponentData<Speed>(fastestest, 99);
            em.SetComponentData<Speed>(fastest, 85);

            Update();

            Assert.AreEqual(3, World.GetOrCreateSystem<GameTurnSystem>().CopyTurnBuffer(Allocator.Temp).Length);

           // Assert.IsTrue(em.HasComponent<TakingATurn>(fastestest));
            em.AddComponentData(fastestest, new ActionPerformed { cost = 100 });
            
            Update();

            Assert.AreEqual(2, World.GetOrCreateSystem<GameTurnSystem>().CopyTurnBuffer(Allocator.Temp).Length);

            //Assert.IsTrue(em.HasComponent<TakingATurn>(fastest));
            em.AddComponentData(fastest, new ActionPerformed { cost = 100 });

            Update();
            
            Assert.AreEqual(1, World.GetOrCreateSystem<GameTurnSystem>().CopyTurnBuffer(Allocator.Temp).Length);

            //Assert.IsTrue(em.HasComponent<TakingATurn>(faster));
            em.AddComponentData(faster, new ActionPerformed { cost = 100 });

            Update();

            Assert.AreEqual(0, World.GetOrCreateSystem<GameTurnSystem>().CopyTurnBuffer(Allocator.Temp).Length);

            //Assert.IsTrue(em.HasComponent<TakingATurn>(fast));
        }

        [Test]
        public void TurnSystemOnlyHandsOutOneTurnAtATime()
        {
            AddSystem<GameTurnSystem>();
            AddSystem<EndSimulationEntityCommandBufferSystem>();

            var em = m_Manager;
            var fast = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            var faster = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            var fastest = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));
            var fastestest = em.CreateEntity(typeof(Actor), typeof(Speed), typeof(Energy));

            em.SetComponentData<Speed>(faster, 75);
            em.SetComponentData<Speed>(fast, 65);
            em.SetComponentData<Speed>(fastestest, 99);
            em.SetComponentData<Speed>(fastest, 85);

            Update();

            //Assert.IsTrue(em.HasComponent<TakingATurn>(fastestest));

            //Assert.IsFalse(em.HasComponent<TakingATurn>(fast));
            //Assert.IsFalse(em.HasComponent<TakingATurn>(faster));
            //Assert.IsFalse(em.HasComponent<TakingATurn>(fastest));

            //Update();
            
            //Assert.IsTrue(em.HasComponent<TakingATurn>(fastestest));

            //Assert.IsFalse(em.HasComponent<TakingATurn>(fast));
            //Assert.IsFalse(em.HasComponent<TakingATurn>(faster));
            //Assert.IsFalse(em.HasComponent<TakingATurn>(fastest));


        }
    }
}