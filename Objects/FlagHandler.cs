using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.GameModes;
using UnityEngine;
using CaptureTheFlag.Objects;
using UnboundLib;
using CaptureTheFlag.GameMode;
using System.Collections;

namespace CaptureTheFlag.Objects
{
    public static class FlagPrefab
    {
        private static GameObject _Flag = null;

        public static GameObject Flag
        {
            get
            {
                if (FlagPrefab._Flag == null)
                {
                    GM_ArmsRace gm = GameModeManager.GetGameMode<GM_ArmsRace>(GameModeManager.ArmsRaceID);
                    GameObject flag = GameObject.Instantiate(gm.gameObject.transform.GetChild(0).gameObject);
                    CaptureTheFlag.Log("Flag Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(flag);
                    flag.name = "FlagPrefab";
                    // gotta give it a photon view, says pykess
                    flag.AddComponent<PhotonView>();
                    FlagHandler flagHandler = flag.AddComponent<FlagHandler>();
                    flagHandler.transitionCurve = new AnimationCurve((Keyframe[])flag.GetComponent<GameCrownHandler>().transitionCurve.InvokeMethod("GetKeys"));

                    UnityEngine.GameObject.DestroyImmediate(flag.GetComponent<GameCrownHandler>());

                    PhotonNetwork.PrefabPool.RegisterPrefab(flag.name, flag);

                    FlagPrefab._Flag = flag;

                }
                return FlagPrefab._Flag;
            }
        }
    }
    public class FlagHandler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
    {



        private static FlagHandler instance;

        private const float TriggerRadius = 1.5f;

        public AnimationCurve transitionCurve;


        private bool hidden = true;
        private float flagPos;
        private int currentFlagHolder = -1;
        internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();
        public int FlagHolder => this.currentFlagHolder;

        internal static void DestroyFlag()
        {
            GM_CaptureTheFlag.instance.DestroyFlag();
            if (FlagHandler.instance != null)
            {
                UnityEngine.GameObject.DestroyImmediate(FlagHandler.instance);
            }

        }
        internal static IEnumerator MakeCrownHandler()
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(FlagPrefab.Flag.name, GM_CaptureTheFlag.instance.transform.position, GM_CaptureTheFlag.instance.transform.rotation, 0);
            }
            yield return new WaitUntil(() => FlagHandler.instance != null);

        }
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            this.transform.localScale = Vector3.one;
            this.transform.GetChild(0).localScale = new Vector3(0.5f, 0.4f, 1f);
            this.Trig.radius = FlagHandler.TriggerRadius;
            this.Col.size = new Vector2(1f, 0.5f);
            this.Col.edgeRadius = 0.1f;
            base.Start();
        }

        public void Reset()
        {
            this.hidden = true;
            this.currentFlagHolder = -1;
        }

        public void Spawn(int TeamID, GameObject FlagHolderObject)
        {
            this.SetPos(Vector3.zero); // This will need to be set to the vertically offset(?) center of the flag holder gameobject for each team. I also need to make sure the flag is distinguishable by teamID.
            this.hidden = false;
            this.SetRot(0f);
            
        }

        public override void SetPos(Vector3 position)
        {
            this.GiveFlagToPlayer(-1);
            base.SetPos(position);

        }

        public void GiveFlagToPlayer(int playerID)
        {
            if (this.View.IsMine && !this.hidden) { this.View.RPC(nameof(RPCA_GiveFlagToPlayer), RpcTarget.All, playerID); }
        }

        protected internal override void OnCollisionEnter2D(Collision2D collision2D)
        {
            int? playerID = collision2D?.collider?.GetComponent<Player>()?.playerID;
            if (playerID != null)
            {
                this.GiveFlagToPlayer((int)playerID);
            }
            base.OnCollisionEnter2D(collision2D);
        }
        protected internal override void OnTriggerEnter2D(Collider2D collider2D)
        {
            int? playerID = collider2D?.GetComponent<Player>()?.playerID;
            if (playerID != null && PlayerManager.instance.CanSeePlayer(this.transform.position, PlayerManager.instance.players.Find(p => p.playerID == playerID)).canSee)
            {
                this.GiveFlagToPlayer((int)playerID);
            }
            base.OnTriggerEnter2D(collider2D);
        }
        protected override void Update()
        {
            if (this.transform.parent == null)
            {
                this.transform.position = 100000f * Vector2.up;
                this.currentFlagHolder = -1;
                return;
            }
            
            
            base.Update();

            if (this.currentFlagHolder != -1 || this.hidden)
            {
                this.SetRot(0f);
                this.Col.enabled = false;
                this.Trig.enabled = false;
                if (this.hidden)
                { 
                    this.SetPos(100000f * Vector2.up); 
                }

            }
            else
            {
                this.Col.enabled = true;
                this.Trig.enabled = true;

            }
        }
        void LateUpdate()
        {
            if (this.currentFlagHolder == -1)
            {
                return;
            }
            Vector3 position = Vector3.zero;
            /// Big Note 
            /// So I remember
            /// to look here.
            /// I'm assuming this is where the flag is moved/animated from point a to point b. 
            /// I need to figure out how to interp from last synced position to current synced position. 
            /// maybe this is why pykess was tracking previous crown holder.
            /// maybe ask mr.pykess. 
            /// maybe he read this right now.
            /// hi :^)
            base.transform.position = position;
        }

        [PunRPC]
        private void RPCA_GiveFlagToPlayer(int playerID)
        {
            this.currentFlagHolder = playerID;
            if (this.currentFlagHolder != -1 && !this.hidden) { base.StartCoroutine(this.IGiveFlagToPlayer()); }
        }
        private IEnumerator IGiveFlagToPlayer()
        {
            for (float i = 0f; i < this.transitionCurve.keys[this.transitionCurve.keys.Length - 1].time; i += Time.unscaledDeltaTime)
            {
                this.flagPos = Mathf.LerpUnclamped(0f, 1f, this.transitionCurve.Evaluate(i));
                yield return null;
            }
            yield break;
        }


        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantionData = info.photonView.InstantiationData;
            this.gameObject.transform.SetParent(GM_CaptureTheFlag.instance.transform);
            GM_CaptureTheFlag.instance.SetFlag(this);
            FlagHandler.instance = this;
        }

        protected override void ReadSyncedData()
        {
            throw new NotImplementedException();
        }

        protected override void SetDataToSync()
        {
            throw new NotImplementedException();
        }

        protected override bool SyncDataNow()
        {
            return !this.hidden;
        }

    }
}
