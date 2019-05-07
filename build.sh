source scripts/common.sh

defaultProject=WebGL.NET.sln
defaultRestore=true

project=${1:-$defaultProject}
restore=${2:-$defaultRestore}
commithash=$(git rev-parse HEAD)

msbuild /restore:$restore $project -p:CommitHash=$commithash