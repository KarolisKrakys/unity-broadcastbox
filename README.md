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


## 2. Script Setup (WHEP Receiver)
**Attach the Script** to any GameObject (e.g., the **Main Camera**).  
