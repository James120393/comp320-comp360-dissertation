﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using System.Threading;
using SWIG.BWAPI;
using SWIG.BWTA;
using POSH_StarCraftBot.logic;

namespace POSH_StarCraftBot.behaviours
{

    public enum Strategy { EighteenNexusOpening = 0, TwoHatchMuta = 1, Zergling = 2 }

    public class StrategyControl : AStarCraftBehaviour
    {
        private Unit scout;
        private Position lureCentroid;
        private Unit probeScout;
        /// <summary>
        /// The scout counter contains the number of bases we already discovered moving from Startlocation towards the most distant ones.
        /// bases are retrieved using bwta.getBaselocations
        /// </summary>
        private int scoutCounter = 1;
        private Strategy currentStrategy;
        private bool startStrategy = true;
        private float alarm = 0.0f;
        private GamePhase phase;


        public StrategyControl(AgentBase agent)
            : base(agent, new string[] { }, new string[] { })
        {

        }
        //
        // INTERNAL
        //
        private bool SwitchBuildToBase(int location)
        {
            if (Interface().baseLocations.ContainsKey(location) && Interface().baseLocations[location] is TilePosition)
            {
                Interface().currentBuildSite = (BuildSite)location;
                return true;
            }

            return false;
        }

        //
        // ACTIONS
        //
        [ExecutableAction("SelectNatural")]
        public bool SelectNatural()
        {
            return SwitchBuildToBase((int)BuildSite.Natural);
        }

        [ExecutableAction("SelectStartBase")]
        public bool SelectStartBase()
        {
            return SwitchBuildToBase((int)BuildSite.StartingLocation);
        }

        [ExecutableAction("SelectExtension")]
        public bool SelectExtension()
        {
            return SwitchBuildToBase((int)BuildSite.Extension);
        }

        [ExecutableAction("Idle")]
        public bool Idle()
        {
            try
            {
                Thread.Sleep(50);
                return true;
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Idle Action Crashed");
                return false;
            }
        }

        [ExecutableAction("RespondToLure")]
        public bool RespondToLure()
        {
            IEnumerable<Unit> probes = Interface().GetProbes().Where(probe => probe.isUnderAttack());

            if (probes.Count() > 1)
                lureCentroid = CombatControl.CalculateCentroidPosition(probes);
            foreach (Unit probe in probes)
            {
                if (probe.isCarryingMinerals())
                {
                    probe.gather(Interface().GetAssimilator().OrderBy(extr => extr.getDistance(probe)).First());
                }
                else if (probe.isCarryingGas())
                {
                    probe.gather(Interface().GetMineralPatches().OrderBy(extr => extr.getDistance(probe)).First());
                }
                else
                {
                    probe.move(new Position(Interface().baseLocations[(int)BuildSite.StartingLocation]));
                }
                alarm += 0.1f;

            }

            return alarm > 0 ? true : false;
        }


        [ExecutableAction("SelectLuredBase")]
        public bool SelectLuredBase()
        {
            int location = Interface().baseLocations.OrderBy(loc => new Position(loc.Value).getApproxDistance(lureCentroid)).First().Key;
            ForceLocations target;
            switch (location)
            {
                case (int)ForceLocations.OwnStart:
                    target = ForceLocations.OwnStart;
                    break;
                case (int)ForceLocations.Natural:
                    target = ForceLocations.Natural;
                    break;
                case (int)ForceLocations.Extension:
                    target = ForceLocations.Extension;
                    break;
                default:
                    target = ForceLocations.Natural;
                    break;

            }
            Interface().currentForcePoint = target;
            return true;


        }

        [ExecutableAction("CounterWithForce")]
        public bool CounterWithForce()
        {
            //TODO: this needs to be implemented
         return false;
        }


        [ExecutableAction("SelectProbeScout")]
        public bool SelectProbeScout()
        {
            Unit scout = null;
            IEnumerable<Unit> units = Interface().GetProbes().Where(probe =>
                probe.getHitPoints() > 0 &&
                !Interface().IsBuilder(probe));

            foreach (Unit unit in units)
            {
                if (!unit.isCarryingGas() && !unit.isCarryingMinerals())
                {
                    scout = unit;
                    break;
                }
            }
            if (scout == null && units.Count() > 0)
            {
                scout = units.First();
            }
            probeScout = scout;

            return (probeScout is Unit && probeScout.getHitPoints() > 0) ? true : false;
        }

