using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System.Linq;

public class SpecialAudio : MonoCacheMultiplayer
{
    [SerializeField] private float radius;

    private PhotonView PV;
    private static Dictionary<Player, SpecialAudio> specialAudioFromPlayers = new Dictionary<Player, SpecialAudio>();
    private IAudioEffectManager agoraAudioEffects;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        specialAudioFromPlayers[PV.Owner] = this;
        agoraAudioEffects = VoiceChatManager.Instance.GetRtcEngine().GetAudioEffectManager();
    }

    private void OnDestroy()
    {
        foreach (var item in specialAudioFromPlayers.Where(x => x.Value == this).ToList()) 
        {
            specialAudioFromPlayers.Remove(PV.Owner);
        }
    }

    public void Update()
    //public override void OnTick()
    {
        if (!PV.IsMine) 
        {
            return;
        }
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        { 
            if (player.IsLocal) continue;
            if (player.CustomProperties.TryGetValue("agoraID", out object agoraID))
            {
                if (specialAudioFromPlayers.ContainsKey(player))
                {
                    SpecialAudio other = specialAudioFromPlayers[player];

                    float gain = GetGain(other.transform.position);
                    float pan = GetPan(other.transform.position);

                    agoraAudioEffects.SetRemoteVoicePosition(uint.Parse((string)agoraID), pan, gain);
                }
                else
                {
                     agoraAudioEffects.SetRemoteVoicePosition(uint.Parse((string)agoraID), 0, 0);
                }
            }

        }
    }

    private float GetGain(Vector3 otherPosition) 
    {
        float distance = Vector3.Distance(transform.position, otherPosition);
        float gain = Mathf.Max(1-distance / radius)* 100f;
        return gain;
    }

    private float GetPan(Vector3 otherPosition) 
    {
        Vector3 direction = otherPosition - transform.position;
        direction.Normalize();
        float doProduct = Vector3.Dot(transform.right, direction);
        return doProduct;
    }
}
