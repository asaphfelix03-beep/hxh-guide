#!/bin/bash
# Génère les portraits originaux des personnages (illustrations simplifiées dessinées pour ce projet).
# Sortie : wwwroot/images/personnages/{identifiant}.svg
# Usage : ./deploy/portraits/generate.sh
set -euo pipefail
cd "$(dirname "$0")/../.."
OUT=wwwroot/images/personnages
mkdir -p "$OUT"

# ---------- Formes de cheveux (repère 400 x 400, tête centrée en 200,190) ----------
# Derrière la tête
BACK_LONG='M110 200 C 108 118, 150 88, 200 88 C 250 88, 292 118, 290 200 L 300 372 L 100 372 Z'
BACK_BOB='M106 252 C 96 160, 120 90, 200 88 C 280 90, 304 160, 294 252 Z'
BACK_MANE='M90 260 C 60 200, 80 110, 130 80 C 150 40, 250 40, 270 80 C 320 110, 340 200, 310 260 C 330 300, 280 320, 270 300 L 130 300 C 120 320, 70 300, 90 260 Z'
BACK_SPIKES='M150 110 L 120 40 L 180 92 L 200 25 L 220 92 L 280 40 L 250 110 Z'
BACK_CURLY='M108 230 C 80 200, 92 150, 112 130 C 100 90, 150 70, 175 82 C 190 55, 240 60, 250 85 C 285 80, 305 120, 292 145 C 315 175, 305 220, 292 235 Z'
BACK_PIGTAILS='M70 230 C 40 260, 50 330, 90 340 C 110 300, 112 260, 108 230 Z M330 230 C 360 260, 350 330, 310 340 C 290 300, 288 260, 292 230 Z'

# Devant (frange, mèches)
FRONT_SPIKY='M112 175 L 96 122 L 124 130 L 108 58 L 150 106 L 148 26 L 186 96 L 205 14 L 226 96 L 260 30 L 256 106 L 298 60 L 280 130 L 306 122 L 290 176 C 270 122, 232 112, 200 114 C 168 112, 130 122, 112 175 Z'
FRONT_MESSY='M110 188 C 92 140, 108 92, 140 78 C 150 46, 192 42, 206 60 C 228 38, 268 54, 270 84 C 304 98, 312 150, 292 188 C 282 152, 262 142, 252 154 C 246 132, 226 126, 216 142 C 206 126, 180 126, 172 146 C 160 130, 140 140, 136 162 C 124 150, 114 166, 110 188 Z'
FRONT_BOB='M114 200 C 110 128, 146 96, 200 96 C 254 96, 290 128, 286 200 C 276 160, 262 146, 250 150 L 236 128 L 224 156 L 206 126 L 192 156 L 176 128 L 162 152 C 146 146, 126 160, 114 200 Z'
FRONT_SHORT='M118 172 C 116 112, 158 90, 200 90 C 246 90, 284 112, 282 172 C 268 136, 242 124, 200 124 C 160 124, 134 136, 118 172 Z'
FRONT_SLICK='M120 172 C 116 118, 152 94, 200 94 C 250 94, 286 116, 282 172 C 270 140, 248 126, 200 126 C 152 126, 132 140, 120 172 Z'
FRONT_BANGS='M116 168 C 116 116, 150 96, 200 96 C 250 96, 284 116, 284 168 L 270 160 L 256 172 L 240 158 L 224 172 L 208 158 L 192 172 L 176 158 L 160 172 L 144 158 L 130 172 Z'
FRONT_PART='M118 172 C 114 116, 156 90, 206 93 C 256 96, 286 124, 282 172 C 262 130, 226 116, 186 124 C 160 130, 136 146, 118 172 Z'
FRONT_POMPADOUR='M118 168 C 108 120, 130 60, 196 52 C 262 46, 300 90, 282 168 C 268 130, 240 118, 200 120 C 160 120, 134 132, 118 168 Z'
FRONT_CURLY='M114 176 C 100 140, 118 104, 146 102 C 150 80, 184 74, 198 92 C 214 72, 252 80, 254 104 C 284 106, 298 142, 286 176 C 274 150, 260 146, 248 156 C 238 138, 218 136, 208 150 C 196 136, 176 138, 168 154 C 156 140, 136 146, 132 164 C 124 158, 116 166, 114 176 Z'
FRONT_TOPKNOT='M176 92 C 176 70, 224 70, 224 92 C 224 108, 176 108, 176 92 Z'
NONE=''