        [ExecutableAction("ProbeScouting")]
        public bool ProbeScouting()
        {
            if (probeScout == null || probeScout.getHitPoints() <= 0 || probeScout.isConstructing())
                return false;

            if (scoutCounter == bwta.getBaseLocations().Where(loc =>
                    bwta.getGroundDistance(loc.getTilePosition(), probeScout.getTilePosition()) >= 0).Count() )
                scoutCounter = 1;


            if (probeScout.isUnderAttack())
            {
                if (Interface().baseLocations.ContainsKey((int)BuildSite.Natural))
                    probeScout.move(new Position(Interface().baseLocations[(int)BuildSite.Natural]));
                else
                    probeScout.move(new Position(Interface().baseLocations[(int)BuildSite.StartingLocation]));
                return false;
            }

            if (probeScout.isMoving())
                return true;

            if (probeScout.getPosition().getDistance(
                bwta.getBaseLocations().Where(loc =>
                    bwta.getGroundDistance(loc.getTilePosition(), probeScout.getTilePosition()) >= 0).OrderBy(loc =>
                    bwta.getGroundDistance(loc.getTilePosition(),Interface().baseLocations[(int)BuildSite.StartingLocation]))
                    .ElementAt(scoutCounter-1)
                    .getPosition()
                    ) < DELTADISTANCE)
            {
                // close to another base location
                if (!Interface().baseLocations.ContainsKey(scoutCounter))
                    Interface().baseLocations[scoutCounter] = new TilePosition(probeScout.getTargetPosition());
                scoutCounter++;
            }
            else
            {

                bool executed = probeScout.move(bwta.getBaseLocations().Where(loc =>
                    bwta.getGroundDistance(loc.getTilePosition(), probeScout.getTilePosition()) >= 0).OrderBy(loc =>
                    bwta.getGroundDistance(loc.getTilePosition(),Interface().baseLocations[(int)BuildSite.StartingLocation]))
                    .ElementAt(scoutCounter-1)
                    .getPosition());
                // if (_debug_)
                Console.Out.WriteLine("Probe is scouting: " + executed); 
            }

            return true;
        }

        [ExecutableAction("Pursue18NexusOpening")]
        public bool Pursue18NexusOpening()
        {
            currentStrategy = Strategy.EighteenNexusOpening;
            return (currentStrategy == Strategy.EighteenNexusOpening) ? true : false;
        }

        [ExecutableAction("SelectChoke")]
        public bool SelectChoke()
        {
            // get the distance between start and natural 
            BuildSite site = Interface().currentBuildSite;
            TilePosition start = Interface().baseLocations[(int)BuildSite.StartingLocation];
            TilePosition targetChoke = null;
            Chokepoint chokepoint = null;

            if (site != BuildSite.StartingLocation && Interface().baseLocations.ContainsKey((int)site))
            {
                targetChoke = Interface().baseLocations[(int)site];
                double distance = start.getDistance(targetChoke);

                // find some kind of measure to determine if the the closest choke to natural is not the once between choke and start but after the natural
                IEnumerable<Chokepoint> chokes = bwta.getChokepoints().Where(ck => bwta.getGroundDistance(new TilePosition(ck.getCenter()), start) > 0).OrderBy(choke => choke.getCenter().getDistance(new Position(targetChoke)));


                foreach (Chokepoint ck in chokes)
                {

                    if (bwta.getGroundDistance(new TilePosition(ck.getCenter()), targetChoke) < bwta.getGroundDistance(new TilePosition(ck.getCenter()), start))
                    {
                        chokepoint = ck;
                        break;
                    }
                }
            }
            else
            {
                targetChoke = start;
                chokepoint = bwta.getChokepoints().Where(ck => bwta.getGroundDistance(new TilePosition(ck.getCenter()), start) > 0).OrderBy(choke => choke.getCenter().getDistance(new Position(start))).First();
            }



            if (chokepoint == null)
                return false;

            //picking the right side of the choke to position forces
            Interface().forcePoints[ForceLocations.NaturalChoke] = (targetChoke.getDistance(new TilePosition(chokepoint.getSides().first)) < targetChoke.getDistance(new TilePosition(chokepoint.getSides().second))) ? new TilePosition(chokepoint.getSides().first) : new TilePosition(chokepoint.getSides().second);
            Interface().currentForcePoint = ForceLocations.NaturalChoke;

            return true;

        }

        //
        // SENSES
        //

