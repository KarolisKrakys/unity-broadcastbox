This Unity project demonstrates how to set up a WebRTC video stream as the background in Unity. It connects to a Broadcast Box using the WHEP endpoint to receive a live video stream.

## 1. Scene Setup

1. **Add a Plane (or Quad) for Video Display**  
   - **Right-click** in **Hierarchy → 3D Object → Quad**.  
   - **Rename** it to **VideoScreen** 

2. **Set Material for Video Screen**  
   - Create a **Material** (e.g., **VideoMaterial**).  
   - Set its **Shader** to **Unlit → Texture**.  
   - Assign it to the **VideoScreen’s Renderer → Material**.  

3. **Main Camera Settings**  
   - Select the **Main Camera** in the **Hierarchy**.  
   - **Clear Flags** → **Solid Color** or **Depth Only**.  
   - **Background Color** → **Black** (`0,0,0,0`).  
   - **Culling Mask** → Ensure only the layer containing the **VideoScreen** is enabled.

---

### Create a C# Script
Create a script named **WHEPReceiver.cs** and attach it to the **Main Camera** or an **Empty GameObject**.

### Input Values You Need to Replace
- **`whepUrl`**: The URL of your Broadcast Box WHEP endpoint (e.g., `https://yourserver.com/api/whep`).
- **`streamKey`**: The Bearer token or stream key required to authorize access.
- **`planeRenderer`**: Assign the **Renderer** component of the **VideoScreen** in the **Inspector**.
