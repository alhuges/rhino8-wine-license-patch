# rhino8-wine-license-patch

**A Windows Rhino 8 crack, ported to Linux / Wine.**

Original DLLs, zero patches, nothing modified inside Rhino. After install, Rhino launches
straight to the main window with `Untitled - Rhinoceros 8 Commercial` in the title bar — no
"Welcome to Rhino — Enter your email" wizard, no "License Not Found" dialog, no network needed.

🌐 English · [简体中文](README.zh-CN.md)

---

## What it is

McNeel's license files (3 `.lic` certificates) are genuinely signed by McNeel. The files are
AES-256-CBC encrypted XML, and the password is **the volume serial number of the C: drive**:

```
password = GetVolumeInformationW("C:") serial, formatted as ToString("X").ToUpper()
(no leading zeros — `vol` shows 0A79-59B4, the actual password is A7959B4)
```

The original crack package was made for one specific Windows machine (serial `A7959B4`).
On any other machine the serial differs → the certificates cannot be decrypted → the wizard shows up.

This package does one thing: **re-encrypts the certificates with your machine's volume serial**,
places them in the right folder, and blocks online validation. That's it.

---

## Quick start

Prerequisites:
- Linux + **Wine ≥ 11.14** (staging/devel branch — not stable 11.0, or Rhino dies with a stack
  overflow on launch). Patched Proton builds (e.g. CachyOS) also work.
- Rhino 8 installed into that prefix with the **official Windows installer**
  (it brings its own .NET 8 Desktop Runtime / VC++ / WebView2).

```bash
tar xjf rhino8-wine-license-patch.tar.bz2
cd rhino8-wine-license-patch
export WINEPREFIX=~/.wine          # 1) your Rhino Wine prefix (adjust path)
./install.sh                       # 2) run the patch (asks for Rhino folder if needed)
```

