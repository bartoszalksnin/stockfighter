sh ./killDashboard.sh

function runServer(){
  cd ./build
  mono StockfighterDashboardApp.exe &
  echo $! > ../process.pid
  open "http://127.0.0.1:8083/web/dashboard.html"
}


file="./build/StockfighterDashboardApp.exe"
if [ -f "$file" ]
then
{
  echo "Starting dashboard:"
  runServer
}
else
  sh ./build.sh
  runServer
fi
