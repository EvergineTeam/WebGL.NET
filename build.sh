source scripts/common.sh

defaultProject=WebGL.NET.sln
defaultRestore=true
defaultTreatWarningsAsErrors=true

project=${1:-$defaultProject}
restore=${2:-$defaultRestore}
treatWarningsAsErrors=${3:-$defaultTreatWarningsAsErrors}
commithash=$(git rev-parse HEAD)

msbuild /restore:$restore $project -p:CommitHash=$commithash -p:TreatWarningsAsErrors=$treatWarningsAsErrors