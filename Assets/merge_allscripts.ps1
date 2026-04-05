Get-ChildItem -Path . -Filter *.cs -Recurse | Get-Content | Out-File -Encoding utf8 PULSE_scripts.txt
