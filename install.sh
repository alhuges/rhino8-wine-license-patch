#!/usr/bin/env bash
# ============================================================
# rhino8-wine-license-patch install — 把 Windows 破解(Rhino 8)移植到任意 Wine prefix
# 只处理破解三件套: 证书 / 6.0-only 布局 / hosts 屏蔽 / WPF 软件渲染。
# 前提: 该 prefix 里已用 Windows 官方安装器装好 Rhino 8。
# 用法:
#   ./install.sh                       交互式，问 prefix 和 Rhino 目录
#   ./install.sh --prefix <dir> [--rhino <dir>] [--reencrypt]
#     --reencrypt  强制按本机 Wine 卷号现场重加密，需 dotnet SDK
#                  卷号不等于 43000000 时自动启用
# 兼容: 系统 wine / wine-staging / Proton 垫片 / Lutris / Bottles
# ============================================================
set -uo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
PREFIX="${WINEPREFIX:-}"; RHINO=""; REENCRYPT=0
while [ $# -gt 0 ]; do
  case "$1" in
    --prefix) PREFIX="$2"; shift 2;;
    --rhino) RHINO="$2"; shift 2;;
    --reencrypt) REENCRYPT=1; shift;;
    -h|--help) sed -n '2,13p' "$0"; exit 0;;
    *) echo "未知参数 $1"; exit 1;;
  esac
done
echo "======== rhino8-wine-license-patch 部署 ========"
if [ -z "$PREFIX" ]; then
  d="$HOME/.local/share/wineprefixes/rhino8"; [ -d "$d/drive_c" ] || d="$HOME/.wine"
  [ -t 0 ] && read -rp "Wine prefix 路径 [$d]: " PREFIX; PREFIX="${PREFIX:-$d}"
fi
[ -d "$PREFIX/drive_c" ] || { echo "错误: $PREFIX 不是 Wine prefix，缺 drive_c"; exit 1; }
command -v wine >/dev/null || { echo "错误: 没装 wine。需 >=11.14 的 staging/devel，或带 uxtheme 修复的 Proton/CachyOS 构建，否则 Rhino 启动即 stack overflow"; exit 1; }
WV=$(wine --version 2>/dev/null); echo "wine: $WV / prefix: $PREFIX"
echo "$WV" | grep -qaE 'wine-(11\.1[4-9]|11\.[2-9][0-9]|[2-9][0-9])' || echo "[!] 若 wine < 11.14 且非 Proton/CachyOS 补丁构建, Rhino 会 stack overflow 秒退"
# 自动搜索 Rhino 安装目录：prefix 内 + Proton/Lutris/Bottles 常见路径；find 兼容空格目录
if [ -z "$RHINO" ]; then
  for root in "$PREFIX/drive_c/Program Files" "$PREFIX/drive_c/Program Files (x86)" \
              "$HOME/.local/share/Steam/steamapps/compatdata"*/pfx/drive_c/Program Files \
              "$HOME/.local/share/proton-games/prefixes"*/pfx/drive_c/Program Files \
              "$HOME/.local/share/lutris/runes"*/wine/*/drive_c/Program Files \
              "$HOME/.var/app/com.usebottles.bottles/data/bottles/bottles"*/drive_c/Program Files; do
    c=$(find "$root" -maxdepth 3 -type f -name Rhino.exe -path "*/System/*" 2>/dev/null | head -1)
    [ -n "$c" ] && { RHINO="$(dirname "$(dirname "$c")")"; break; }
  done
fi
if [ -z "$RHINO" ]; then
  d="$PREFIX/drive_c/Program Files/Rhino 8"
  [ -t 0 ] && read -rp "Rhino 8 安装目录，需包含 System/Rhino.exe [$d]: " RHINO; RHINO="${RHINO:-$d}"
