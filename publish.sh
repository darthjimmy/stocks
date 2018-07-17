sudo killall /usr/bin/dotnet
sudo dotnet publish -o /var/Conway --configuration Release
sudo systemctl restart kestrel-conway.service
