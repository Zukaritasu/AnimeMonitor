#!/bin/bash
rm -rf dist
ncc build index.js -o dist
node --experimental-sea-config sea-config.json
cp "$(command -v node)" ndservc.exe
npx postject ndservc.exe NODE_SEA_BLOB sea-prep.blob --sentinel-fuse NODE_SEA_FUSE_fce680ab2cc467b6e072b8b5df1996b2

if [ $? -ne 0 ]; then
    echo "Error: The last command failed. Exiting with error code 1."
    exit 1
fi