# rhino8-wine-license-patch — Windows 版 Rhino 8 破解 → Wine (Linux) 移植包

🌐 [English](README.md) · 简体中文

在任意 Linux 的 Wine prefix 里，让 **Windows 安装的 Rhino 8（原厂 DLL，零补丁）** 免邮箱向导直接启动，
标题显示 `Untitled - Rhinoceros 8 Commercial`。

## 原理（要点）

1. 破解包里的 3 张 `.lic` 是 **McNeel 签名的真证书**。文件 = AES-256-CBC 加密的 XML，
   口令 = **本机 C: 卷号**（`GetVolumeInformationW` → `ToString("X").ToUpper()`，**无前导零**）。
   打包机卷号 `A7959B4`；标准 Wine 卷号恒为 `43000000`。
   本包默认证书已重加密为 `43000000`，普通 Wine prefix 即装即用。
2. 证书**只放一个目录**：`drive_c/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses/`。
   8.30 的 `ZooClientSettings.LicenseFolder()` 常量路径就是 `Rhinoceros\6.0\...`。
   **其它目录必须空**——多副本会触发按 mtime 择优回写逻辑，把 Rhino 带进云/向导路径。
3. `drivers/etc/hosts` 封 `127.0.0.1 api.mcneel.com`（与 Windows 破解现场一致）——
   **prefix 副本和系统 `/etc/hosts` 都要写**（实测只写 prefix 会失效：Proton/wine 的 DNS
   解析会落到系统层，Rhino 仍能连到真服务器）。
   在线验证被断 → Rhino 走离线证书校验（`Mcneel.License` 本地算法），全程不联网、不需要任何服务器应答。
4. **WPF 强制软件渲染**：`HKCU\Software\Microsoft\Avalon.Graphics\DisableHWAcceleration=1`。
   Rhino 向导/Eto 对话框是 WPF，在 Wine/Proton 的 d3d 加速链路上易黑屏/白屏；禁用后退回软件光栅化，
   界面稳定（不影响 Rhino 视口的 GPU 渲染）。

前提（不在本包职责内）：Wine ≥ 11.14（staging/devel，或带 uxtheme 修复的 Proton/CachyOS 构建），
且该 prefix 里已用 **Windows 官方安装器** 装好 Rhino 8（它自带 .NET 8 Desktop Runtime / VC++ / WebView2）。

## 用法

```bash
tar xjf rhino8-wine-license-patch.tar.bz2 && cd rhino8-wine-license-patch
export WINEPREFIX=~/.wine       # 1) 你的 Rhino 所在 Wine prefix（按需改路径）
./install.sh                    # 2) 部署（交互式；传了 --prefix 则完全非交互）

# 或一步到位（不 export 也行，直接传参）：
./install.sh --prefix ~/.wine --rhino "$HOME/.wine/drive_c/Program Files/Rhino 8"
```

install.sh 做的事：
- 探针读出该 prefix 的 C: 卷号；若 ≠ 43000000，自动用 `certs-win/` + dotnet 现场重加密（`--reencrypt` 可强制）；
- 证书仅放入 6.0 ProgramData，清空 5.0/7.0/8.0/roaming 里所有 `.lic`，删 `cloudzoo.json` 等脏状态；
- prefix hosts + **系统 `/etc/hosts`** 都封 `api.mcneel.com`（实测只封 prefix 会失效；系统 hosts 需要写权限，无权限时会打印要手工添加的那一行）；
- 写入 `HKCU\Software\Microsoft\Avalon.Graphics\DisableHWAcceleration=1`（WPF 软件渲染，防黑屏/白屏）；
- 自动搜索 Rhino 安装目录（覆盖系统 wine / Proton compatdata / proton-games / Lutris / Bottles 常见路径）；
- 用本机解密工具验证证书可解（得到 `<ZooLicense` XML 才算成功）。

启动：
```bash
export WINEPREFIX=~/.wine       # 保持导出；或直接前缀给单条命令
wine ~/.wine/drive_c/"Program Files/Rhino 8/System/Rhino.exe"
# 等价一行式： WINEPREFIX=~/.wine wine "<prefix>/drive_c/Program Files/Rhino 8/System/Rhino.exe"
```

排障：
```bash
./check.sh --prefix <prefix> --rhino <dir>   # 体检：卷号/可解性/布局/hosts/向导窗口
```

常见坑：
- **仍弹向导**：先确认启动用的 prefix 就是打补丁的那个（`echo $WINEPREFIX`，或用上面的 `WINEPREFIX=…` 一行式——prefix 搞错是经典的隐形失败）。再 `./check.sh`。剩下的 99% 是①其它目录混进了 `.lic`，或②卷号不符（重跑 install）。
- **stack overflow 秒退**：wine < 11.14。
- **别登录 McNeel 账号**（会走 CloudZoo 流程覆盖本地证书状态）。
- Windows 版 Rhino 升级/重装后，重跑一次 `install.sh` 即可（它会重新清布局）。
- 若同一 prefix 里有旧破解残留（改过的 DLL、假证书），先恢复原厂或换新 prefix。

## 关于 127.0.0.1:1717（常见疑问）

**本方案完全不需要 1717，不用起任何桥接/服务。**
1717 是 `Proxy.dll` 里 `InternalRhinoAccountsManager` 的 OAuth 回环监听端口
（`_redirectURIs = http://127.0.0.1:1717/ …`，见 `docs/Proxy.dll.decompiled.cs:781`），
**只有 Rhino Accounts / CloudZoo 云登录流程**才会开它。我们的证书是单机(Standalone)节点类型，
走离线校验，全程不联网、不登录、不开 1717。
`docs/` 里附了 `Proxy.dll` 与其反编译供取证；Windows 现场（work）也实测无任何 1717 监听。

## hosts 为什么必须写系统 /etc/hosts（实测教训）

只在 prefix 的 `drive_c/windows/system32/drivers/etc/hosts` 里封 `api.mcneel.com`
**不够**：Wine/Proton 的 DNS 解析最终会落到 Linux 系统层（nss/系统 hosts），
Rhino 仍可解析到真实服务器并连通 → 在线验证被服务器应答 → 破解失效、重新弹向导。
所以本包同时写 prefix hosts 和 `/etc/hosts`（两者都幂等、带 `# rhino8-wine-license-patch` 标记）。
若运行环境没有 `/etc/hosts` 写权限且无免密 sudo，`install.sh` 会打印需手工添加的那一行。

## 包内容

| 文件 | 说明 |
|---|---|
| `install.sh` | 部署（交互式/参数式；含 WPF 软件渲染注册表）|
| `check.sh` | 体检排障（含 Avalon 键检查）|
| `*.lic`（3 张）| 成品证书（口令 `43000000`）|
| `certs-win/` | 破解包原始证书（口令 `A7959B4`，重加密原料）|
| `probe/vols.exe` | Wine 卷号探针（net48，打印 `serial=`）|
| `probe/zdec.exe` | 反射证书验证器（net48，`IsValid=True` 判据）|
| `probe/liccrypt*` | 重加密工具（net8，`reenc <from> <to> <src> <dst>`）|
| `src/*.txt` | 上述三个工具的完整源码 |
| `docs/` | 取证：8.30 原厂 ValidationClient.dll + 关键反编译（LicenseFolder 常量、LoadAllLicenses、向导状态机）|

*Rhino® 为 Robert McNeel & Associates 商标；本包为个人学习环境下的逆向记录与移植，请支持正版。*
