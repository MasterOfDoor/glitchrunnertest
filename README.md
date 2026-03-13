## GlitchRunner

GlitchRunner is a 2D pixel‑art adventure platformer set **inside a compromised computer**, blending **Avalanche blockchain culture**, wallet login, and a surreal cyberspace story.
You play as a shrunken version of yourself, uploaded into the machine after a Trojan attack, fighting through CPU, RAM, GPU and TPM realms to reclaim your real body.

---

### 1. Project Title & Short Description

**GlitchRunner** is a Unity‑based 2D game where you:
- Log in with a **Web3 wallet** (Reown / WalletConnect AppKit).
- Traverse the internals of a infected PC, defeat Trojans living in CPU, RAM and GPU.
- Manipulate the **security wall’s 51%** to escape back to your flesh‑and‑bone body.

---

### 2. Core Features

- **Blockchain‑flavored cyber story**
  - A Trojan drags your character into the depths of the computer.
  - You step on a mysterious pin and get **uploaded as software** into the system.
  - To escape, you must manipulate **51% of the security wall** while fighting malware.

- **Avalanche‑integrated economy**
  - Wallet login via **Reown AppKit (WalletConnect)**.
  - Uses an Avalanche token (**GlitchRunnerCoin**) for in‑game economy and rewards.
  - Configurable via environment variables and a simple `/api/config` endpoint.

- **2D pixel‑art action platforming**
  - Side‑scrolling levels across **CPU Entry**, **CPU Bazaar**, **RAM**, **GPU**, and **TPM** layers.
  - Enemies themed as trojans, glitches, and corrupted processes.
  - A **Final Boss** fight at the TPM shard guarding the security wall.

- **Scene‑based flow**
  - `Login` → tutorial/teaching scene → **CPU Entry** → **CPU Bazaar** → **RAM** (and beyond).
  - Each scene advances the narrative of escaping the machine.

- **WebGL‑ready**
  - Designed to be deployed as **WebGL** on Vercel.
  - Configurable compression and build pipeline (see `DEPLOY.md`).

---

### 3. Installation & Getting Started

#### 3.1. Requirements

- **Unity**: 2021.3 or newer is recommended.
- **Reown AppKit / WalletConnect Unity package** for wallet login.
- A supported **Avalanche RPC endpoint** (Fuji testnet or mainnet).

#### 3.2. Cloning the Repository

```bash
git clone https://github.com/MasterOfDoor/GlitchRunner.git
cd GlitchRunner
```

#### 3.3. Opening the Project in Unity

1. Open Unity Hub.
2. Click **Add** and select the folder: `My project (1)`.
3. Alternatively, open the main scene directly (the `.unity` scene that starts from the Login screen).

#### 3.4. Local Environment Configuration (Optional)

For local development, you can use a `.env` file:

- Place `.env` in the **project root** (same level as `My project (1)`) or next to `Assets`.
- When missing, Reown + Avalanche configuration will fall back to defaults/empty values in the Editor.

Typical variables (mirroring what is expected in production):

```bash
REOWN_PROJECT_ID=your_reown_project_id
AVALANCHE_RPC_URL=https://api.avax-test.network/ext/bc/C/rpc
AVALANCHE_TOKEN_ADDRESS=0xYourGlitchRunnerCoin
AVALANCHE_TOKEN_DECIMALS=18
AVALANCHE_DISTRIBUTOR_ADDRESS=0xYourDistributorWallet
```

> **Note:** Keep private keys and secrets out of this repo. Use only public configuration values and environment variables.

#### 3.5. Running the Game in the Editor

1. Open the main scene (starting from the **Login** scene).
2. Press **Play** in the Unity Editor.
3. Log in with a supported wallet (if configured) or run with default editor values.
4. Progress through:
   - Login → Tutorial → CPU Entry → CPU Bazaar → RAM → GPU → TPM / Final Boss.

---

### 4. Usage & Gameplay

- **Login & Wallet**
  - Connect your wallet via the integrated Reown AppKit UI.
  - The game reads Avalanche configuration from environment variables or `/api/config`.

- **Core Gameplay Loop**
  - Navigate 2D pixel‑art levels.
  - Defeat trojans and corrupted entities hiding in CPU, RAM, and GPU.
  - Collect tokens and power‑ups themed around glitchy memory, corrupted sectors, and overclocked cores.
  - Progress toward the **TPM shard**, where the Final Boss guards the security wall.

- **Endgame**
  - Manipulate **51% of the security wall** to break free of the infected system.
  - If you succeed, your consciousness returns from pure software back into your physical body.

#### 4.1. Controls (Typical Defaults)

> These are example mappings; check the in‑game settings or Unity Input configuration for the exact bindings.

- **Move**: `A / D` or arrow keys  
- **Jump**: `Space`  
- **Attack / Interact**: `Left Mouse` or `E`  
- **Pause / Menu**: `Esc`

#### 4.2. WebGL / Vercel Deployment

To build and deploy a WebGL version (e.g., to Vercel):

1. In Unity:  
   - `File → Build Settings → WebGL → Switch Platform`  
   - Set **Compression Format = Disabled** (see `DEPLOY.md` for why).  
   - Build to `My project (1)/Builds/WEBGL`.
2. Copy the build output (`index.html`, `Build/`, `TemplateData/`) into the `webgl-deploy` folder of the repo.
3. Follow the detailed steps and environment variable table in **[`DEPLOY.md`](DEPLOY.md)**.

---

### 5. Tech Stack

- **Game Engine**
  - Unity (2D)

- **Languages**
  - C# (gameplay, UI, wallet logic)
  - ShaderLab / HLSL (effects, materials)
  - JavaScript / Node (for Vercel API config endpoint)

- **Blockchain & Web3**
  - Avalanche (Fuji or mainnet)
  - Reown AppKit / WalletConnect for wallet login
  - Custom Avalanche token: **GlitchRunnerCoin**

- **Infrastructure**
  - WebGL build
  - Vercel for static hosting + serverless `/api/config`

---

### 6. Contributing

Contributions, feedback, and ideas are very welcome — especially around:
- Level design and pacing in CPU / RAM / GPU / TPM areas.
- Balancing combat and difficulty against different trojan enemies.
- Expanding Avalanche integration and on‑chain interactions.

#### 6.1. How to Contribute

1. **Fork** the repository on GitHub.
2. Create a new branch for your feature or bugfix:

   ```bash
   git checkout -b feature/your-awesome-idea
   ```

3. Make your changes in Unity/C# (and scripts or configs as needed).
4. Test your changes in the Unity Editor (and WebGL if relevant).
5. Commit with a clear message and push your branch:

   ```bash
   git commit -m "Add: short description of your change"
   git push origin feature/your-awesome-idea
   ```

6. Open a **Pull Request** against the `main` branch, describing:
   - What you changed.
   - How to test it.
   - Any known issues or follow‑ups.

#### 6.2. Reporting Issues

If you find a bug, have a gameplay suggestion, or see something off with Avalanche integration:
- Open a **GitHub Issue** in this repository.
- Include:
  - A clear title and description.
  - Steps to reproduce (if it’s a bug).
  - Your platform (Editor version, OS, WebGL/desktop).
  - Screenshots or logs if helpful.

---

**GlitchRunner** aims to merge the feel of classic 2D pixel‑art platformers with the strange logic of modern blockchain‑driven machines.
Jack into the Trojan‑infected CPU, bend the 51% rule to your will, and fight your way back to reality.
