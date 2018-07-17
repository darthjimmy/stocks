sudo killall /usr/bin/dotnet
sudo dotnet publish -o /var/Stocks --configuration Release
sudo systemctl restart kestrel-conway.service
