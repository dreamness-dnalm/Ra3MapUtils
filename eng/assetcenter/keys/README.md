# AssetCenter public keys

This directory contains distributable public trust roots only. Never place an Ed25519 private key,
AssetCenter bearer credential, Cloudflare token, or presigned upload URL here.

The Ra3MapUtils signing private key is stored outside the repository. Local tooling resolves it from
`RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH` or the documented `%LOCALAPPDATA%` default.
