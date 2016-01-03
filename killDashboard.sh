file="./process.pid"
if [ -f "$file" ]
then
{
  echo "Stopping dashboard"
  kill -9 `cat ./process.pid`
  rm ./process.pid
}
else
	echo "Dashboard not running"
fi
