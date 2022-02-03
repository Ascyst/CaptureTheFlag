using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using CaptureTheFlag.Objects;

namespace CaptureTheFlag.GameMode
{
    /// <summary>
    /// 
    /// A classic game mode played in teams, where each team has to capture and return home with the opposing team's flag.
    /// Each team's flag is within their home area, in the designated flag holder. Enemy players can take this flag by walking into it. 
    /// If a player holds the opposing teams flag and dies, they will drop the flag and it will be frozen where they died. 
    /// The parent team can return the flag by jumping into it, otherwise the opposing team can pick up the flag and continue trying to capture it. 
    /// The round is won by capturing the flag 5 times in a battle. The team that loses the round will be able to pick a new card per player after this. 
    /// First team to reach 3 rounds won, wins the game. 
    /// Players will respawn when killed, with a default respawn time of 1 second. This increases by 1 second for each point they are ahead of the enemy team.
    /// </summary>
    public class GM_CaptureTheFlag : RWFGameMode
    {
        internal static GM_CaptureTheFlag instance;

        private const float pointsNeededToWin = 5f;

        private const float spawnDelayPerPoint = 1f;
        private const float baseRespawnDelay = 1f;

        private List<int> awaitingRespawn = new List<int>() { };
        private FlagHandler flag;



        protected override void Awake()
        {
            GM_CaptureTheFlag.instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            GameObject _ = FlagPrefab.Flag;
            base.Start();
        }
        public void SetFlag(FlagHandler flagHandler)
        {
            this.flag = flagHandler;
        }
        public void DestroyFlag()
        {
            if (this.flag != null)
            {
                UnityEngine.GameObject.DestroyImmediate(this.flag);
            }
        }

    }
}