# ---------- Détails ----------
earring() { echo "<path d='M276 232 q -8 14 0 24 q 8 -10 0 -24 Z' fill='#d6336c'/>"; }
glasses() { echo "<g fill='#1c2440' fill-opacity='.85' stroke='#0b1020' stroke-width='5'><circle cx='170' cy='202' r='22'/><circle cx='230' cy='202' r='22'/></g><path d='M192 200 h 16' stroke='#0b1020' stroke-width='5'/>"; }
hisoka() { echo "<path d='M166 236 l 4 9 l 10 1 l -7 7 l 2 10 l -9 -5 l -9 5 l 2 -10 l -7 -7 l 10 -1 Z' fill='#7048e8'/><path d='M234 228 q -7 12 0 20 q 7 -8 0 -20 Z' fill='#1971c2'/>"; }
cross() { echo "<path d='M200 132 v 28 M190 142 h 20' stroke='#1c2440' stroke-width='6' stroke-linecap='round'/>"; }
beard() { echo "<path d='M140 236 C 150 300, 175 340, 200 352 C 225 340, 250 300, 260 236 C 240 262, 220 268, 200 268 C 180 268, 160 262, 140 236 Z' fill='#f1f3f5' stroke='#ced4da' stroke-width='2'/><path d='M150 172 q 22 -10 40 2 M210 174 q 18 -12 40 -2' stroke='#f1f3f5' stroke-width='8' stroke-linecap='round' fill='none'/>"; }
pipe() { echo "<path d='M214 244 l 38 12' stroke='#5c3d2e' stroke-width='7' stroke-linecap='round'/><rect x='248' y='236' width='20' height='26' rx='5' fill='#7f5539'/><path d='M262 228 c 10 -14 -6 -22 4 -36' stroke='#adb5bd' stroke-width='4' fill='none' stroke-linecap='round' opacity='.8'/>"; }
cap() { echo "<path d='M112 150 C 118 92, 160 70, 202 70 C 246 70, 286 94, 290 150 Z' fill='#495057'/><path d='M100 150 h 205 q -5 14 -20 14 h -170 q -15 0 -15 -14 Z' fill='#343a40'/>"; }
catears() { echo "<path d='M120 120 L 112 52 L 168 96 Z M280 120 L 288 52 L 232 96 Z' fill='#f1f3f5' stroke='#ced4da' stroke-width='3'/><path d='M126 108 L 122 70 L 152 95 Z M274 108 L 278 70 L 248 95 Z' fill='#f783ac'/>"; }
antennae() { echo "<path d='M168 106 C 150 60, 120 50, 102 64 M232 106 C 250 60, 280 50, 298 64' stroke='#2b8a3e' stroke-width='7' fill='none' stroke-linecap='round'/><path d='M140 140 C 160 120, 240 120, 260 140' stroke='#5c940d' stroke-width='6' fill='none'/>"; }
headband() { echo "<path d='M114 150 C 140 128, 260 128, 286 150 L 286 172 C 260 152, 140 152, 114 172 Z' fill='#e9ecef' stroke='#adb5bd' stroke-width='2'/><path d='M286 160 l 30 18 l -8 10 Z' fill='#e9ecef'/>"; }
collar_fur() { echo "<path d='M96 372 C 110 320, 150 300, 170 312 C 160 330, 150 352, 150 372 Z M304 372 C 290 320, 250 300, 230 312 C 240 330, 250 352, 250 372 Z' fill='#f8f9fa'/>"; }
tabard() { echo "<path d='M168 312 L 232 312 L 248 400 L 152 400 Z' fill='#1864ab'/><path d='M200 316 v 84' stroke='#fab005' stroke-width='5'/>"; }

# ---------- Portrait ----------
# portrait slug fond peau vêtement yeux cheveux arrière avant [yeux fermés:0/1] [détails...]
portrait() {
  local slug=$1 bg=$2 skin=$3 cloth=$4 eye=$5 hair=$6 back=$7 front=$8 closed=$9
  shift 9
  local extra="$*"
  local eyes
  if [ "$closed" = 1 ]; then
    eyes="<path d='M156 204 q 14 10 28 0 M216 204 q 14 10 28 0' stroke='#343a40' stroke-width='5' fill='none' stroke-linecap='round'/>"
  else
    eyes="<g><ellipse cx='170' cy='204' rx='12' ry='15' fill='$eye'/><ellipse cx='230' cy='204' rx='12' ry='15' fill='$eye'/><circle cx='174' cy='198' r='4' fill='#fff'/><circle cx='234' cy='198' r='4' fill='#fff'/></g><path d='M154 180 q 16 -8 32 0 M214 180 q 16 -8 32 0' stroke='#343a40' stroke-width='5' fill='none' stroke-linecap='round'/>"
  fi
  cat > "$OUT/$slug.svg" <<SVG
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 400" role="img" aria-label="Illustration originale">
<rect width="400" height="400" fill="$bg"/>
<circle cx="200" cy="200" r="170" fill="#fff" opacity=".18"/>
<path d="$back" fill="$hair"/>
<path d="M78 400 C 88 326, 148 302, 200 302 C 252 302, 312 326, 322 400 Z" fill="$cloth"/>
<rect x="178" y="248" width="44" height="66" rx="14" fill="$skin"/>
<rect x="178" y="248" width="44" height="30" rx="10" fill="#000" opacity=".08"/>
<ellipse cx="124" cy="206" rx="13" ry="20" fill="$skin"/>
<ellipse cx="276" cy="206" rx="13" ry="20" fill="$skin"/>
<ellipse cx="200" cy="192" rx="78" ry="92" fill="$skin"/>
<ellipse cx="160" cy="232" rx="14" ry="8" fill="#ff8787" opacity=".25"/>
<ellipse cx="240" cy="232" rx="14" ry="8" fill="#ff8787" opacity=".25"/>
$eyes
<path d="M200 214 q -5 16 4 18" stroke="#000" stroke-opacity=".2" stroke-width="4" fill="none" stroke-linecap="round"/>
<path d="M184 252 q 16 10 32 0" stroke="#7a3b3b" stroke-width="5" fill="none" stroke-linecap="round"/>
<path d="$front" fill="$hair"/>
$extra
</svg>
SVG
}