        [ExecutableSense("NaturalFound")]
        public bool NaturalFound()
        {
            return (Interface().baseLocations.ContainsKey((int)BuildSite.Natural) && Interface().baseLocations[(int)BuildSite.Natural] is TilePosition);
        }


        [ExecutableSense("CanCreateUnits")]
        public bool CanCreateUnits()
        {
            if (Interface().GetGateway().Count() == 0)
                return false;
            switch (currentStrategy){
                case Strategy.EighteenNexusOpening:
                    return (Interface().GetGateway().Count() > 0) && Interface().GetCyberneticsCore().Count() > 0 ? true : false;
                //case Strategy.TwoHatchMuta:
                    //return (Interface().GetCyberteticsCore().Count() > 0 && Interface().GetSpire().Count() > 0) ? true : false;
                //case Strategy.Zergling:
                    //return (Interface().GetHatcheries().Count() > 0 || Interface().GetLairs().Count() > 0 ) ? true : false;
                default:
                    break;
            }
            return false;
        }
        

        [ExecutableSense("DoneExploring")]
        public bool DoneExploring()
        {
            if (Interface().baseLocations.Count() == bwta.getBaseLocations().Count())
                return true;

            return false;
        }


        [ExecutableSense("DoneExploringLocal")]
        public bool DoneExploringLocal()
        {
            return Interface().baseLocations.ContainsKey((int)BuildSite.Natural);
        }

        /// <summary>
        /// Returns the enemy race once it is known. The options are: -1 for unknown, 0 for Zerg, 1 for Protoss, 2 for Human
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("EnemyRace")]
        public int EnemyRace()
        {
            // currently we only expect 1-on-1 matches so there should be only one other player in the game

            int enemyRace = Interface().ActivePlayers.First().Value.getRace().getID();
            if (enemyRace == bwapi.Races_Unknown.getID())
            {
                Interface().enemyRace = Races.Unknown;
                return (int)Races.Unknown;
            }
            if (enemyRace == bwapi.Races_Zerg.getID())
            {
                Interface().enemyRace = Races.Zerg;
                return (int)Races.Zerg;
            }
            if (enemyRace == bwapi.Races_Protoss.getID())
            {
                Interface().enemyRace = Races.Protoss;
                return (int)Races.Protoss;
            }
            if (enemyRace == bwapi.Races_Terran.getID())
            {
                Interface().enemyRace = Races.Terran;
                return (int)Races.Terran;
            }

            return -1;
        }


        [ExecutableSense("GameRunning")]
        public bool GameRunning()
        {
            return Interface().GameRunning();
        }

        /// <summary>
        /// Used to switch between different implemented strategies. If one is detected or changes are slim of using it Strategy is switched.
        /// "0" refers to 3HachHydra, "1" is not defined yet
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("FollowStrategy")]
        public int FollowStrategy()
        {
            //TODO: implement proper logic for dete cting when to switch between different strats.
            if (startStrategy)
            {
                currentStrategy = Strategy.EighteenNexusOpening;
                startStrategy = false;
            }
            return (int)currentStrategy;
        }

        [ExecutableSense("BuildArmy")]
        public bool BuildArmy()
        {

            return false;
        }


        [ExecutableSense("Alarm")]
        public float Alarm()
        {
            alarm = (alarm < 0.0f) ? 0.0f : alarm - 0.05f;

            return alarm;
        }


        [ExecutableSense("ProbeAssimilatorRatio")]
        public float ProbeAssimilatorRatio()
        {
            if (Interface().GetAssimilator().Count() < 0 || Interface().GetAssimilator().Where(ex => ex.isCompleted() && ex.getResources() > 0 && ex.getHitPoints() > 0).Count() < 1)
                return 1;
            if (Interface().GetProbes().Count() < 1 || Interface().GetProbes().Where(probe => probe.isGatheringMinerals()).Count() < 1)
                return 1;
            return Interface().GetProbes().Where(probe => probe.isGatheringGas()).Count() / Interface().GetProbes().Where(probe => probe.isGatheringMinerals()).Count();
        }


        [ExecutableSense("ProbesLured")]
        public bool ProbesLured()
        {
            return Interface().GetProbes().Where(probe => probe.isUnderAttack()).Count() > 0;
        }


        [ExecutableSense("ProbeScoutAvailable")]
        public bool ProbeScoutAvailable()
        {
            return (probeScout is Unit && probeScout.getHitPoints() > 0 && !probeScout.isConstructing() && !probeScout.isRepairing()) ? true : false;
        }        
    }
}
