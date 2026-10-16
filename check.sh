#!/usr/bin/env bash
# rhino8-wine-license-patch 体检: 卷号 / 证书可解 / 布局(必须仅6.0) / hosts / 向导状态
set -uo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
PREFIX=""; RHINO=""
while [ $# -gt 0 ]; do case "$1" in
  --prefix) PREFIX="$2"; shift 2;; --rhino) RHINO="$2"; shift 2;;
  -h|--help) sed -n '2,4p' "$0"; exit 0;; *) PREFIX="$1"; shift;;
esac; done
PREFIX="${PREFIX:-${WINEPREFIX:-$HOME/.local/share/wineprefixes/rhino8}}"
if [ -z "$RHINO" ]; then
  # 自动搜索: prefix 内 + Proton/Lutris/Bottles 常见路径(find 兼容空格目录)
  for root in "$PREFIX/drive_c/Program Files" "$PREFIX/drive_c/Program Files (x86)" \
              "$HOME/.local/share/Steam/steamapps/compatdata"*/pfx/drive_c/Program Files \
              "$HOME/.local/share/proton-games/prefixes"*/pfx/drive_c/Program Files \
              "$HOME/.local/share/lutris/runes"*/wine/*/drive_c/Program Files \
              "$HOME/.var/app/com.usebottles.bottles/data/bottles/bottles"*/drive_c/Program Files; do
    c=$(find "$root" -maxdepth 3 -type f -name Rhino.exe -path "*/System/*" 2>/dev/null | head -1)
    [ -n "$c" ] && { RHINO="$(dirname "$(dirname "$c")")"; break; }
  done
fi
P=0; F=0; ok(){ echo "  [ok] $1"; P=$((P+1)); }; bad(){ echo "  [!!] $1"; F=$((F+1)); }
echo "== prefix: $PREFIX ; rhino: ${RHINO:-未找到}"
[ -d "$PREFIX/drive_c" ] && ok "prefix 存在" || { bad "无 drive_c"; exit 1; }
[ -n "$RHINO" ] && ok "Rhino 安装存在" || bad "没找到 Rhino 8 (--rhino 指定)"
VOL=$(WINEPREFIX="$PREFIX" WINEDEBUG=-all timeout 90 wine "$HERE/probe/vols.exe" 2>/dev/null | tr ' ' '\n' | grep -ao 'serial=[0-9A-F]*' | cut -d= -f2)
[ -n "${VOL:-}" ] && ok "卷号=$VOL" || bad "卷号探针失败(wine/.NET 异常?)"
D6="$PREFIX/drive_c/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses"
n=$(ls "$D6"/*.lic 2>/dev/null | wc -l); [ "$n" = 3 ] && ok "6.0 ProgramData 3 张证书" || bad "6.0 目录 $n 张(应 3)"
extra=$(find "$PREFIX/drive_c/ProgramData/McNeel/Rhinoceros" "$PREFIX/drive_c/users/$(id -un)/AppData/Roaming/McNeel/Rhinoceros" -name "*.lic" 2>/dev/null | grep -av "/6.0/License Manager/Licenses/" | head -1)
[ -z "$extra" ] && ok "其余目录无 .lic" || bad "多余证书(会触发向导): $extra"
cj=$(find "$PREFIX/drive_c" -name cloudzoo.json 2>/dev/null | head -1)
[ -z "$cj" ] && ok "无 cloudzoo.json" || { pgrep -x Rhino.exe >/dev/null && ok "cloudzoo.json 存在(Rhino 运行中正常现象)" || bad "cloudzoo.json 残留(启动前需清): $cj"; }
grep -qa api.mcneel "$PREFIX/drive_c/windows/system32/drivers/etc/hosts" 2>/dev/null && ok "prefix hosts 封了 api.mcneel.com" || bad "prefix hosts 未封"
grep -qa api.mcneel /etc/hosts 2>/dev/null && ok "系统 /etc/hosts 封了 api.mcneel.com (实测必须)" || bad "系统 /etc/hosts 未封! 破解会失效 — 加一行: 127.0.0.1 api.mcneel.com"
if [ -n "${VOL:-}" ]; then
  h=$(dotnet "$HERE/probe/liccrypt.dll" dec "$VOL" "$D6/868c63f5-3760-4a45-8600-5399cc57b47c.lic" 2>/dev/null | head -c 13)
  [ "$h" = "<ZooLicense x" ] && ok "证书可解(口令=$VOL)" || bad "证书以口令 $VOL 解不开 → ./install.sh --reencrypt"
fi
wine --version 2>/dev/null | grep -qaE 'wine-(11\.1[4-9]|11\.[2-9][0-9]|[2-9][0-9])' && ok "wine ≥ 11.14" || bad "wine 过低(<11.14 → stack overflow)"
if pgrep -x Rhino.exe >/dev/null 2>&1; then WZ=$(DISPLAY=:0 xdotool search --class wine_desktop_window 2>/dev/null | while read -r w; do DISPLAY=:0 xdotool getwindowname "$w" 2>/dev/null; done | grep -aci "^Licensing"); [ "${WZ:-0}" = 0 ] && ok "当前无 Licensing 向导窗口" || bad "Licensing 向导窗口存在"; fi
AV=$(wine reg query 'HKCU\Software\Microsoft\Avalon.Graphics' //v DisableHWAcceleration 2>/dev/null | grep -a "0x1")
[ -n "$AV" ] && ok "WPF 软件渲染已开(Avalon.DisableHWAcceleration=1)" || bad "WPF 软件渲染未开: wine reg add 'HKCU\Software\Microsoft\Avalon.Graphics' /v DisableHWAcceleration /t REG_DWORD /d 1 /f"
echo; echo "结果: $P 通过 / $F 问题"; [ "$F" = 0 ]