SKIN='#ffe0c7'; TAN='#e8b48f'; PALE='#fff1e6'
portrait gon-freecss       '#d3f9d8' $SKIN '#2b8a3e' '#5c3d2e' '#1b1f22' "$NONE"          "$FRONT_SPIKY"     0
portrait killua-zoldyck    '#d0ebff' $PALE '#364fc7' '#4dabf7' '#e9ecef' "$NONE"          "$FRONT_MESSY"     0
portrait kurapika          '#c5f6fa' $PALE '#f8f9fa' '#5c6784' '#f2c94c' "$BACK_BOB"      "$FRONT_BOB"       0 "$(tabard)" "$(earring)"
portrait leorio            '#ffe8cc' $TAN  '#212529' '#3b2f2f' '#212529' "$NONE"          "$FRONT_SHORT"     0 "$(glasses)"
portrait hisoka            '#f3d9fa' $PALE '#e5dbff' '#f59f00' '#e03131' "$BACK_SPIKES"   "$FRONT_SLICK"     0 "$(hisoka)"
portrait illumi-zoldyck    '#e5dbff' $PALE '#2b3a2e' '#111' '#111827' "$BACK_LONG"        "$FRONT_BANGS"     0
portrait isaac-netero      '#d3f9d8' $SKIN '#f8f9fa' '#495057' '#e9ecef' "$NONE"          "$FRONT_TOPKNOT"   1 "$(beard)"
portrait kite              '#c5f6fa' $SKIN '#5c940d' '#343a40' '#f1f3f5' "$BACK_LONG"     "$FRONT_BANGS"     0 "$(cap)"
portrait chrollo-lucilfer  '#ffdeeb' $PALE '#212529' '#111' '#111827' "$NONE"             "$FRONT_SLICK"     0 "$(cross)" "$(collar_fur)" "$(earring)"
portrait uvogin            '#d3f9d8' $TAN  '#7f5539' '#3b2f2f' '#6b4423' "$BACK_MANE"     "$FRONT_MESSY"     0
portrait shalnark          '#e5dbff' $SKIN '#e9d8a6' '#2f9e44' '#f2c94c' "$NONE"          "$FRONT_PART"      0
portrait biscuit-krueger   '#d0ebff' $PALE '#f783ac' '#e64980' '#f2c94c' "$BACK_PIGTAILS" "$FRONT_BOB"       0
portrait razor             '#ffe8cc' $TAN  '#e8590c' '#212529' '#1b1f22' "$BACK_LONG"     "$FRONT_SLICK"     0
portrait knuckle-bine      '#ffe8cc' $SKIN '#343a40' '#212529' '#1b1f22' "$NONE"          "$FRONT_POMPADOUR" 0
portrait morel             '#e5dbff' $TAN  '#f8f9fa' '#212529' '#adb5bd' "$NONE"          "$FRONT_SLICK"     0 "$(pipe)"
portrait neferpitou        '#ffdeeb' $PALE '#5f3dc4' '#e03131' '#f8f9fa' "$BACK_CURLY"    "$FRONT_CURLY"     0 "$(catears)"
portrait meruem            '#e5dbff' '#b2c8a8' '#2f4f3a' '#495057' '#b2c8a8' "$NONE"      "$NONE"            0 "$(antennae)"
portrait komugi            '#ffdeeb' $SKIN '#f8f9fa' '#343a40' '#2f4f3a' "$BACK_BOB"      "$FRONT_BOB"       1
portrait ging-freecss      '#ffdeeb' $TAN  '#c9a227' '#3b2f2f' '#1b1f22' "$NONE"          "$FRONT_MESSY"     0 "$(headband)"
portrait alluka-zoldyck    '#ffdeeb' $PALE '#f8f9fa' '#111' '#111827' "$BACK_LONG"        "$FRONT_BANGS"     0
portrait pariston-hill     '#ffe8cc' $SKIN '#868e96' '#1971c2' '#f2c94c' "$NONE"          "$FRONT_PART"      0

echo "$(ls "$OUT"/*.svg | wc -l) portraits générés dans $OUT"
