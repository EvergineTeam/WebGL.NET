rm -rf website/www/
cp -rf Samples/bin/Debug/netstandard2.0/ website/www/

cd website/
npm install
npm run deploy
cd ..