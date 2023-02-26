using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using Mirror;

public class voiceTest : NetworkBehaviour
{
    
    AudioClip voice;
    bool isRecording = false;
    [SerializeField]
    private AudioSource playerVoice;
    /* slider for volume from 1 to 100 */
    [SerializeField]
    private float volume = 50f;


    void Update()
    {
        if(!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.V))
        {
            _ = RecordFor1Second();
        }
        if (Input.GetKeyUp(KeyCode.V))
        {
            isRecording = false;
        }
    }

    async Task RecordFor1Second()
    {
        isRecording = true;
        try
        {
            if (Microphone.devices.Length < 1)
            {
                Debug.LogError("No microphone found");
                isRecording = false;
                return;
            }
            while (isRecording) {

                string micName = Microphone.devices[0];
                
                voice = Microphone.Start(micName, true, 1, 22050);

                playerVoice.clip = voice;
                playerVoice.Play();
                while (playerVoice.isPlaying)
                {
                    await Task.Yield();
                }

                float[] samples = new float[voice.samples];
                voice.GetData(samples, 0);

                CmdSendSamples(samples, connectionToClient);
            }

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private void CmdSendSamples(float[] samples, NetworkConnectionToClient sender = null)
    {
        RpcSendSamples(samples, sender.identity);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSendSamples(float[] samples, NetworkIdentity player)
    {
        AudioSource source = player.GetComponent<AudioSource>();
        source.clip = AudioClip.Create("Voice", samples.Length, 1, 22050, false);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= volume / 10f;
        }
        source.clip.SetData(samples, 0);
        source.Play();
    }
}
