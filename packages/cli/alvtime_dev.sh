eval $(poetry env activate)
export ALVTIME_CONFIG=$(pwd)/alvtime_dev.conf
eval "$(_ALVTIME_COMPLETE=zsh_source alvtime)"
