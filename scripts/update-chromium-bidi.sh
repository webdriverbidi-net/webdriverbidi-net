mkdir -p .chromium-bidi
curl -sL "https://registry.npmjs.org/chromium-bidi/latest" | jq -r ".dist.tarball" | xargs curl -sL | tar -xz -C .chromium-bidi
cp .chromium-bidi/package/out/Default/gen/src/mapperTab.js third_party/chromium-bidi-mapper
rm -rf .chromium-bidi
