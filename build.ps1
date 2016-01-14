function build {
  .paket/paket.exe update
  packages/FAKE/tools/FAKE.exe build.fsx
}

$file=".paket/paket.exe"
if (Test-Path $file)
{
  build
}
else
{
  .paket/paket.bootstrapper.exe
  build
}
