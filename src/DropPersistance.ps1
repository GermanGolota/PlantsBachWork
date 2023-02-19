Move-Item -Path ".\persist\certs" -Destination ".\certsTemp" -Force
Remove-Item ".\persist" -Recurse -Confirm:$false
git restore "persist"
Move-Item -Path ".\certsTemp\*" -Destination ".\persist\certs" -Force
Remove-Item ".\certsTemp" -Recurse -Confirm:$false