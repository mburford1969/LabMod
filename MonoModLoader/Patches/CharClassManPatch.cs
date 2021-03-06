﻿#pragma warning disable CS0626 // orig_ method is marked external and has no attributes on it.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MonoMod;
using RemoteAdmin;
using LabMod.Events;
using MEC;

namespace LabMod
{
	[MonoModPatch("global::CharacterClassManager")]
	class CharClassManPatch : CharacterClassManager
	{
		public class SpawnLateHelper
		{
			public CharacterClassManager ccm;
			public RoleType rt;
			public RoleType rtspawn;
		}

		public string AuthToken;

		[MonoModPublic]
		public string GetAuthToken() => AuthToken;

		public extern void orig_SetRandomRoles();
		public void SetRandomRoles()
		{
			bool stop = false;
			LabModPreRoundStart.TriggerEvent(this);
			LabModRoundStart.TriggerEvent(this, out stop);
			if (!stop)
				orig_SetRandomRoles();
			LabModPostRoundStart.TriggerEvent(this);
		}

		public extern void orig_Start();
		public void Start()
		{
			orig_Start();
			//not used, no nickname setup
			//LabModPlayerJoin.TriggerEvent(this);
		}

		public extern void orig_CallCmdRegisterEscape();
		public void CallCmdRegisterEscape()
		{
			bool stop = false;
			LabModPlayerEscape.TriggerEvent(this, out stop);
			if (!stop)
				orig_CallCmdRegisterEscape();
		}

		public extern IEnumerator<float> orig__LaterJoin();
		public IEnumerator<float> _LaterJoin()
		{
			bool stop = false;
			LabModJoinLate.TriggerEvent(this, out stop);
			if (!stop)
				Timing.RunCoroutine(orig__LaterJoin(), Segment.FixedUpdate);
			yield break;
		}

		public System.Collections.IEnumerator EndRoundSoon(float sec)
		{
			yield return new WaitForSeconds(sec);
			PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
		}

		public System.Collections.IEnumerator SpawnLate(SpawnLateHelper h)
		{
			yield return new WaitForSeconds(0.5f);
			h.ccm.SetClassID(h.rt);
			h.ccm.GetComponent<PlayerStats>().health = h.ccm.Classes.SafeGet(h.rt).maxHP;
			h.ccm.GetComponent<PlyMovementSync>().OnPlayerClassChange(SpawnpointManager.GetRandomPosition(h.rtspawn).transform.position, 0f);
		}
	}
}