fi
[ -f "$RHINO/System/Rhino.exe" ] || { echo "错误: 没有 $RHINO/System/Rhino.exe，先用官方 Windows 安装器在该 prefix 装 Rhino 8"; exit 1; }
echo "rhino: $RHINO"
# 新 prefix 兜底初始化（Proton 的 pfx 一般已 wineboot 过）
[ -f "$PREFIX/drive_c/windows/system.ini" ] || { echo "prefix 未初始化: wineboot..."; WINEPREFIX="$PREFIX" WINEDEBUG=-all wineboot >/dev/null 2>&1 || true; }
# 读取本机 Wine C: 卷号，即证书加密口令
VOL=$(WINEPREFIX="$PREFIX" WINEDEBUG=-all timeout 90 wine "$HERE/probe/vols.exe" 2>/dev/null | tr ' ' '\n' | grep -ao 'serial=[0-9A-F]*' | cut -d= -f2)
VOL="${VOL:-43000000}"
echo "本机 Wine C: 卷号，证书口令 = $VOL"
CERTS="$HERE"
NEED_REENC="$REENCRYPT"
[ "$VOL" != "43000000" ] && NEED_REENC=1
if [ "$NEED_REENC" = "1" ]; then
  command -v dotnet >/dev/null || { echo "错误: 需 dotnet SDK>=8 现场重加密，卷号=$VOL"; exit 1; }
  SRC="$HERE/certs-win"
  CERTS="$HERE/.reenc-$VOL"; mkdir -p "$CERTS"
  for f in "$SRC"/*.lic; do
    dotnet "$HERE/probe/liccrypt.dll" reenc A7959B4 "$VOL" "$f" "$CERTS/$(basename "$f")" >/dev/null || { echo "错误: 重加密失败 $f"; exit 1; }
  done
  echo "已用 certs-win 按本机卷号 $VOL 现场重加密 -> $CERTS"
else
  echo "卷号=43000000, 直接使用包内成品证书"
fi
# 证书布局（关键）：只放 6.0 ProgramData；其余目录必须清空
D6="$PREFIX/drive_c/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses"
mkdir -p "$D6"; rm -f "$D6"/*.lic 2>/dev/null
cp -f "$CERTS"/*.lic "$D6"/
for v in 5.0 6.0 7.0 8.0; do
  for base in "$PREFIX/drive_c/ProgramData/McNeel/Rhinoceros/$v/License Manager/Licenses" \
              "$PREFIX/drive_c/users/$(id -un)/AppData/Roaming/McNeel/Rhinoceros/$v/License Manager/Licenses"; do
    [ "$base" = "$D6" ] && continue
    mkdir -p "$base" 2>/dev/null || true; rm -f "$base"/*.lic 2>/dev/null || true
  done
done
rm -rf "$PREFIX/drive_c/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses.bak" 2>/dev/null || true
find "$PREFIX/drive_c" -name cloudzoo.json -delete 2>/dev/null || true
find "$PREFIX/drive_c" -name '00000000-0000*' -delete 2>/dev/null || true
echo "布局: 证书已仅放入 6.0/ProgramData, 其余目录已清空, cloudzoo 已清"
# hosts 封 api.mcneel.com：prefix 内 + 系统 /etc/hosts 都写。
# 实测教训：只写 prefix hosts 不够 —— Proton 环境下 wine 的 DNS 解析会落到
# 系统层（nss/系统 hosts），prefix 副本可能被无视，Rhino 仍能解析到真服务器 → 破解失效。
H="$PREFIX/drive_c/windows/system32/drivers/etc/hosts"
sed -i '/api\.mcneel/d' "$H" 2>/dev/null || true
printf '127.0.0.1 api.mcneel.com\n' >> "$H"
echo "hosts: prefix 内已封 api.mcneel.com"
if ! grep -qa api.mcneel /etc/hosts 2>/dev/null; then
  if [ -w /etc/hosts ]; then
    printf '# rhino8-wine-license-patch\n127.0.0.1 api.mcneel.com\n' >> /etc/hosts && echo "hosts: 系统 /etc/hosts 已封 api.mcneel.com"
  elif command -v sudo >/dev/null && sudo -n true 2>/dev/null; then
    echo '# rhino8-wine-license-patch
127.0.0.1 api.mcneel.com' | sudo tee -a /etc/hosts >/dev/null && echo "hosts: 系统 /etc/hosts 已封 api.mcneel.com (sudo)"
  else
    echo "[!] 需要手工把这一行加进系统 /etc/hosts（实测不写会失效）:"
    echo "      127.0.0.1 api.mcneel.com"
  fi
else
  echo "hosts: 系统 /etc/hosts 已有 api.mcneel 条目, 跳过"
fi
# WPF 强制软件渲染：绕过 Wine/Proton 的 d3d 加速链路（向导/Eto 界面均为 WPF，
# Proton 下易黑屏/白屏/渲染异常）。HKCU\Software\Microsoft\Avalon.Graphics
wine reg add 'HKEY_CURRENT_USER\Software\Microsoft\Avalon.Graphics' /v DisableHWAcceleration /t REG_DWORD /d 1 /f >/dev/null 2>&1 \
  && echo "注册表: Avalon.DisableHWAcceleration=1, WPF 软件渲染已开" \
  || echo "[!] 写入 HKCU Avalon 键失败, 检查 wine 可用; 手动: wine reg add 'HKCU\Software\Microsoft\Avalon.Graphics' /v DisableHWAcceleration /t REG_DWORD /d 1 /f"
# 自检：以本机卷号解密一张证书
if command -v dotnet >/dev/null; then
  VOUT=$(dotnet "$HERE/probe/liccrypt.dll" dec "$VOL" "$D6/868c63f5-3760-4a45-8600-5399cc57b47c.lic" 2>/dev/null | head -c 13)
  [ "$VOUT" = "<ZooLicense x" ] && echo "验证: 证书可解, 解密得到 ZooLicense XML" || echo "[!] 验证: 证书以口令 $VOL 解不开, 运行 ./check.sh"
else
  echo "验证: 跳过, 本机无 dotnet; 若启动仍弹向导, 运行 ./check.sh 排查"
fi
echo
echo "✅ 破解部署完成。启动:"
echo "   WINEPREFIX=\"$PREFIX\" wine \"$RHINO/System/Rhino.exe\""
echo "   期望: 直接进主界面, 标题 'Untitled - Rhinoceros 8 Commercial', 无邮箱向导"
echo "   排查: ./check.sh --prefix \"$PREFIX\" --rhino \"$RHINO\""
