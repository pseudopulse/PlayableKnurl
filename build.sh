rm -rf PlayableKnurl/bin
dotnet restore
dotnet build
cp PlayableKnurl/assetbundurl PlayableKnurl/bin/Debug/netstandard2.0
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/PlayableKnurl/BepInEx/plugins/PlayableKnurl
cp -r PlayableKnurl/bin/Debug/netstandard2.0  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/PlayableKnurl/BepInEx/plugins/PlayableKnurl

rm -rf KBuild
mkdir KBuild
cp manifest.json KBuild
cp icon.png KBuild
cp README.md KBuild
cp PlayableKnurl/bin/Debug/netstandard2.0/* KBuild
cd KBuild
rm -rf ../KB.zip
zip ../KB.zip *
cd ..