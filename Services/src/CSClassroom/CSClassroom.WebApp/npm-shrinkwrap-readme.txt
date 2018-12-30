The jquery-multifile package currently depends on jquery 1.7.3, which requires
incompatible scripts. To fix this, npm-shrinkwrap.json is generated with the
following commands:

cd node_modules/jquery-multifile
npm i save --save --save-exact jquery@3.3.1
cd ../..
npm shrinkwrap
