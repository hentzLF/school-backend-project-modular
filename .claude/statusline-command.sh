#!/usr/bin/env bash
# Claude Code status line: context window + 5-hour rate limit usage
# Displays visual progress bars and percentages.

input=$(cat)

# --- helpers ---

make_bar() {
  local pct="${1:-0}"
  local width=10
  local filled=$(( (pct * width + 99) / 100 ))
  [ "$filled" -gt "$width" ] && filled=$width
  local empty=$(( width - filled ))
  local bar=""
  local i=0
  while [ "$i" -lt "$filled" ]; do bar="${bar}█"; i=$(( i + 1 )); done
  while [ "$i" -lt "$width" ]; do bar="${bar}░"; i=$(( i + 1 )); done
  printf "%s" "$bar"
}

bar_color() {
  local pct="${1:-0}"
  if [ "$pct" -ge 90 ]; then
    printf "\033[31m"   # red
  elif [ "$pct" -ge 70 ]; then
    printf "\033[33m"   # yellow
  else
    printf "\033[32m"   # green
  fi
}

RESET="\033[0m"
DIM="\033[2m"

# --- context window ---

ctx_used=$(echo "$input" | jq -r '.context_window.used_percentage // empty')

ctx_part=""
if [ -n "$ctx_used" ]; then
  ctx_pct=$(printf "%.0f" "$ctx_used")
  ctx_bar=$(make_bar "$ctx_pct")
  ctx_color=$(bar_color "$ctx_pct")
  ctx_part=$(printf "${DIM}ctx${RESET} ${ctx_color}${ctx_bar}${RESET} ${ctx_pct}%%")
fi

# --- 5-hour rate limit ---

five_pct_raw=$(echo "$input" | jq -r '.rate_limits.five_hour.used_percentage // empty')

rate_part=""
if [ -n "$five_pct_raw" ]; then
  five_pct=$(printf "%.0f" "$five_pct_raw")
  rate_bar=$(make_bar "$five_pct")
  rate_color=$(bar_color "$five_pct")
  rate_part=$(printf "${DIM}5h${RESET}  ${rate_color}${rate_bar}${RESET} ${five_pct}%%")
fi

# --- assemble ---

if [ -n "$ctx_part" ] && [ -n "$rate_part" ]; then
  printf "%b   %b" "$ctx_part" "$rate_part"
elif [ -n "$ctx_part" ]; then
  printf "%b" "$ctx_part"
elif [ -n "$rate_part" ]; then
  printf "%b" "$rate_part"
fi
