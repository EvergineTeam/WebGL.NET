rm -rf website/www/
cp -rf src/Samples/bin/Debug/netstandard2.0/ website/www/
mkdir website/www/tests
cp -rf src/Tests/bin/Debug/netstandard2.0/ website/www/tests/

cd website/
npm install
npm run deploy
cd ..