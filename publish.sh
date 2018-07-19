sudo killall /usr/bin/dotnet
sudo dotnet publish -o /var/Stocks --configuration Release
PrevDir=$(pwd)
cd /var/Stocks
sudo npm install
sudo systemctl restart kestrel-stocks.service
cd $PrevDir
unset PrevDir