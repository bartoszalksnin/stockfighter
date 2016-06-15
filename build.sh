function build(){
  mono .paket/paket.exe update
  mono packages/FAKE/tools/FAKE.exe build.fsx
}

file=".paket/paket.exe"
if [ -f "$file" ]
then
{
  build
}
else
  mono .paket/paket.bootstrapper.exe
  build
fi
