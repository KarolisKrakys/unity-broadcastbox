using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;

public class WHEPReceiver : MonoBehaviour
{
    [Header("WHEP Configuration")]
    public string whepUrl = "https://YourWhepUrl";

    [Tooltip("Bearer token from your Broadcast Box or streaming server. ")]
    public string streamKey = "YourStreamKey";

    [Header("Video Display (3D Plane)")]
    [Tooltip("Assign the plane's MeshRenderer here")]
    public Renderer planeRenderer;

    // PeerConnection for receiving video
    private RTCPeerConnection peerConnection;
    private VideoStreamTrack remoteVideoTrack;
    private Texture2D videoTexture;

    private void Start()
    {
        var config = new RTCConfiguration
        {
            iceServers = new RTCIceServer[] { }
        };
        peerConnection = new RTCPeerConnection(ref config);

        peerConnection.OnIceConnectionChange = OnIceConnectionChange;
        peerConnection.OnIceCandidate = candidate => Debug.Log($"ICE Candidate: {candidate.Candidate}");
        peerConnection.OnTrack = OnTrackReceived;

        var videoTransceiver = peerConnection.AddTransceiver(TrackKind.Video);

        videoTransceiver.Direction = RTCRtpTransceiverDirection.RecvOnly;

        var allVideoCodecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;
        Debug.Log(allVideoCodecs);
        var h264Codecs = allVideoCodecs.Where(codec => codec.mimeType == "video/H264");
        if (h264Codecs.Any())
        {
            var error = videoTransceiver.SetCodecPreferences(h264Codecs.ToArray());
            if (error != RTCErrorType.None)
                Debug.LogError($"SetCodecPreferences failed: {error}");
            else
                Debug.Log("H.264 codec preference set successfully.");
        }
        else
        {
            Debug.LogWarning("No H.264 codecs found. The server might still negotiate VP8 or VP9 if available.");
        }

        StartCoroutine(CreateAndSendOffer());
        StartCoroutine(WebRTC.Update());

    }

    private IEnumerator CreateAndSendOffer()
    {
        var offerOp = peerConnection.CreateOffer();
        yield return offerOp;

        if (offerOp.IsError)
        {
            Debug.LogError($"CreateOffer Error: {offerOp.Error.message}");
            yield break;
        }

        var localDesc = offerOp.Desc;
        var setLocalOp = peerConnection.SetLocalDescription(ref localDesc);
        yield return setLocalOp;

        if (setLocalOp.IsError)
        {
            Debug.LogError($"SetLocalDescription Error: {setLocalOp.Error.message}");
            yield break;
        }

        Debug.Log($"Local SDP:\n{localDesc.sdp}");

        yield return StartCoroutine(PostSdpToWhep(localDesc.sdp));
    }

    private IEnumerator PostSdpToWhep(string localSdp)
    {
        using (var request = new UnityWebRequest(whepUrl, "POST"))
        {
            byte[] sdpBytes = Encoding.UTF8.GetBytes(localSdp);
            request.uploadHandler = new UploadHandlerRaw(sdpBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/sdp");
            request.SetRequestHeader("Authorization", $"Bearer {streamKey}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"WHEP POST Error: {request.error}");
                yield break;
            }

            string remoteSdp = request.downloadHandler.text;
            if (string.IsNullOrEmpty(remoteSdp))
            {
                Debug.LogError("Empty SDP from server. Check streamKey or server logs.");
                yield break;
            }

            Debug.Log($"Remote SDP:\n{remoteSdp}");
            var remoteDesc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = remoteSdp };

            var setRemoteOp = peerConnection.SetRemoteDescription(ref remoteDesc);
            yield return setRemoteOp;

            if (setRemoteOp.IsError)
            {
                Debug.LogError($"SetRemoteDescription Error: {setRemoteOp.Error.message}");
            }
            else
            {
                Debug.Log("Remote SDP set. Awaiting video frames...");
            }
        }
    }

    private void OnTrackReceived(RTCTrackEvent e)
    {
        Debug.Log($"OnTrackReceived: track kind = {e.Track.Kind}, ID = {e.Track.Id}");

        if (e.Track is VideoStreamTrack track)
        {
            Debug.Log("Received remote video track!");
            remoteVideoTrack = track;

            videoTexture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);

            track.OnVideoReceived += ptr =>
            {
                Debug.Log("Received frame!");
                if (videoTexture != null)
                {
                    videoTexture.UpdateExternalTexture(ptr.GetNativeTexturePtr());
                }

                if (planeRenderer != null)
                {
                    planeRenderer.material.mainTexture = videoTexture;
                }
            };
        }
    }

    private void OnIceConnectionChange(RTCIceConnectionState state)
    {
        Debug.Log($"ICE Connection State changed to: {state}");
    }

    private void OnDestroy()
    {
        // Cleanup
        if (remoteVideoTrack != null)
        {
            remoteVideoTrack.Dispose();
            remoteVideoTrack = null;
        }

        if (peerConnection != null)
        {
            peerConnection.Close();
            peerConnection.Dispose();
            peerConnection = null;
        }
    }
}