Or fully non-interactive (no prompts at all — you don't need `export` when you pass `--prefix`):

```bash
./install.sh --prefix ~/.wine \
             --rhino ~/.wine/drive_c/"Program Files/Rhino 8"
```

Launch:

```bash
export WINEPREFIX=~/.wine          # keep it exported, or prefix the command
wine ~/.wine/drive_c/"Program Files/Rhino 8/System/Rhino.exe"
# equivalent one-liner:
# WINEPREFIX=~/.wine wine ~/.wine/drive_c/"Program Files/Rhino 8/System/Rhino.exe"
```

Expected result: main window directly, title `Untitled - Rhinoceros 8 Commercial`, no wizard.

Troubleshooting checkup:

```bash
./check.sh --prefix ~/.wine --rhino ~/.wine/drive_c/"Program Files/Rhino 8"
```

`check.sh` runs 10 checks (volume serial / cert decryptability / folder layout / hosts /
wine version / Avalon key / wizard window) and prints pass/fail per item.

---

## What install.sh does

1. **Probes your Wine C: volume serial** (`probe/vols.exe`, run inside Wine)
2. **Picks certificates**
   - serial = `43000000` (almost every Wine prefix) → uses the pre-encrypted certificates in this package
   - any other serial (PlayOnLinux/Bottles/custom) → re-encrypts `certs-win/` on the fly with
     `probe/liccrypt` (requires `dotnet` SDK ≥ 8, one-time)
3. **Certificate layout (the critical part)**: the 3 `.lic` files go **only** into
   `drive_c/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses/`
   — and every other folder (5.0/7.0/8.0/roaming) is **wiped**. Spreading copies into multiple
   folders actually *triggers* the wizard (see "Why 6.0 only" below).
4. **Cleans dirty state**: deletes `cloudzoo.json`, `00000000-*` and similar leftovers
5. **Blocks `api.mcneel.com` in BOTH the prefix hosts file and the system `/etc/hosts`**
   (field-tested: prefix-only is NOT enough — see below)
6. **Enables WPF software rendering**: `HKCU\Software\Microsoft\Avalon.Graphics\DisableHWAcceleration=1`
   (Rhino's wizard/Eto dialogs are WPF; under Wine/Proton their GPU path black-screens or
   white-screens. This does not affect Rhino viewport GPU rendering.)
7. **Self-verifies**: decrypts one certificate with the probed serial and confirms the plaintext
   is `<ZooLicense` XML

Auto-discovery: if `--rhino` is not given, it searches common install locations — plain Wine
prefixes, Proton `compatdata/*/pfx`, proton-games shared prefixes, Lutris and Bottles runners.

Nothing is modified inside the Rhino installation itself: no DLL patching, no IL hacks, no services.

---

## Why 6.0 only

Rhino 8's `ZooClientSettings.LicenseFolder()` (verified by decompiling 8.30) returns a
**hard-coded** path:

```
<CommonAppData or ApplicationData>\McNeel\Rhinoceros\6.0\License Manager\Licenses
```

Yes, 8.x still reads `6.0` (legacy constant). The Windows crack scene keeps its certificates
in exactly that folder, too.

If you also drop copies into 7.0/8.0/roaming folders, `LoadAllLicensesByFileDate` picks among
duplicates of the same GUID by file mtime and rewrites them — which drags Rhino onto the
cloud/wizard path. Verified empirically: certificates everywhere = wizard;
certificates only in 6.0 ProgramData = straight into the app. So **install.sh enforces the
6.0-only layout.**

---

## Why the system /etc/hosts matters (field-tested)

Blocking `api.mcneel.com` only inside the prefix's
`drive_c/windows/system32/drivers/etc/hosts` is **not sufficient**: Wine/Proton DNS resolution
falls through to the Linux system layer (nss / system hosts), so Rhino can still resolve and
reach the real server → online validation gets answered → the crack stops working.
`install.sh` therefore writes **both** files (idempotent, `# rhino8-wine-license-patch` marker in
`/etc/hosts`). Without write permission and without passwordless sudo, it prints the exact
line to add manually:

```
127.0.0.1 api.mcneel.com
```

---

## About 127.0.0.1:1717 (FAQ)

**Not needed. No bridges, no services.** 1717 is the OAuth loopback listener inside
`Proxy.dll` → `InternalRhinoAccountsManager` (`_redirectURIs = http://127.0.0.1:1717/ …`,
see `docs/Proxy.dll.decompiled.cs:781`). It is only used by the Rhino Accounts / CloudZoo
login flow. Our certificates are `NodeType=Standalone` and validated offline with the
`Mcneel.License` local algorithm. The Windows crack machine also had zero listeners on 1717
(verified with netstat).

---

## Package contents

```
rhino8-wine-license-patch/
├── install.sh          # deploy (interactive or flag-driven)
├── check.sh            # 10-item health check
├── *.lic × 3           # working certificates (password = Wine serial 43000000)
├── certs-win/          # original crack certificates (password = A7959B4), for re-encryption
├── probe/
│   ├── vols.exe        # Wine volume-serial probe (net48, prints serial=…)
│   ├── zdec.exe        # reflective certificate verifier (net48)
│   └── liccrypt(.dll)  # re-encryption tool (net8: reenc <from> <to> <src> <dst>)
├── src/                # full source of all three probes
├── README.md           # this file
├── README.zh-CN.md     # 中文文档
└── docs/               # forensics
    ├── Proxy.dll + Proxy.dll.decompiled.cs        # evidence for the 1717 OAuth listener
    └── ValidationClient.dll                       # stock 8.30 DLL (proof: zero patches)
```

> Full decompilations of the relevant types (`LicenseFolder()` constant, `LoadAllLicensesByFileDate`,
> wizard state machine — tens of thousands of lines) are published in the companion archive
> `rhino8-wine-license-patch-decomp` (see releases); the key snippets are quoted inline in this README.

---

## FAQ

**Still shows the "Welcome to Rhino" wizard?**
First make sure you're launching with the **same prefix you patched** (`echo $WINEPREFIX`, or
use the `WINEPREFIX=…` one-liner above — a mismatched prefix is a classic silent failure).
Then run `./check.sh`. 99% of the remaining cases are ① stray `.lic` files in another folder
(rerun install.sh — it wipes them) or ② volume serial mismatch (rerun install.sh; it will
re-encrypt on the fly if dotnet is available).

**Crashes instantly with a stack overflow?**
Wine < 11.14. Use wine-staging (Arch: `pacman -S wine-staging`; Debian/Ubuntu: WineHQ
devel/staging repos). Not stable 11.0.

**Black screen / rendering glitches (dialogs, wizard, viewport)?**
install.sh already enables WPF software rendering (Avalon `DisableHWAcceleration`). For the
3D viewport itself: Options → View → GPU → GPU Technology → OpenGL, then restart Rhino
(Nvidia / Wayland especially).

**Do I need the dotnet SDK?**
Only for on-the-fly re-encryption (serial ≠ 43000000, or explicit `--reencrypt`).
Standard Wine prefixes use the bundled certificates with zero dependencies.

**Bottles / Lutris?**
Works, as long as the runner's Wine is ≥ 11.14. Pass its prefix path to `--prefix`.

**Rhino updated (8.30 → 8.35)?**
Rerun `./install.sh`. The certificates are version-portable (same GUID `868c63f5…` =
LICENSE_GUID_THIS_VERSION across 8.2/8.30/8.35) and the layout logic is unchanged.

**Should I sign in to my McNeel account in Rhino?**
No — that triggers the CloudZoo flow and can overwrite the local certificate state
(and needs 1717). If you did: rerun `install.sh` (it clears `cloudzoo.json` and relayouts certs).

**Reinstall/repair on Windows?**
The same package works on Windows: the certificates are already in McNeel's own format,
and the wizard-free state comes from cert + hosts. Copy the matching-serial `.lic` files into
`%ProgramData%\McNeel\Rhinoceros\6.0\License Manager\Licenses` and block `api.mcneel.com`
in `C:\Windows\System32\drivers\etc\hosts`.

---

## Tested environments

| Host | Wine | Rhino | Result |
|---|---|---|---|
| CachyOS (Arch) | wine-staging 11.19 | 8.30.26103.11001 | ✅ wizard-free, Commercial |
| Fresh prefix (official 8.30 installer) | wine-staging 11.19 | 8.30.26103.11001 | ✅ same |

## Known harmless quirks

- While running, Rhino writes a status file to
  `…/Rhinoceros/6.0/License Manager/Licenses/cloudzoo.json` (install.sh clears it before each use).
- The certificate's `DateAdded` field gets refreshed (re-written) by Rhino on launch — content
  unchanged, normal write-back.
- Wine logs `fixme:uxtheme:GetImmersiveColor*` lines; harmless.

---

## Credits & disclaimer

- The crack itself (certificates + blocking scheme) is by its original authors
  (KG / Rhinoceros Universal License Patch). This package is a **port toolkit**: we reverse
  engineered the crypto (`PasswordDeriveBytes(serial)` + AES-256-CBC), re-encrypted the
  certificates, and replicated the 6.0-only layout, hosts blocking and WPF software rendering.
- All DLL claims were verified by byte-for-byte comparison between the Windows crack machine
  and the Wine/Proton side — **zero patched binaries**.
- Rhino® and Rhinoceros® are trademarks of Robert McNeel & Associates. This project is not
  affiliated with or endorsed by McNeel. Please support the official product;
  use this package for personal/educational environments.

## License

MIT (scripts and probe sources). The `.lic` files are binary artifacts carrying McNeel's
signatures and remain as shipped by the original crack.
# rhino8-wine-license-patch
